import Ember from 'ember';
import ENV from '../config/environment';

export default Ember.Controller.extend({
  isUsed: false,
  actions: {
    addSerial(account) {
      let name = account.get('name') 
      alert(name)
      let component = this;
      let serialKey = this.get('newSerial');
      alert(serialKey);
      component.set("errors", []);
      let store = this.store;
      alert(store);
      let randomUserName = Math.floor(Math.random()* Math.pow(10,15)).toString(16)
      alert(randomUserName)
      let makerDomain = "boss.monitron.com"
      let newuser = {
        jid: {name: randomUserName, domain: makerDomain},
        password: "123456",
        kind: "device",
        roster: []
        }
      let contact = store.createRecord('contact',newuser)
      contact.save().then(function(contact_info){
        alert(contact_info.get('jid'))
        alert('before new serial');
        let newSerial = store.createRecord('serial', {
        maker: account,
        serial_key: serialKey,
        contact: contact,
        });
        newSerial.save()
          .then(() => {
            alert('fuckmyliife');
            window.setTimeout(()=>$("#new-user-dialog").modal('hide'));
            alert('fuckmyliif again');
          })
          .catch((e) => {
            alert('catch!');
            component.set("errors", e.errors);
            newSerial.deleteRecord();
          })
        .catch(function(error){
          alert('todo catch')
        })
      })
        .finally(() => {
          component.set('is-creating-user', false);
        });
    },
    onDialogShow(e) {
      let self = this;
      $(e.target).find("input").val("");
      this.set("errors", []);
      this.store.findAll("serial")
        .then((res) => self.set("serials", res));
    },
    preventHide(shouldPrevent, e) {
      if (shouldPrevent) {
        e.preventDefault();
      }
    },
  }
});


