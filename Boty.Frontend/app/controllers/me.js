import Ember from 'ember';
import ENV from '../config/environment';

const DEVICES = {
  "2a6bfd4b-75b3-4639-a47c-6cbaee76a825": {
    jid: {name: "SAMSUNG_TV_6cbaee76a825", domain: "samsung.com"},
    password: "123456",
    kind: "device",
    roster: []
  },
  "6325f603-d924-484c-ab73-996466e5cf61": {
    jid: {name: "BOSE_STEREO_6cbaee76a825", domain: "bose.com"},
    password: "123456",
    kind: "device",
    roster: []
  }
};

export default Ember.Controller.extend({
  domain: ENV["domain"],
  actions: {
    onDialogShow(e) {
      let self = this;
      $(e.target).find("input").val("");
      this.set("errors", []);
      this.set("plugins", null);
      this.store.findAll("node-plugin")
        .then((res) => self.set("plugins", res));
    },
    createUser(account) {
      alert("maorrr");
      alert(account);
      this.set('is-creating-user', true);
      let component = this;
      let name = $('#contact-name').val();
      let password = $('#password').val();
      let description = $('#description').val();
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
    preventHide(shouldPrevent, e) {
      if (shouldPrevent) {
        e.preventDefault();
      }
    },
    pairDevice(account) {
      this.set('is-pairing-device', true);
      let component = this;
      let serial = $('#device-serial').val();
      let description = $('#device-description').val();
      component.set("errors", []);
      //let device_info = DEVICES[serial];
      let device_info = this.store.query('serial', {serial_key:serial})
      .then(function(device_info){alert(device_info.get('serial_key'))})
      .catch(function(error){
        alert('todo catch')
      }
        );
      

      if (!device_info) {
        component.set("errors", [
          {msg: "Invalid serial"}
        ]);
        component.set('is-pairing-device', false);
        return;
      }

      device_info.owner = account;
      device_info.description = description;
      let store = this.store;
      window.setTimeout(()=>{
        let contact = store.createRecord('contact', device_info);
        contact.save()
          .then(() => {
            window.setTimeout(()=>$("#pair-device-dialog").modal('hide'));
          })
          .catch((e) => {
            component.set("errors", [
              {msg: "Device already paired, either with you or with another account."}
            ]);
            contact.deleteRecord();
          })
          .finally(() => {
            component.set('is-pairing-device', false);
          });
      }, 4 * 1000);
    },
    startNetBot(account) {
      this.set('is-starting-netbot', true);
      let component = this;
      let name = $('#bot-user-name').val();
      let plugin = $('#node-plugin').val();
      let description = $('#netbot-description').val();
      component.set("errors", []);

      let contactInfo = {
        jid: {name: name, domain: this.get("domain")},
        owner: account,
        description: description,
        kind: "netbot",
        roster: []
      }

      let store = this.store;
      let contact = store.createRecord('contact', contactInfo);
      let e = document.getElementById('node-plugin');
      let nodePluginId = e.options[e.selectedIndex].value;

      contact.save()
        .then((contact) => {
          window.setTimeout(()=>$("#start-netbot-dialog").modal('hide'));
          store.findRecord('node-plugin', nodePluginId)
            .then(function(nodePlugin) {
              store.createRecord('netbot', {
                contact: contact,
                nodePlugin: nodePlugin
              }).save();
            });
        })
        .catch((e) => {
          component.set("errors", e.errors);
          contact.deleteRecord();
        })
        .finally(() => {
          component.set('is-starting-netbot', false);
        });
    }
  }
});
