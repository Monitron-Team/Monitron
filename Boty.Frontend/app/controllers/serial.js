import Ember from 'ember';
import ENV from '../config/environment';

export default Ember.Controller.extend({
  isUsed: false,
  actions: {
    assign() {
      this.set("isUsed", true);
      this.model.save()
        .then((account) => this.transitionToRoute('serial'))
        .catch((err) => {
          if (err.errors && err.errors[0]) {
            this.set('errorMessage', err.errors[0].msg);
          }
          else {
            this.set('errorMessage', err.message);
          }
        })
        .finally(() => {
          this.set("assignDisabled", false);
        });
    },
    addSerial(account){
      alert(10000);
      alert(account)
      let gg = this.get('newSerial');
      alert(gg);
    },
    createUser(account) {
      let component = this;
      component.set("errors", []);
      let store = this.store;
      let contact = store.createRecord('contact', {
        jid: {name: name, domain: this.get("domain")},
        owner: account,
        description: description,
        password: password,
        kind: "user",
        roster: [],
      });
      contact.save()
        .then(() => {
          window.setTimeout(()=>$("#new-user-dialog").modal('hide'));
        })
        .catch((e) => {
          component.set("errors", e.errors);
          contact.deleteRecord();
        })
        .finally(() => {
          component.set('is-creating-user', false);
        });
    },
    onDialogShow(e) {
      let self = this;
      $(e.target).find("input").val("");
      this.set("errors", []);
      this.set("plugins", null);
      this.store.findAll("node-plugin")
        .then((res) => self.set("plugins", res));
    },
    preventHide(shouldPrevent, e) {
      if (shouldPrevent) {
        e.preventDefault();
      }
    },
  }
});


