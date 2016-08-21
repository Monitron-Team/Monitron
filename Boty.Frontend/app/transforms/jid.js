import DS from 'ember-data';

export default DS.Transform.extend({
  deserialize(serialized) {
    return {
      name: serialized.name,
      domain: serialized.domain
    };
  },

  serialize(deserialized) {
    return {
      name: deserialized.name,
      domain: deserialized.domain
    };
  }
});
