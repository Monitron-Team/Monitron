const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema;

const AccountSchema = new Schema({
  name: {type: String, unique: true, required: true},
  password: String,
  "is-admin": {type: Boolean, default: false},
});

module.exports = mongoose.model('Account', AccountSchema);
/* vim: set sw=2 ts=2 tw=2 et : */
