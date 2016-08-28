const co = require('co');
const xmpp = require('./xmpp');
const Element = require('node-xmpp-core').Element;

let notifyRosterUpdate = co.wrap(function*(jid) {
  let session = yield xmpp.executeAdHocCommand(xmpp.jid.domain, 'http://monitron.ddns.net/protocol/admin#notify-roster-update');
  if (session.status !== 'executing') {
    throw new Error(session.node.getText());
  }

  let data = new Element('x', {xmlns: 'jabber:x:data', type: 'submit'})
    .c('field', {type: 'hidden', var: 'FORM_TYPE'})
    .c('value')
    .t('http://monitron.ddns.net/protocol/admin#add-roster-item')
    .up()
    .up()
    .up()
    .c('field', {var: 'accountjid', type: 'jid-single'})
    .c('value').t(jid).tree();
  session = yield session.sendAction('execute', data);
  if (session.status !== 'completed') {
    throw new Error(session.node.getText());
  }
});

module.exports = {
  notifyRosterUpdate: notifyRosterUpdate
}
