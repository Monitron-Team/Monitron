import JSONAPIAdapter from 'ember-data/adapters/json-api';
import DataAdapterMixin from 'ember-simple-auth/mixins/data-adapter-mixin';

export default JSONAPIAdapter.extend(DataAdapterMixin, {
  authorizer: 'authorizer:boty',
  host: 'http://localhost:9898',
  namespace: 'api/v1'
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
