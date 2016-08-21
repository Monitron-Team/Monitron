const co = require('co');
const Contact = require('../models/contact');
const GenericController = require('./generic');

let isOwnerAuth = function(contact, req) {
  let ownerId = contact.owner.toString();
  let authId = req.auth.account._id.toString();
  return ownerId === authId;
};

module.exports = (router) => {
  router.use('/contacts', co.wrap(function*(req, res, next) {
    if (req.method !== 'OPTIONS' && !req.auth.isAuthorized) {
      res.status(403).send({errors: [{code: 403, msg: 'Unauthorized access'}]});
    } else {
      next();
    }
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
        return null;
      }

      return contact;
    },
    beforeSave: co.wrap(function* (contact, req, res) {
      if (req.method === 'PUT') {
        if (contact.password === undefined || contact.password === null) {
          try {
            let origContact = yield Contact.findOne({_id: contact.id}).exec();
            contact.password = origContact.password;
          } catch(e) {
            return null;
          }
        }
      }

      return contact;
    })
  })(router);
};
