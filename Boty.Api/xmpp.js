const co = require('co');
const log = require('./log');
const XmppClient = require('node-xmpp-client');
const Stanza = require('node-xmpp-core').Stanza;
const Element = require('node-xmpp-core').Element;

function _generateId() {
  return Math.floor((1 + Math.random()) * 0x10000)
    .toString(16)
    .substring(1);
}

let pendingRequests = {};

let client = new XmppClient({
  autostart: false
});
client._origConnect = client.connect;
client.connect = function(options) {
  this.options = options;

  this.on('error', (err) => {
    log.error('XMPP error: %s', err);
  });

  this.on('online', () => {
    log.info('Connected to XMPP server');
  });

  this.on('offline', () => {
    log.warn('XMPP client offline, reconnecting');
  });

  this.on('stanza', (stanza) => {
    if (stanza.name === 'iq') {
      let waiter = pendingRequests[stanza.id];
      if (waiter === undefined) {
        log.warn('Got iq stanza that no one is waiting for: %s', stanza.toString());
      } else {
        delete pendingRequests[stanza.id];
        log.debug('Got response for iq `%s`', stanza.id);
        if (stanza.type === 'error') {
          waiter.reject(stanza);
        } else {
          waiter.resolve(stanza);
        }
      }
    }
  });

  log.info('Connecting to XMPP server');
  this._origConnect();
};

client.iqRequest = function(iq, element) {
  if (!iq.id) {
    iq.id = 'nodejs' + _generateId();
  }

  let stanza = new Stanza('iq', iq).cnode(element);
  let promise = new Promise(function(resolve, reject) {
    pendingRequests[iq.id] = {
      resolve: resolve,
      reject: reject
    };
  });
  log.debug('Sent iq `%s`', iq.id);
  this.send(stanza);
  return promise;
};

function Session(client, stanza) {
  let command = stanza.getChild(
    'command',
    'http://jabber.org/protocol/commands');
  this.data = command.getChild('x', 'jabber:x:data');
  this.note = command.getChild('note');
  this.id = command.attrs.sessionid;
  this.node = command.attrs.node;
  this.to = stanza.attrs.from;
  this.status = command.attrs.status;
  this.client = client;
}

Session.prototype.sendAction = co.wrap(function*(action, data) {
  let resp = yield this.client.iqRequest(
    {
      to: this.to,
      type: 'set'
    }, new Element('command', {
      xmlns: 'http://jabber.org/protocol/commands',
      sessionid: this.id,
      node: this.node,
      action: action
    }).cnode(data).tree()
  );

  return new Session(this.client, resp);
});

client.executeAdHocCommand = co.wrap(function*(jid, node) {
  let resp = yield this.iqRequest(
    {
      to: jid,
      type: 'set'
    }, new Element('command', {
      xmlns: 'http://jabber.org/protocol/commands',
      node: node,
      action: 'execute'
    })
  );

  return new Session(this, resp);
});

module.exports = client;
