const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema,
      ObjectId   = mongoose.Schema.ObjectId;

const RosterItem = new Schema({
  jid: {type: String, required: true},
  alias: {type: String},
  groups: {type: [String], index: true, trim: true}
}, {_id: false, id: false});

const Contact = new Schema({
  jid: {type: String, required: true, unique: true, index: true},
  owner: {type: ObjectId, ref: 'Account'},
  description: {type: String},
  updatedAt: {type: Date, default: Date.now},
  password: String,
  kind: {type: String, required: true, lowercase: true, trim: true},
  roster: [RosterItem]
});

module.exports = mongoose.model('Contact', Contact);
/* vim: set sw=2 ts=2 tw=2 et : */
