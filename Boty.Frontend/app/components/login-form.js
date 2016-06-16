import Ember from 'ember';

export default Ember.Component.extend({
  session: Ember.inject.service('session'),
	actions: {
    authenticate() {
      let credentials = this.getProperties('identification', 'password');
      this.get('session').authenticate('authenticator:boty', credentials)
        .catch((message) => {
          this.set('errorMessage', message);
        });
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
