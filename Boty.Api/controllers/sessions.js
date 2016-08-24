const Account = require('../models/account');
const Session = require('../models/session');

module.exports = (router) => {
  router.get('/sessions/create', (req, res) => {
    res.status(403).send('You shouldn\'t do that');
  });
  router.post('/sessions/create', (req, res) => {
    Account.findOne({
      name: req.body.username,
      password: req.body.password
    }, (err, account) => {
      if (err) {
        res.status(403).send(err);
        return;
      }

      if (account === null) {
        res.status(403).send('Userame or password are incorrect');
        return;
      }

      Session.create({
        account: account
      }, (err, session) => {
        if (err) {
          res.status(500).send(err);
        }

        res.json({
          account: account._id,
          is_admin: account.isAdmin,
          id_token: session._id
        });
      });
    });
  });
};
