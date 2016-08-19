const express = require('express');
const app = express();
const mongoose = require('mongoose');
const API = require('json-api');
const bodyParser = require('body-parser');
const multer = require('multer');
const upload = multer({ dest: 'uploads/' });
const NodePluginController = require('./controllers/node-plugins.js');
const options = require('./options.js');

const models = {
  'NodePlugin': require('./models/node-plugin.js'),
  'Account': require('./models/account.js'),
  'Session': require('./models/session.js')
};

mongoose.connect(
  options.mongo_url,
  options.mongo_options,
  (err) => {
    if (err) {
      console.log(err);
    } else {
      console.log("connected");
    }
  }
);

app.use(function(req, res, next) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
  res.header('Access-Control-Allow-Methods', 'POST, GET, PUT, DELETE, OPTIONS, PATCH');
  next();
});

const adapter = new API.dbAdapters.Mongoose(models);
const registry = new API.ResourceTypeRegistry({
  'node-plugins': {
    urlTemplates: {
      'self': '/node-plugins/{id}'
    },
    beforeRender: (resource, req, res) => {
      metadata = resource._attrs.metadata;
      resource._attrs = {
        name: metadata.name,
        description: metadata.description,
        version: metadata.version
      };
      return resource;
    }
  },
  'accounts': {
    urlTemplates: {
      'self': '/accounts/{id}'
    },
    beforeRender: (resource, req, res) => {
      resource.removeAttr("password");
      return resource;
    },
    beforeSave: (resource, req, res) => {
      if (req.method === 'PATCH') {
        if (resource.password === null || resource.password === '') {
          models.Account.findOne({
            _id: resource._id
          }, (err, account) => {
            if (err) {
              res.status(500).send(err);
              return;
            }

            resource.password = account.password;
          });
        }
      }

      return resource;
    }
  }
}, {
  'dbAdapter': adapter
});

let api_controller = new API.controllers.API(registry);
let docs_controller = new API.controllers.Documentation(registry, {name: 'Boty API'});
let front = new API.httpStrategies.Express(api_controller, docs_controller);
let request_handler = front.apiRequest.bind(front);

const port = process.env.port || 9898;

const router = express.Router();

app.use(bodyParser.json());
//app.use(bodyParser.urlencoded({ extended: true }));
app.use('/api/v1/', router);

const allowedGET = [
  'node-plugins',
  'accounts'
].join('|');
const allowedPOST = [
  'accounts'
].join('|');
const allowedPATCH = [
  'accounts'
].join('|');
const allowedDELETE = [
  'accounts'
].join('|');
router.get('/:type(' + allowedGET + ')', request_handler);
router.get('/:type('+ allowedGET + ')/:id', request_handler);
router.post('/:type(' + allowedPOST + ')', request_handler);
router.patch('/:type(' + allowedPATCH + ')/:id', request_handler);
router.delete('/:type(' + allowedDELETE + ')/:id', request_handler);
router.get('/sessions/create', (req, res) => {
  res.status(403).send("You shouldn't do that");
});
router.delete('/node-plugins/:id', NodePluginController.delete );
router.post('/node-plugins/upload', upload.single('plugin'), (req, res, next) => {
  NodePluginController.upload(req.file, (err) => {
    if (err) {
      res.status(403).send(err);
      return;
    }

    res.send("OK!");
  });
});
router.post('/sessions/create', (req, res) => {
  models.Account.findOne({
    name: req.body.username,
  }, (err, account) => {
    if (err) {
      res.status(403).send(err);
      return;
    }

    if (account === null) {
      res.status(403).send('Userame or password are incorrect');
      return;
    }

    models.Session.create({
      account: account
    }, (err, session) => {
      if (err) {
        res.status(500).send(err);
      }

      res.json({
        id_token: session._id
      });
    })
  });
});

app.listen(port, '0.0.0.0');

console.log('Listening on port ' + port);

/* vim: set sw=2 ts=2 tw=2 et : */
