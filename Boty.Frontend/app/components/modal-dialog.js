import Ember from 'ember';

export default Ember.Component.extend({
  tagName: 'div',
  classNames: ['modal'],
  classNameBindings: ['fade'],
  attributeBindings: ['tabIndex:_tabIndex', 'role:_role'],
  _tabIndex: -1,
  _role: 'dialog',
  fade: true,
  didRender() {
    let component = this;
    let self = $('#' + component.get('id'));
    self.on('show.bs.modal', (e) => {
      let cb = component.get('on-show');
      if (cb) {
        cb(e);
      }
    });
    self.on('shown.bs.modal', (e) => {
      self.focus();
      let cb = component.get('on-shown');
      if (cb) {
        cb(e);
      }
    });
    self.on('hide.bs.modal', (e) => {
      let cb = component.get('on-hide');
      if (cb) {
        cb(e);
      }
    });
    self.on('hidden.bs.modal', (e) => {
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
