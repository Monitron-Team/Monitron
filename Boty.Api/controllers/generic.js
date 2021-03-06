const co = require('co');
const log = require('../log');

let GenericController = function (entity, model, verbs, cbs) {
  if (cbs === null || cbs === undefined) {
    cbs = {};
  }

  let renderResponse = co.wrap(function* (results, req, res) {
    let response = [];
    for (let i = 0; i < results.length; i++) {
      if (results[i] === null) {
        continue;
      }

      let obj = results[i].toObject({getters: true});
      if (cbs.beforeRender) {
        try {
          obj = yield cbs.beforeRender(obj, req, res);
        } catch (e) {
        }
      }

      if (obj === null) {
        continue;
      }

      response.push(obj);
    }

    res.send({[entity.plural] : response});
  });

  let listEntitys = co.wrap(function* (req, res) {
    let results;
    let query = req.query;
    if (cbs.beforeList) {
      query = yield cbs.beforeList(query, req, res);
    }

    try {
      results = yield model.find(query).exec();
    } catch (e) {
      res.status(500).send({errors: [{code: 500, msg: e.message}]});
      return;
    }

    yield renderResponse(results, req, res);
  });

  let showEntity = co.wrap(function* (req, res) {
    let result;
    try {
      result = yield model.findOne({_id: req.params.id}).exec();
    } catch (e) {
      if (e.name === 'CastError') {
        result = null;
      } else {
        res.status(500).send({errors: [{code: 500, msg: e.message}]});
        return;
      }
    }

    yield renderResponse([result], req, res);
  });

  let createEntity = co.wrap(function* (req, res) {
    let obj = req.body[entity.singular];
    if (cbs.beforeSave) {
      obj = yield cbs.beforeSave(obj, req, res);
    }

    let instance;
    try {
      instance = new model(obj);
      yield instance.save();
    } catch (e) {
      log.error(e);
      if (e.code === 11000) {
        res.status(500).send({errors: [{code: 500, msg: entity.singular + ' already exists'}]});
      } else {
        res.status(500).send({errors: [{code: 500, msg: e.message}]});
      }
      return;
    }

    if (cbs.afterSave) {
      yield cbs.afterSave(instance, req, res);
    }

    yield renderResponse([instance], req, res);
  });

  let updateEntity = co.wrap(function* (req, res) {
    let obj = req.body[entity.singular];
    obj.id = req.params.id;
    if (cbs.beforeSave) {
      obj = yield cbs.beforeSave(obj, req, res);
    }

    let id = obj.id;
    delete obj.id;
    let result;
    try {
      result = yield model.findOneAndUpdate({_id: id}, obj, {new: true}).exec();
    } catch (e) {
      res.status(500).send({errors: [{code: 500, msg: e.message}]});
      return;
    }

    if (cbs.afterSave) {
      yield cbs.afterSave(result, req, res);
    }

    yield renderResponse([result], req, res);
  });

  let deleteEntity = co.wrap(function* (req, res) {
    let id = req.params.id;
    if (cbs.beforeDelete) {
      let shouldDelete = yield cbs.beforeDelete(id, req, res);
      if (!shouldDelete) {
        res.status(403).send({errors: [{code: 403, msg: 'Deletion not allowed'}]});
        return;
      }
    }
    try {
      yield model.remove({_id: id}).exec();
    } catch (e) {
      res.status(500).send({errors: [{code: 500, msg: e.message}]});
      return;
    }

    if (cbs.afterDelete) {
      yield cbs.afterDelete(id, req, res);
    }

    yield renderResponse([], req, res);
  });

  return (router) => {
    if (verbs.list) {
      router.get('/' + entity.plural + '/', listEntitys);
    }
    if (verbs.show) {
      router.get('/' + entity.plural + '/:id', showEntity);
    }
    if (verbs.create) {
      router.post('/' + entity.plural + '/', createEntity);
    }
    if (verbs.update) {
      router.put('/' + entity.plural + '/:id', updateEntity);
    }
    if (verbs.delete) {
      router.delete('/' + entity.plural + '/:id', deleteEntity);
    }
  };
};

module.exports = GenericController;
