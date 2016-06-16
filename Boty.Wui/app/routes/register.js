import Ember from 'ember';

export default Ember.Route.extend({
	model() {
    return this.store.createRecord('account', {
      name: "",
      password: "",
      is_admin: false,
    });
  },

  setupController(controller, model) {
    this._super(controller, model);
    controller.set('model', model);
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
