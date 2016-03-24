#!/bin/env bash
username="$1"
password="$2"
echo -e $password\\n$password | prosodyctl passwd "$1"
if [ $? -ne 0 ]; then
	echo User did not exist, creating new user
	echo -e $password\\n$password | prosodyctl adduser "$1"
fi

