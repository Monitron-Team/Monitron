import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    submission() {
      this.model.save()
        .then((account) => this.transitionToRoute('index'))
        .catch((err) => this.set('errorMessage', err.message));
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
