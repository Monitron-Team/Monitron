#!/bin/env sh
mongo monitron << EOF
db.dropUser("monitron_mgmt")
db.createUser({
    "user": "monitron_mgmt",
    "pwd": "$1",
    "roles": [ "readWrite" ]
})
EOF
