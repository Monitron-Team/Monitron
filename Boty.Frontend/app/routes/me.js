import Ember from 'ember';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Ember.Route.extend(AuthenticatedRouteMixin, {
  session: Ember.inject.service('session'),
	model() {
    let accountId = this.get("session.data").authenticated.account;
    return {
      account: this.store.findRecord('account', accountId),
      contacts: this.store.findAll('contact'),
      serials: this.store.findAll('serial')
    };
	}
});
