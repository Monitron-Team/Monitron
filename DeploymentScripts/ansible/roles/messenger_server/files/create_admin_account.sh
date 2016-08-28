#!/bin/env sh
mongo monitron << EOF
db.accounts.update(
  {
    name: 'admin'
  },
  {
    name: 'admin',
    password: '$1',
    isAdmin: true,
  },
  {
    upsert: true,
    multi: false
  }
);
EOF
exit $?
# vim: set tabstop=2 shiftwidth=2 expandtab: :
