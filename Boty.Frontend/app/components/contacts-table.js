import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    delete_account(contact) {
      contact.deleteRecord();
      contact.save();
    },
    unpair(contact, router) {
      let self = this;
      contact.store.query('serial', {contact: contact.id}).
        then((result) => {
          result.popObject()
            .unpair()
            .finally(() => {
              contact.store.findAll('contact', {reload: true});
							contact.store.unloadAll();
            });
        });
    }
  }
});
