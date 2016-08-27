import Ember from 'ember';

export default Ember.Controller.extend({
  actions: {
    submission() {
      let self = this;
      self.set('is-saving', true);
      let model = this.get("model");
      model.save()
        .then((m)=> {
          model.get('roster').filterBy('isNew').invoke('unloadRecord');
          this.rerender();
        })
        .catch((e)=>this.set('errors', e.errors))
        .finally(()=>{
          self.set('is-saving', false);
      });
    }
  }

});
