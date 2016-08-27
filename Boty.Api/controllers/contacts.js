const log = require('../log');
const co = require('co');
const Contact = require('../models/contact');
const GenericController = require('./generic');

let isOwnerAuth = function(contact, req) {
  let ownerId = contact.owner.toString();
  let authId = req.auth.account.id;
  return ownerId === authId;
};

module.exports = (router) => {
  router.use('/contacts', co.wrap(function*(req, res, next) {
    if (req.path !== '/login' &&  req.path !== '/isuser' && !req.path.startsWith('/roster') && req.method !== 'OPTIONS' && !req.auth.isAuthorized) {
      res.status(403).send({errors: [{code: 403, msg: 'Unauthorized access'}]});
    } else {
      next();
    }
  }));
  router.get('/contacts/isuser', co.wrap(function* (req, res) {
    let username = req.body.username;
    if (!username) {
      res.status(403).send('Problem with authentication information');
      return;
    }

    let result = yield Contact.findOne({jid: username});
    if (result === null) {
      res.status(404).send('Problem with authentication information');
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
    console.log(resultObj.roster);
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
    beforeRender: (contact, req, res) => {
      if (!isOwnerAuth(contact, req)) {
        return Promise.resolve(null);
      }

      for(let i in contact.roster) {
        let item = contact.roster[i];
        item._id = item.jid.name + '@' + item.jid.domain;
      }

      delete contact.password

      return contact;
    },
    beforeSave: co.wrap(function* (contact, req, res) {
      if (req.method === 'PUT') {
        if (contact.password === undefined || contact.password === null) {
          try {
            let origContact = yield Contact.findOne({_id: contact.id}).exec();
            contact.password = origContact.password;
          } catch(e) {
            return Promise.resolve(null);
          }
        }
      }

      if (contact.roster.length > 0) {
        contact.jid = contact.jid.name + '@' + contact.jid.domain;
      }

      contact.updatedAt = Date.now;

      return contact;
    })
  })(router);
};
