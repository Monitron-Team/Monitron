-- Section for VirtualHost example.com

VirtualHost "{{domain_name}}"
	enabled = true -- Remove this line to enable this host

	-- Assign this host a certificate for TLS, otherwise it would use the one
	-- set in the global section (if any).
	-- Note that old-style SSL on port 5223 only supports one certificate, and will always
	-- use the global one.
        -- {% if prosody_ssl %}

	ssl = {
	    key = "/etc/pki/prosody/{{domain_name}}.key";
	    certificate = "/etc/pki/prosody/{{domain_name}}.crt";
	}
        -- {% endif %}

	monitron_admins = {
	    "monitron_admin@{{domain_name}}";
	    "monitron_test_admin@{{domain_name}}";
	}

	modules_enabled = {
	    "monitron_adhoc";
            "admin_adhoc";
	}

------ Components ------
-- You can specify components to add hosts that provide special services,
-- like multi-user conferences, and transports.
-- For more information on components, see http://prosody.im/doc/components

---Set up a MUC (multi-user chat) room server on conference.example.com:
--Component "conference.example.com" "muc"

-- Set up a SOCKS5 bytestream proxy for server-proxied file transfers:
--Component "proxy.example.com" "proxy65"

---Set up an external component (default component port is 5347)
--
-- External components allow adding various services, such as gateways/
-- transports to other networks like ICQ, MSN and Yahoo. For more info
-- see: http://prosody.im/doc/components#adding_an_external_component
--
--Component "gateway.example.com"
--	component_secret = "password"
