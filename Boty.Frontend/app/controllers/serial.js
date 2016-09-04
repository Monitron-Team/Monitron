import Ember from 'ember';
import ENV from '../config/environment';

export default Ember.Controller.extend({
  isUsed: false,
  actions: {
    addSerial(account) {
      let name = account.get('name')
      let component = this;
      let serialKey = this.get('newSerial');
      component.set("errors", []);
      let store = this.store;
      let randomUserName = Math.floor(Math.random()* Math.pow(10,15)).toString(16)
      let makerDomain = "boss.monitron.com"
      let newSerial = store.createRecord('serial', {
        maker: account,
        serial_key: serialKey,
        contact: null,
      });
      newSerial.save()
        .then(function(newSerial) {
          let contact = store.createRecord('contact', {
            jid: {name: randomUserName, domain: makerDomain},
            password: "123456",
            kind: "device",
            owner: account,
            roster: []
          });
          contact.save()
            .then(function(contact) {
              newSerial.set('contact', contact);
              newSerial.save();
              component.set('newSerial', '');
            })
            .catch((e) => {
              component.set('errors', e.errors);
              newSerial.deleteRecord();
              newSerial.save();
            });
        })
        .catch((e) => {
          newSerial.deleteRecord();
          component.set('errors', e.errors);
        });
    }
  }
});


