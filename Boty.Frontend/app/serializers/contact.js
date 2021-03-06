import DS from 'ember-data';

export default DS.RESTSerializer.extend(DS.EmbeddedRecordsMixin, {
  primaryKey: '_id',
  attrs: {
    jid: {
      embedded: 'allways'
    },
    roster: {
      serialize: 'records',
      deserialize: 'records'
    },
    owner: {
      serialize: 'id',
      deserialize: 'id'
    }
  }
});
