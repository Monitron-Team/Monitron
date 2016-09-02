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

const Serial = new Schema({
  serial_key: {type: String, unique: true, required: true, index: true},
  maker: String,
  owner: {type: ObjectId, ref: 'Account'},
  jid: {type: String, index: true, get: JidGetter, set: JidSetter},
});

module.exports = mongoose.model('Serial', Serial);
/* vim: set sw=2 ts=2 tw=2 et : */
