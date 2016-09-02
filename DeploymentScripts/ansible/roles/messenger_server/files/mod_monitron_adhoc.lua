-- Copyright (C) 2016-2016 Saggi Mizrahi
--
-- This file is MIT/X11 licensed. Please see the
-- COPYING file in the source package for more information.
--

local _G = _G;

local prosody = _G.prosody;
local hosts = prosody.hosts;
local t_concat = table.concat;

local module_host = module:get_host();

local st = require "util.stanza";
local jid_bare = require "util.jid".bare;
local jid_prep = require "util.jid".prep;
local config = require "core.configmanager";
local usermanager_user_exists = require "core.usermanager".user_exists;
local usermanager_create_user = require "core.usermanager".create_user;
local usermanager_delete_user = require "core.usermanager".delete_user;
local usermanager_get_password = require "core.usermanager".get_password;
local usermanager_set_password = require "core.usermanager".set_password;
local rm_load_roster = require "core.rostermanager".load_roster;
local rm_remove_from_roster = require "core.rostermanager".remove_from_roster;
local rm_save_roster = require "core.rostermanager".save_roster;
local rm_roster_push = require "core.rostermanager".roster_push;
local dataforms_new = require "util.dataforms".new;

local MONITRON_COMMAND_PATH = "http://monitron.ddns.net/protocol/admin#"

function is_monitron_admin(jid, host)
        if host and not hosts[host] then return false; end
        if type(jid) ~= "string" then return false; end

        local is_admin;
        jid = jid_bare(jid);
        host = host or "*";

        local host_admins = config.get(host, "monitron_admins");
        local global_admins = config.get("*", "moitron_admins");

        if host_admins and host_admins ~= global_admins then
                if type(host_admins) == "table" then
                        for _,admin in ipairs(host_admins) do
                                if jid_prep(admin) == jid then
                                        is_admin = true;
                                        break;
                                end
                        end
                elseif host_admins then
                        log("error", "Option 'monitron_admins' for host '%s' is not a list", host);
                end
        end

        if not is_admin and global_admins then
                if type(global_admins) == "table" then
                        for _,admin in ipairs(global_admins) do
                                if jid_prep(admin) == jid then
                                        is_admin = true;
                                        break;
                                end
                        end
                elseif global_admins then
                        log("error", "Global option 'monitron_admins' is not a list");
                end
        end

        return is_admin or false;
end

local function adhoc_simple(form, result_handler)
    return function(self, data, state)
        if not is_monitron_admin(data.from, data.to) then
            return { status = "completed", error = { type = "auth", command = "forbidden", message = "You don't have permission to execute this command" } }
        end

        if state then
            if data.action == "cancel" then
                return { status = "canceled" };
            end
            local fields, err = form:data(data.form);
            return result_handler(fields, err, data);
        else
            return { status = "executing", actions = {"next", "complete", default = "complete"}, form = form }, "executing";
        end
    end
end


module:depends("adhoc");
local adhoc_new = module:require "adhoc".new;

