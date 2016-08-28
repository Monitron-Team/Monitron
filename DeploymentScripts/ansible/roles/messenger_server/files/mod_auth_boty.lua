local http = require("socket.http")
local json = require("dkjson")
local ltn12 = require("ltn12")
local usermanager = require("core.usermanager")
local new_sasl = require("util.sasl").new

local datamanager = require("util.datamanager")
local rm_roster_push = require "core.rostermanager".roster_push;

local host = module.host
local provider = {}

local API_ROOT = "http://127.0.0.1:9898/api/v1"
local LOGIN_PATH = API_ROOT .. "/contacts/login"

local function print_tbl(tbl)
	local res = ""
	local typ = type(tbl)
	if typ == "table" then
		res = res .. "{"
		local first = true
		for k, v in pairs(tbl) do
			if (not first) then
				res = res .. ", "
			end
			first = false

			res = res .. "[" .. print_tbl(k) .. "] = " .. print_tbl(v)
		end

		res = res .. "}"
	elseif typ == "string" then
		res = res .. "\"" .. tbl .. "\""
	else
		res = res .. tostring(tbl)
	end

	return res
end

local function test_password(username, password)
	local req_body = json.encode({
		username = username,
		password = password
	})

	local _, status, _, _ = http.request{
		url = LOGIN_PATH,
		method = "POST",
		headers = {
			["content-length"] = tostring(#req_body),
			["content-type"] = "application/json"
		},
		source = ltn12.source.string(req_body)
	}


	module:log("info", "Authorizing user '%s' %s", username, ({[true]='success', [false]='fail'})[status == 200])

	return status == 200
end

local function user_exists(username)
	module:log("info", "Checking if contact %s exists", username)
	local req_body = json.encode({
		username = username,
	})

	local _, status, _, _ = http.request{
		url = API_ROOT .. "/contacts/isuser",
		method = "GET",
		headers = {
			["content-length"] = tostring(#req_body),
			["content-type"] = "application/json"
		},
		source = ltn12.source.string(req_body)
	}

	return status == 200
end

local function _roster_get(username)
	local t = {}
	local _, status = http.request{
		url = API_ROOT .. "/contacts/roster/" .. username,
		sink = ltn12.sink.table(t),
		method = "GET",
		headers = {
			Accept = "application/json",
		}
	}

	if (status ~= 200) then
		return {}
	else
		return json.decode(t[1])
	end
end

local function roster_load(username)
	username = username
	local raw_roster = _roster_get(username)
	local roster = {
		[false] = {version = true}
	}

	for _, item in ipairs(raw_roster) do
		if item.alias == json.null then
			item.alias = nil
		end

		roster[item.jid] = {
			subscription = "both",
			name = item.alias,
			groups = {}
		}

		for _, group in ipairs(item.groups or {}) do
			roster[item.jid].groups[group] = true
		end
	end

	return roster
end

function provider.test_password(username, password)
	return test_password(username .. "@" .. host, password);
end

function provider.set_password(username, password)
	return nil, "Account modification not available."
end

function provider.user_exists(username)
	return user_exists(username .. "@" .. host);
end

function provider.create_user(username, password)
	return nil, "Account creation/modification not available."
end

function provider.get_sasl_handler()
	local testpass_authentication_profile = {
		plain_test = function(sasl, username, password, realm)
			return usermanager.test_password(username, realm, password), true;
		end,
	};
	return new_sasl(host, testpass_authentication_profile);
end

local function replace_contacts(username, host, roster)
	for k, _ in pairs(roster) do
		roster[k] = nil
	end

	module:log("info", "Fetching roster for '%s@%s'", username, host)
	for k, v in pairs(roster_load(username .. "@" .. host)) do
		roster[k] = v
	end

	roster[false].version = true

	module:log("debug", "Loaded roster for '%s@%s'", username, host)
end

local function cancel_roster_save(username, host, datastore, data)
	if datastore == "roster" then
		module:log("Clearing roster before saving")
		return false
	end
end

local function intersect(a, b)
	local res = {}
	for k, _ in pairs(a) do
		if b[k] == nil then
			res[k] = true
		end
	end
	for k, _ in pairs(b) do
		if a[k] == nil then
			res[k] = true
		end
	end

	return res;
end

local function handle_external_update(username, host, roster)
	module:log("info",
		"Updating roster for '%s@%s' due to external change",
		username,
		host)

	local real_roster = roster_load(username .. "@" .. host)
	local changed = {}
	for k, v in pairs(real_roster) do
		if
			roster[k] == nil
			or roster[k].name ~= real_roster[k].name
			or intersect(real_roster[k].groups or {}, roster[k].groups or {})
		then
			roster[k] = v
			changed[#changed + 1] = k
		end
	end

	for k, _ in pairs(roster) do
		if real_roster[k] == nil then
			roster[k] = nil
			changed[#changed + 1] = k
		end
	end

	module:log("debug", "changed entries for %s@%s: %s", username, host, print_tbl(changed))
	for _, itemjid in ipairs(changed) do
		if itemjid ~= false then
			rm_roster_push(username, host, itemjid)
		end
	end
end

function module.load()
	module:log("Registering hooks")
	module:hook("roster-load", replace_contacts);
	module:hook("roster-external-update", handle_external_update);
	datamanager.add_callback(cancel_roster_save);
end

module:provides("auth", provider);
