import { moduleForComponent, test } from 'ember-qunit';
import hbs from 'htmlbars-inline-precompile';

moduleForComponent('node-plugins-table', 'Integration | Component | node plugins table', {
  integration: true
});

test('it renders', function(assert) {
  // Set any properties with this.set('myProperty', 'value');
  // Handle any actions with this.on('myAction', function(val) { ... });

  this.render(hbs`{{node-plugins-table}}`);

  assert.equal(this.$().text().trim(), '');

  // Template block usage:
  this.render(hbs`
    {{#node-plugins-table}}
      template block text
    {{/node-plugins-table}}
  `);

  assert.equal(this.$().text().trim(), 'template block text');
});