local function generate_error_message(errors)
        local errmsg = {};
        for name, err in pairs(errors) do
                errmsg[#errmsg + 1] = name .. ": " .. err;
        end
        return { status = "completed", error = { message = t_concat(errmsg, "\n") } };
end

-- Adding a new user
local add_user_layout = dataforms_new{
    title = "Adding a User";
    instructions = "Fill out this form to add a user.";

    { name = "FORM_TYPE", type = "hidden", value = MONITRON_COMMAND_PATH.."add-user" };
    { name = "accountjid", type = "jid-single", required = true, label = "The Jabber ID for the account to be added" };
    { name = "password", type = "text-private", label = "The password for this account" };
};

local add_user_command_handler = adhoc_simple(add_user_layout, function(fields, err)
        if err then
            return generate_error_message(err);
        end

        local username, host, resource = jid.split(fields.accountjid);
        if module_host ~= host then
            return { status = "completed", error = { message = "Trying to add a user on " .. host .. " but command was sent to " .. module_host}};
        end

        if fields["password"] and username and host then
            if usermanager_user_exists(username, host) then
                return { status = "completed", error = { message = "Account already exists" } };
            else
                if usermanager_create_user(username, fields.password, host) then
                    module:log("info", "Created new account %s@%s", username, host);
                    return { status = "completed", info = "Account successfully created" };
                else
                    return { status = "completed", error = { message = "Failed to write data to disk" } };
                end
            end
        else
            module:log("debug", "Invalid data, password mismatch or empty username while creating account for %s", fields.accountjid or "<nil>");
            return { status = "completed", error = { message = "Invalid data.\nPassword mismatch, or empty username" } };
        end
end);

-- Deleting a user's account
local delete_user_layout = dataforms_new{
    title = "Deleting a User";
    instructions = "Fill out this form to delete a user.";

    { name = "FORM_TYPE", type = "hidden", value = MONITRON_COMMAND_PATH.."delete-user"};
    { name = "accountjids", type = "jid-multi", required = true, label = "The Jabber ID(s) to delete" };
};

local delete_user_command_handler = adhoc_simple(delete_user_layout, function(fields, err)
    if err then
        return generate_error_message(err);
    end

    local failed = {};
    local succeeded = {};
    for _, aJID in ipairs(fields.accountjids) do
        local username, host, resource = jid.split(aJID);
        if (host == module_host) and usermanager_user_exists(username, host) and usermanager_delete_user(username, host) then
            module:log("debug", "User %s has been deleted", aJID);
            succeeded[#succeeded+1] = aJID;
        else
            module:log("debug", "Tried to delete non-existant user %s", aJID);
            failed[#failed+1] = aJID;
        end
    end

    return {
        status = "completed",
        info = (#succeeded ~= 0
            and "The following accounts were successfully deleted:\n"..t_concat(succeeded, "\n").."\n" or "")..
            (#failed ~= 0 and "The following accounts could not be deleted:\n"..t_concat(failed, "\n") or "") };
end);

-- Adding an item to a user's roster
local add_roster_item_layout = dataforms_new{
    title = "Adding a Roster Item";
    instructions = "Fill out this form to add an item to a user's roster.";

    { name = "FORM_TYPE", type = "hidden", value = MONITRON_COMMAND_PATH.."add-roster-item"};
    { name = "accountjid", type = "jid-single", required = true, label = "The Jabber ID for the roster owner" };
    { name = "itemjid", type = "jid-single", required = true, label = "The Jabber ID for the item to add" };
    { name = "groups", type = "text-multi", required = true, label = "The groups to put the item in" };
};

local add_roster_item_command_handler = adhoc_simple(add_roster_item_layout, function(fields, err)
    if err then
        return generate_error_message(err);
    end
    local username, host, resource = jid.split(fields.accountjid)
    local itemjid = jid_prep(fields.itemjid)
    local host_obj = hosts[host]
    local sessions = host_obj and host_obj.sessions
    local session =  sessions and sessions[username]
    local roster = nil
    local error = nil
    local info = nil
    module:log("info", "Got request to add %s to roster %s", fields.accountjid, itemjid);

    if session ~= nil then
        roster = session.roster
    else
    	roster = rm_load_roster(username, host)
    end

    local groups = {}
    for group in fields.groups:gmatch("[^\n]+") do
    	groups[group] = true
    end
    old_item = roster[itemjid]
    roster[itemjid] = {
    	subscription = "both",
    	groups = groups
    }

    local success, err_type, err_cond, err_msg = rm_save_roster(username, host, roster)

    if success then
	rm_roster_push(username, host, itemjid);
    	info = "Roster item added successfully"
    else
    	roster[itemjid] = old_item
    	error = { message = "("..err_type..") "..err_cond..": "..err_msg }
    end

    return {
    	status = "completed",
    	error = error,
    	info = info
    }
end);

-- Deleting an item to a user's roster
local delete_roster_item_layout = dataforms_new{
    title = "Deleting a Roster Item";
    instructions = "Fill out this form to delete an item to a user's roster.";

    { name = "FORM_TYPE", type = "hidden", value = MONITRON_COMMAND_PATH.."add-roster-item"};
    { name = "accountjid", type = "jid-single", required = true, label = "The Jabber ID for the roster owner" };
    { name = "itemjid", type = "jid-single", required = true, label = "The Jabber ID for the item to delete" };
};

local delete_roster_item_command_handler = adhoc_simple(delete_roster_item_layout, function(fields, err)
    if err then
        return generate_error_message(err);
    end

    local username, host, resource = jid.split(fields.accountjid)
    local itemjid = jid_prep(fields.itemjid)
    local session = hosts[host].sessions[username]
    local roster = nil
    local error = nil
    local info = nil
    module:log("info", "Got request to delete %s from roster %s", fields.accountjid, itemjid);

    if session ~= nil then
        roster = session.roster
    else
    	roster = rm_load_roster(username, host)
    end

    old_item = roster[itemjid]
    roster[itemjid] = nil

    local success, err_type, err_cond, err_msg = rm_save_roster(username, host, roster)

    if success then
    	info = "Roster item deleted successfully"
	rm_roster_push(username, host, itemjid);
    else
    	roster[itemjid] = old_item
    	error = { message = "("..err_type..") "..err_cond..": "..err_msg }
    end

    return {
    	status = "completed",
    	error = error,
    	info = info
    }
end);

-- Notify a roster update
local notify_roster_update_item_layout = dataforms_new{
    title = "Notify a Roster Update";
    instructions = "Fill out this form to notify an update of a user's roster.";

    { name = "FORM_TYPE", type = "hidden", value = MONITRON_COMMAND_PATH.."add-roster-item"};
    { name = "accountjid", type = "jid-single", required = true, label = "The Jabber ID for the roster owner" };
};

local notify_roster_update_command_handler = adhoc_simple(notify_roster_update_item_layout, function(fields, err)
    if err then
        return generate_error_message(err);
    end

    local username, host, _ = jid.split(fields.accountjid)
    module:log("info", "Got request to update the roster of %s", fields.accountjid);
    local session = hosts[host].sessions[username]
    local error = nil

    if session == nil then
	module:log("debug", "User %s not logged in, no reason to update", fields.accountjid);
	return {
		status = "completed",
		info = "User not logged in, no action taken"
	};
    end

    local roster = session.roster;

    hosts[host].events.fire_event('roster-external-update', username, host, roster);

    return {
    	status = "completed",
    	error = error,
    	info = "Roster update notifiaction send"
    }
end);

local add_user_desc = adhoc_new("Add User", MONITRON_COMMAND_PATH.."add-user", add_user_command_handler, "monitron_admin");
local delete_user_desc = adhoc_new("Delete User", MONITRON_COMMAND_PATH.."delete-user", delete_user_command_handler, "monitron_admin");
local add_roster_item_desc = adhoc_new("Add Roster Item", MONITRON_COMMAND_PATH.."add-roster-item", add_roster_item_command_handler, "monitron_admin");
local delete_roster_item_desc = adhoc_new("Delete Roster Item", MONITRON_COMMAND_PATH.."delete-roster-item", delete_roster_item_command_handler, "monitron_admin");
local notify_roster_update_desc = adhoc_new("Notify Roster Update", MONITRON_COMMAND_PATH.."notify-roster-update", notify_roster_update_command_handler, "monitron_admin");

module:provides("adhoc", add_user_desc);
module:provides("adhoc", delete_user_desc);
module:provides("adhoc", add_roster_item_desc);
module:provides("adhoc", delete_roster_item_desc);
module:provides("adhoc", notify_roster_update_desc);

