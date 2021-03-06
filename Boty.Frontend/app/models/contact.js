import Model from 'ember-data/model';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';

export default Model.extend({
  jid: attr('jid'),
  owner: belongsTo('account', { async: true }),
  password: attr('string'),
  description: attr('string'),
  kind: attr('string'),
  roster: hasMany('roster-item', { async: false })
});
