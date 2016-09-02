import DS from 'ember-data';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';

export default DS.Model.extend({
  contact: belongsTo('contact', { async: true }),
  nodePlugin: belongsTo('node-plugin', { async: true }),
  hostedAt: attr('string')
});
