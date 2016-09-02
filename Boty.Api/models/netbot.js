const mongoose   = require('mongoose'),
      Schema     = mongoose.Schema,
      ObjectId   = mongoose.Schema.ObjectId;

const NetBotSchema = new Schema({
  contact: {type: ObjectId, ref: 'Contact', index: true, unique: true},
  nodePlugin: {type: ObjectId, ref: 'NodePlugin'},
  hostedAt: {type: String}
});

module.exports = mongoose.model('NetBot', NetBotSchema);
