import DS from 'ember-data';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';

export default DS.Model.extend({
  jid: attr('jid'),
  alias: attr('string'),
  groups: attr({default: []}),
});
