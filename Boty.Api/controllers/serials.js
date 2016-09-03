const log = require('../log');
const co = require('co');
const Serial = require('../models/serial');
const Contact = require('../models/contact');
const GenericController = require('./generic');

module.exports = (router) => {
  router.post('/serials/:id/unpair', co.wrap(function* (req, res) {
    if (!req.auth.isAuthorized) {
      res.status(403).send({errors: [{code: 403, msg: 'Must be authenticated to unpair'}]});
      return;
    }

    let serialNum = req.params.id;
    let newOwner = req.auth.account;
    log.info('Trying to unpair serial "%s" with "%s"', serialNum, newOwner.name);
    let serial = yield Serial.findOne({serial_key: serialNum});
    if (!serial) {
      res.status(404).send({errors: [{code:404, msg: 'Serial not found'}]});
      return;
    }

    if (!serial.isPaired) {
      res.status(403).send({errors: [{code: 403, msg: 'Device not paired'}]});
      return;
    }

    serial.isPaired = false;

    log.debug('Serial found, looking for matching contact');
    let contact = yield Contact.findOne({_id: serial.contact});
    if (!contact) {
      res.status(404).send({errors: [{code: 404, msg: 'Serial expired'}]});
      return;
    }

    log.debug('Contact found, unpairing...');
    contact.owner = serial.maker;
    contact.roster = [];
    contact.description = '';
    yield contact.save();
    yield serial.save();

    res.status(200).send(serial.toObject());
  }));
  router.post('/serials/:id/pair', co.wrap(function* (req, res) {
    if (!req.auth.isAuthorized) {
      res.status(403).send({errors: [{code: 403, msg: 'Must be authenticated to pair'}]});
      return;
    }

    let serialNum = req.params.id;
    let newOwner = req.auth.account;
    log.info('Trying to pair serial "%s" with "%s"', serialNum, newOwner.name);
    let serial = yield Serial.findOne({serial_key: serialNum});
    if (!serial) {
      res.status(404).send({errors: [{code:404, msg: 'Serial not found'}]});
      return;
    }

    if (serial.isPaired) {
      res.status(403).send({errors: [{code: 403, msg: 'Device already paired'}]});
      return;
    }

    serial.isPaired = true;

    log.debug('Serial found, looking for matching contact');
    let contact = yield Contact.findOne({_id: serial.contact});
    if (!contact) {
      res.status(404).send({errors: [{code: 404, msg: 'Serial expired'}]});
      return;
    }

    log.debug('Contact found, pairing...');
    contact.owner = newOwner._id;
    yield contact.save();
    yield serial.save();

    res.status(200).send(serial.toObject());
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
    beforeList: (query, req, res) => {
      if (!req.auth.isAuthorized) {
        return {maker: null};
      } else if (query.serial_key) {
        return {serial_key: query.serial_key};
      } else if (query.contact) {
        return {contact: query.contact};
      } else {
        query.maker = req.auth.account.id;
        return query;
      }
    },
    beforeSave: co.wrap(function* (serial, req, res) {
      if (!req.auth.isAuthorized) {
        return Promise.resolve(null);
      }

      serial.maker = req.auth.account.id;

      return serial;
    }),
  }
  )(router);
};
