import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    clicked() {
      let cb = this.get('on-clicked')
      if (cb) {
        cb(this);
      }
    }
  }
});
