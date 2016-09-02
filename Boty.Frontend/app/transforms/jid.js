import DS from 'ember-data';

export default DS.Transform.extend({
  deserialize(serialized) {
    let parts = serialized.split(/@/);
    return {
      name: parts[0],
      domain: parts[1]
    };
  },

  serialize(deserialized) {
    return deserialized.name + '@' + deserialized.domain;
  }
});
