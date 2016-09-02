import Model from 'ember-data/model';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';

export default Model.extend({
  maker: attr('string'),
  serial_key: attr('string'),
  jid: attr('jid'),
  owner: belongsTo('account', { async: true , default: null})
});