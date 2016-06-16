const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema;

const PluginSchema = new Schema({
  name: String,
  description: String,
  version: String
});

module.exports = mongoose.model('NodePlugin', PluginSchema, 'plugin_store.files');
/* vim: set sw=2 ts=2 tw=2 et : */
