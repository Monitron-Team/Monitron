#!/bin/env sh
mongo monitron << EOF
var admin = db.accounts.findOne({name: 'admin'});
db.contacts.update(
  {
    jid: '$1'
  },
  {
    jid: '$1',
    password: '$2',
    description: '$3',
    owner: admin._id,
    kind: 'user'
  },
  {
    upsert: true,
    multi: false
  }
);
EOF
exit $?
# vim: set tabstop=2 shiftwidth=2 expandtab: :
