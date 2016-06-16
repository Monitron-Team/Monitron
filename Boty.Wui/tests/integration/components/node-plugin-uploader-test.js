import { moduleForComponent, test } from 'ember-qunit';
import hbs from 'htmlbars-inline-precompile';

moduleForComponent('node-plugin-uploader', 'Integration | Component | node plugin uploader', {
  integration: true
});

test('it renders', function(assert) {
  // Set any properties with this.set('myProperty', 'value');
  // Handle any actions with this.on('myAction', function(val) { ... });

  this.render(hbs`{{node-plugin-uploader}}`);

  assert.equal(this.$().text().trim(), '');

  // Template block usage:
  this.render(hbs`
    {{#node-plugin-uploader}}
      template block text
    {{/node-plugin-uploader}}
  `);

  assert.equal(this.$().text().trim(), 'template block text');
});
