import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    addGroup() {
      let input = $(this.element).find("input");
      let name = input.val();
      if (name === "") {
        return;
      }
      let model = this.get("model");
      let groups = model.get("groups");
      groups.push(name);
      console.log(this.get("model"));
      input.val("");
      model.set("groups", groups);
      this.rerender();
    },
    removeGroup(group) {
      let groups = this.get("model").get("groups");
      let i = groups.indexOf(group);
      groups.splice(i, 1);
      this.get("model").set("groups", groups);
      this.rerender();
    }
  }
});
