import Ember from 'ember';
import NonRestActionMixin from 'boty-wui/mixins/non-rest-action';
import { module, test } from 'qunit';

module('Unit | Mixin | non rest action');

// Replace this with your real tests.
test('it works', function(assert) {
  let NonRestActionObject = Ember.Object.extend(NonRestActionMixin);
  let subject = NonRestActionObject.create();
  assert.ok(subject);
});
