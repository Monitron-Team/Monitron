import Ember from 'ember';

export default Ember.Controller.extend({
	actions: {
    updateModel() {
      this.set('model', this.store.findAll('node-plugin'));
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
