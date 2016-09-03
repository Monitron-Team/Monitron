const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema,
      ObjectId   = mongoose.Schema.ObjectId;




const Serial = new Schema({
  serial_key: {type: String, unique: true, required: true, index: true},
  maker: {type: ObjectId, ref: 'Account'},
  contact: {type: ObjectId, ref: 'Contact'},
});

module.exports = mongoose.model('Serial', Serial);
/* vim: set sw=2 ts=2 tw=2 et : */
