import Model from 'ember-data/model';
import attr from 'ember-data/attr';
//import { belongsTo, hasMany } from 'ember-data/relationships';

export default Model.extend({
	name: attr('string'),
	password: attr('string'),
	isAdmin: attr('boolean', { defaultValue: false }),
	isDeviceMaker: attr('boolean', { defaultValue: false }),
  /*contacts: hasMany('contact', {
    inverse: 'owner'
  })*/
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
