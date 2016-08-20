import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    submission() {
      this.model.save()
        .then((account) => this.transitionToRoute('index'))
          .catch((err) => {
            if (err.errors && err.errors[0]) {
              this.set('errorMessage', err.errors[0].title);
            }
            else {
              this.set('errorMessage', err.message);
            }
          });
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
