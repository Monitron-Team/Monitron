import Ember from 'ember';

export default Ember.Component.extend({
  actions: {
    addGroupKey(e) {
      if (e.key == "Enter") {
        this.send('addGroup')
        return false;
      }

    },
    addGroup() {
      let input = $(this.element).find("input");
      let name = input.val();
      if (name === "") {
        return;
      }

      let model = this.get("model");
      let groups = model.get("groups");
      if (groups.indexOf(name) === -1) {
        groups.push(name);
        model.set("groups", groups);
        input.val("");
      }

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
