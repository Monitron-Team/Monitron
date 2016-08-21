import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    delete_account(account) {
      account.deleteRecord();
      account.save();
    },
    toggle_admin(account) {
      console.log(account.get('isAdmin'));
      let old_value = account.get('isAdmin');
      account.set('isAdmin', old_value === false);
      console.log(account.changedAttributes());
      account.save().catch((err) => console.log(err));
      console.log(account.get('isAdmin'));
    },
    toggle_device_maker(account) {
      console.log(account.get('isDeviceMaker'));
      let old_value = account.get('isDeviceMaker');
      account.set('isDeviceMaker', old_value === false);
      console.log(account.changedAttributes());
      account.save().catch((err) => console.log(err));
      console.log(account.get('isDeviceMaker'));
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
