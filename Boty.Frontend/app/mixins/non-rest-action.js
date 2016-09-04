import Ember from 'ember';

const { getOwner } = Ember;

export default Ember.Mixin.create({
  nonRestAction: function (action, method, data, key) {
    const type = this.constructor.modelName;
    const adapter = this.store.adapterFor(type);
    return adapter.ajax(this.getActionUrl(action, adapter, key), method, { data: data });
  },

  getActionUrl: function(action, adapter, key) {
    const type = this.constructor.modelName;
    if (!key) {
      key = 'id';
    }
    const id = this.get(key);

    return `${adapter.buildURL(type, id)}/${action}`;
  }
});
