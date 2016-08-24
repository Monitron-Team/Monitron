import Ember from 'ember';

export default Ember.Component.extend({
  init() {
    this._super();
    let model = this.get("model");
    let store = model.store;
    this.set("contacts", store.findAll('contact'));
  },
  didUpdate() {
    let model = this.get("model");
  },
  actions: {
    addContact(contact) {
      let model = this.get("model");
      let item = model.store.createRecord('roster-item', {
        jid: contact.get('jid'),
        groups:[]
      });
      model.pushObject(item);
      //item.destroy();
    },
    removeContact(item) {
      let model = this.get("model");
      model.removeObject(item);
    }
  }
});
