import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    delete_account(contact) {
      contact.deleteRecord();
      contact.save();
    }
  }
});
