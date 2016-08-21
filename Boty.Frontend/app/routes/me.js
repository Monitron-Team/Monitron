import Ember from 'ember';

export default Ember.Route.extend({
  session: Ember.inject.service('session'),
	model() {
    let accountId = this.get("session.data").authenticated.account;
    return {
      account: this.store.findRecord('account', accountId),
      contacts: this.store.findAll('contact')
    };
	}
});
