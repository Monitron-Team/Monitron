import Ember from 'ember';

export default Ember.Controller.extend({
  registerDisabled: false,
  actions: {
    submission() {
      this.set("registerDisabled", true);
      this.model.save()
        .then((account) => this.transitionToRoute('index'))
        .catch((err) => {
          if (err.errors && err.errors[0]) {
            this.set('errorMessage', err.errors[0].msg);
          }
          else {
            this.set('errorMessage', err.message);
          }
        })
        .finally(() => {
          this.set("registerDisabled", false);
        });
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
