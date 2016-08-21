import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    CreateUserDialog_onShow(dialog) {
      $('#username').val('');
      $('#password').val('');
      this.set("errors", []);
    },
    PairDeviceDialog_onShow(dialog) {
      $('#device_serial').val('');
    },
    StartNetbotDialog_onShow(dialog) {
    },
    createUser(model) {
      let component = this;
      let name = $('#username').val();
      let password = $('#password').val();
      component.set("errors", []);
      let store = this.store;
      let contact = store.createRecord('contact', {
        jid: {name: name, domain: "monitron.ddns.net"},
        owner: model.account,
        password: password,
        kind: "user",
        roster: [],
      });
      contact.save()
        .then(() => {
          $("#new-user-dialog").modal('hide');
        })
        .catch((e) => {
          component.set("errors", e.errors);
          contact.deleteRecord();
        })
    },
  }
});
