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
  GenericController({
    plural: 'serials',
    singular: 'serial'
  }, Serial, {
    list: true,
    show: true,
    create: true,
    update: true,
    delete: true
  }
  )(router);
};
