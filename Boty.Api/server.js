const log = require('./log')
const co = require('co');
const express = require('express');
const app = express();
const mongoose = require('mongoose');
const bodyParser = require('body-parser');
const options = require('./options.js');
const Session = require('./models/session');
const Account = require('./models/account');

mongoose.Promise = global.Promise

mongoose.connect(
  options.mongo_url,
  options.mongo_options,
  (err) => {
    if (err) {
      log.error(err);
    } else {
      log.info("connected");
    }
  }
);

app.use(function(req, res, next) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
  res.header('Access-Control-Allow-Methods', 'POST, GET, PUT, DELETE, OPTIONS');
  next();
});

const port = process.env.port || 9898;

const router = express.Router();

app.use(bodyParser.json());
//app.use(bodyParser.urlencoded({ extended: true }));
router.use('*', (req, res, next) => {
  log.info('Got request for %s %s', req.method, req.originalUrl);
  try {
    next();
  } catch (e) {
    log.error(e)
    throw e;
  }
});

router.use('*', co.wrap(function*(req, res, next) {
  if (req.method === 'OPTIONS') {
    next();
    return;
  }

  req.auth = {
    isAuthorized: false,
    account: null
  };

  if (req.headers.authorization) {
    let token = req.headers.authorization.split(/ /)[1];
    try {
      let session = yield Session.findOne({_id: token});
      if (session !== null) {
        req.auth.account = yield Account.findOne({_id: session.account});
        if (req.auth.account !== null) {
          req.auth.isAuthorized = true;
        }
      }
    } catch (e) {
      log.error(e);
    }
  }

  next();
}));

app.use('/api/v1/', router);


const CONTROLLERS = [
  'accounts',
  'sessions',
  'node-plugins',
  'contacts'
];

for(let i in CONTROLLERS) {
  require('./controllers/' + CONTROLLERS[i])(router);
}

app.listen(port, '0.0.0.0');

log.info('Listening on port ' + port);

/* vim: set sw=2 ts=2 tw=2 et : */
