import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    delete_plugin(plugin) {
      plugin.deleteRecord();
      plugin.save();
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
