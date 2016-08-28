import DS from 'ember-data';
import ApplicationSerializer from './application';

export default DS.RESTSerializer.extend(DS.EmbeddedRecordsMixin, {
  primaryKey: '_id',
  attrs: {
    jid: {
      embedded: 'allways'
    },
    groups: {
      embedded: 'allways'
    }
  }
});
