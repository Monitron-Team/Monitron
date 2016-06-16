import Model from 'ember-data/model';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';

export default Model.extend({
  name: attr(),
  description: attr(),
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
