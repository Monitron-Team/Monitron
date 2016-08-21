const co = require('co');
const Account = require('../models/account');
const GenericController = require('./generic');

module.exports = GenericController({
  plural: 'accounts',
  singular: 'account'
}, Account, {
  list: true,
  show: true,
  create: true,
  update: true,
  delete: true
},{
  beforeRender: (account, req, res) => {
    delete account['password'];
    return account;
  },
  beforeSave: co.wrap(function* (account, req, res) {
    if (req.method === 'PUT') {
      if (account.password === undefined || account.password === null) {
        try {
          let origAccount = yield Account.findOne({_id: account.id}).exec();
          account.password = origAccount.password;
        } catch(e) {
          return null;
        }
      }
    }

    return account;
  })
});
