import DS from 'ember-data';

export default DS.RESTSerializer.extend(DS.EmbeddedRecordsMixin, {
  attrs: {
    jid: {
      embedded: 'allways'
    },
    owner: {
      serialize: 'id',
      deserialize: 'id'
    }
  }
});
