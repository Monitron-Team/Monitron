import Ember from 'ember';

export default Ember.Controller.extend({
  session: Ember.inject.service('session'),
  actions: {
    invalidateSession: function() {
      this.get('session').invalidate();
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
