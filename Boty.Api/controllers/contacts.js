const log = require('../log');
const co = require('co');
const Contact = require('../models/contact');
const GenericController = require('./generic');
const adhoc = require('../adhoc');
const NetBot = require('../models/netbot');

let isOwnerAuth = function(contact, req) {
  let ownerId = contact.owner.toString();
  let authId = req.auth.account.id;
  return ownerId === authId;
};

function _generatePassword() {
  return Math.floor((1 + Math.random()) * 0x10000)
    .toString(16)
    .substring(1);
}

module.exports = (router) => {
  router.use('/contacts', co.wrap(function*(req, res, next) {
    if (req.path !== '/login' &&  req.path !== '/isuser' && !req.path.startsWith('/roster') && req.method !== 'OPTIONS' && !req.auth.isAuthorized) {
      log.debug('Rejected contact related request: %s', req.path);
      res.status(403).send({errors: [{code: 403, msg: 'Unauthorized access'}]});
    } else {
      next();
    }
  }));
  router.get('/contacts/isuser', co.wrap(function* (req, res) {
    let username = req.body.username;
    log.debug('Got user check request for `%s`', username);
    if (!username) {
      res.status(403).send('Problem with authentication information');
      return;
    }

    let result = yield Contact.findOne({jid: username});
    if (result === null) {
      res.status(404).send('User not found');
      return;
    }

    res.send('OK');
  }));
  router.post('/contacts/login', co.wrap(function* (req, res) {
    let username = req.body.username;
    let password = req.body.password;
    log.debug('Got auth request for `%s`', username);
    if ((!username) || (!password)) {
      res.status(403).send('Problem with authentication information');
      return;
    }

    let result = yield Contact.findOne({jid: username, password: password});
    if (result === null) {
      log.debug('Auth request failed for `%s`', username);
      res.status(403).send('Problem with authentication information');
      return;
    }

    log.debug('Auth request succeeded for `%s`', username);

    res.send('OK');
  }));
  router.get('/contacts/roster/:username', co.wrap(function* (req, res) {
    let username = req.params.username;
    if (!username) {
      res.status(404).send('Invalid user name');
      return;
    }

    let result = yield Contact.findOne({jid: username});
    if (result === null) {
      res.status(404).send('User not found');
      return;
    }

    let resultObj = result.toObject();
    res.send(resultObj.roster);
  }));
  GenericController({
    plural: 'contacts',
    singular: 'contact'
  }, Contact, {
    list: true,
    show: true,
    create: true,
    update: true,
    delete: true
  },{
    beforeList: (query, req, res) => {
      if (!req.auth.isAuthorized) {
        return {owner: null};
      } else {
        return {owner: req.auth.account.id};
      }
    },
    beforeRender: (contact, req, res) => {
      for(let i in contact.roster) {
        let item = contact.roster[i];
        item._id = contact.id + '>' + item.jid;
      }

      delete contact.password;

      return contact;
    },
    beforeSave: co.wrap(function* (contact, req, res) {
      if (!req.auth.isAuthorized) {
        return Promise.resolve(null);
      }
      if (req.method === 'PUT') {
        if (contact.password === undefined || contact.password === null) {
          try {
            let origContact = yield Contact.findOne({_id: contact.id}).exec();
            contact.password = origContact.password;
          } catch(e) {
            return Promise.resolve(null);
          }
        }

      } else {
        if (contact.kind === 'netbot') {
          contact.password = _generatePassword();
        }
      }

      contact.owner = req.auth.account.id;
      contact.updatedAt = Date.now();

      return contact;
    }),
    afterSave: function(contact, req, res) {
      log.debug(
        'Updating xmpp server about roster change for: %s', contact.jid);
      adhoc.notifyRosterUpdate(contact.jid);
      return Promise.resolve();
    },
    beforeDelete: co.wrap(function* (id, req, res) {
      let contact = yield Contact.findOne({_id: id});
      if (contact) {
        let updatedContacts = yield Contact.find({roster: {$elemMatch: {jid: contact.jid}}});
        yield Contact.update({}, {$pull: {roster: {jid: contact.jid}}}, {multi: true});
        for (let i = 0; i < updatedContacts.length; i++) {
          adhoc.notifyRosterUpdate(updatedContacts[i].jid);
        }
      }

      return true;
    }),
    afterDelete: co.wrap(function* (id, req, res) {
      yield NetBot.remove({contact: id});
      adhoc.notifyNetbotUpdate();
    })
  }
  )(router);
};
