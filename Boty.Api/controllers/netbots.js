const NetBot = require('../models/netbot');
const GenericController = require('./generic');
const adhoc = require('../adhoc');

module.exports = GenericController({
  plural: 'netbots',
  singular: 'netbot'
}, NetBot, {
  list: true,
  show: true,
  create: true,
  update: true,
  delete: false
}, {
  afterSave: function(netbot) {
    adhoc.notifyNetbotUpdate();
    return netbot;
  },
  afterDelete: function() {
    adhoc.notifyNetbotUpdate();
  }
});
