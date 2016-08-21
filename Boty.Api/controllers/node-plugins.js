const fs = require('fs');
const path = require('path');
const tar = require('tar-stream');
const xmlParseString = require('xml2js').parseString;
const mongoose = require('mongoose');
const conn = mongoose.connection;
const Grid = require('gridfs-stream');
const BUCKET = 'plugin_store';
const multer = require('multer');
const upload = multer({ dest: 'uploads/' });
const GenericController = require('./generic');
const NodePlugin = require('../models/node-plugin');

/* vim: set sw=2 ts=2 tw=2 et : */
function uploadNodePlugin(file, callback) {
  let extract = tar.extract();
  let tarFile = fs.createReadStream(file.path);
  extract.on('entry', (header, stream, cb) => {
    if (path.parse(header.name).base === 'MANIFEST.xml') {
      let rawXml = '';
      stream.on('data', (chunk) => {
        rawXml += chunk;
      });
      stream.on('end', () => {
        tarFile.close();
        let gfs = Grid(conn.db, mongoose.mongo);
        xmlParseString(rawXml, (err, res) => {
          let manifest = res.PluginManifest;
          let stream = gfs.createWriteStream({
            filename: manifest.Id[0],
            root: BUCKET,
            metadata: {
              id: manifest.Id[0],
              description: manifest.Description[0],
              dll_name: manifest.DllName[0],
              name: manifest.Name[0],
              type: manifest.Type[0],
              version: manifest.Version[0],
              dll_path: path.parse(header.name).dir
            }
          });
          gfs.remove({
            filename: manifest.Id[0],
            root: BUCKET
          }, (err) => {
            tarFile = fs.createReadStream(file.path);
            stream.on('close', () => {
              callback(null);
            });
            stream.on('error', (err) => {
              callback('Internal failuer while uploading plugin');
            });
            tarFile.pipe(stream);
          });
        });
      });
    } else {
      stream.on('end', cb);
      stream.resume();
    }
  });

  extract.on('finish', () => {
    fs.unlink(file.path, () => {
      callback(null);
    });
  });

  extract.on('error', (err) => {
    console.log(err);
    fs.unlink(file.path, () => {
      callback('Could not open, make sure it\'s a valid plugin pack');
    });
  });
  tarFile.pipe(extract);
}

function deleteNodePlugin(req, res) {
  let gfs = Grid(conn.db, mongoose.mongo);
  gfs.remove({
    _id: req.params.id,
    root: BUCKET
  }, (err) => {
    if (err) {
      res.status(500).send(err);
      return;
    }

    res.status(200).send('{"meta": {}}');
  });
}

module.exports = (router) => {
  GenericController({
    plural: 'nodePlugins',
    singular: 'nodePlugin'
  }, NodePlugin, {
    list: true,
    show: true
  }, {
    beforeRender: function(nodePlugin, req, res) {
      return {
        _id: nodePlugin._id,
        name: nodePlugin.metadata.name,
        description: nodePlugin.metadata.description,
        version: nodePlugin.metadata.description
      };
    }
  })(router);

  router.delete('/nodePlugins/:id', deleteNodePlugin );
  router.post('/nodePlugins/upload', upload.single('plugin'), (req, res) => {
    uploadNodePlugin(req.file, (err) => {
      if (err) {
        res.status(403).send(err);
        return;
      }

      res.send('OK!');
    });
  });
};
/* vim: set sw=2 ts=2 tw=2 et : */
