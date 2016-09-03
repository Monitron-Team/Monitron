import Model from 'ember-data/model';
import attr from 'ember-data/attr';
import { belongsTo, hasMany } from 'ember-data/relationships';
import NonRestAction from 'boty-wui/mixins/non-rest-action';

import Ember from 'ember';

export default Model.extend(NonRestAction, {
  serial_key: attr('string'),
  maker: belongsTo('account', { async: true }),
  contact: belongsTo('contact', { async: true }),
  isPaired: attr('boolean'),
  pair: function() {
    const type = this.constructor.modelName;
    return this.nonRestAction('pair', 'POST', null ,'serial_key');
  },
  unpair: function() {
    const type = this.constructor.modelName;
    return this.nonRestAction('unpair', 'POST', null ,'serial_key');
  }
});
