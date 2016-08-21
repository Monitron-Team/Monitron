import Ember from 'ember';

export default Ember.Component.extend({
  didRender() {
    let component = this;
    let myId = this.get("dialog-id");
    let self = $('#' + myId);
    self.on('show.bs.modal', (e) => {
      let cb = component.get('on-show');
      if (cb) {
        cb(e);
      }
    });
    self.on('shown.bs.modal', () => {
      self.focus();
      let cb = component.get('on-shown');
      if (cb) {
        cb(e);
      }
    });
    self.on('hide.bs.modal', () => {
      self.focus();
      let cb = component.get('on-hide');
      if (cb) {
        cb(e);
      }
    });
    self.on('hidden.bs.modal', () => {
      self.focus();
      let cb = component.get('on-hidden');
      if (cb) {
        cb(e);
      }
    });
  },
  actions: {
    submission() {
      let cb = this.get("on-submit");
      if (cb) {
        cb();
      }
    }
  }
});
