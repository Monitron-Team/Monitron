import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    submission() {
      let self = this;
      self.set('is-saving', true);
      let model = this.get("model");
      model.save()
        .catch((e)=>this.set('errors', e.errors))
        .finally(()=>{
        self.set('is-saving', false);
        model.get('roster').filterBy('isNew').invoke('unloadRecord');
        this.set('model', model);
      });
    }
  }

});
