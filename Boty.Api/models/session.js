const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema;

const SessionSchema = new Schema({
  account: {type: Schema.Types.ObjectId, ref: 'Account'}
});

module.exports = mongoose.model('SessionPlugin', SessionSchema);
/* vim: set sw=2 ts=2 tw=2 et : */
