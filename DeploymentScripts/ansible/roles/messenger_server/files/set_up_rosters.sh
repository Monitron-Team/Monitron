#!/bin/env sh
mongo monitron << EOF
var admin = db.accounts.findOne({name: 'admin'});
var contacts = db.contacts.find({owner: admin._id});
var roster = [];
while (contacts.hasNext()) {
  var contact = contacts.next();
  roster.push({jid: contact.jid});
}
db.contacts.update(
  {
    owner: admin._id
  },
  {
    \$set: {
      roster: roster
    }
  },
  {
    multi: true
  }
);
EOF
exit $?
# vim: set tabstop=2 shiftwidth=2 expandtab: :
