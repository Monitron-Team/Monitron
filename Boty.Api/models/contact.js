const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema,
      ObjectId   = mongoose.Schema.ObjectId;

const JidSetter = function(v) {
  return v.name + '@' + v.domain;
};

const JidGetter = function(v) {
  let parts = v.split(/@/);
  return {
    name: parts[0],
    domain: parts[1]
  };
};

const RosterItem = new Schema({
  jid: {type: String, required: true, get: JidGetter, set: JidSetter},
  alias: {type: String},
  groups: {type: [String], index: true, trim: true}
}, {_id: false, id: false});

const Contact = new Schema({
  jid: {type: String, required: true, unique: true, index: true, get: JidGetter, set: JidSetter},
  owner: {type: ObjectId, ref: 'Account'},
  description: {type: String},
  password: String,
  kind: {type: String, required: true, lowercase: true, trim: true},
  roster: [RosterItem]
});

module.exports = mongoose.model('Contact', Contact);
/* vim: set sw=2 ts=2 tw=2 et : */
