const log = require('../log');
const co = require('co');
const Serial = require('../models/serial');
const Contact = require('../models/contact');
const GenericController = require('./generic');
const adhoc = require('../adhoc');

let isOwnerAuth = function(contact, req) {
  let ownerId = contact.owner.toString();
  let authId = req.auth.account.id;
  return ownerId === authId;
};

module.exports = (router) => {
  router.use('/serials', co.wrap(function*(req, res, next) {
    if (req.path !== '/login' &&  req.path !== '/isuser' && !req.path.startsWith('/roster') && req.method !== 'OPTIONS' && !req.auth.isAuthorized) {
      log.debug('Rejected contact related request: %s', req.path);
      res.status(403).send({errors: [{code: 403, msg: 'Unauthorized access'}]});
    } else {
      next();
    }
  }));
  router.get('/serials/isuser', co.wrap(function* (req, res) {
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
    plural: 'serials',
    singular: 'serial'
  }, Serial, {
    list: true,
    show: true,
    create: true,
    update: true,
    delete: true
  },{
    beforeRender: (serial, req, res) => {
      
      return serial;
    },
    beforeSave: co.wrap(function* (serial, req, res) {
      return serial;
    }),
    afterSave: function(serial, req, res) {
      return Promise.resolve();
    }
  }
  )(router);
};
