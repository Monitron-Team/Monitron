import JSONAPIAdapter from 'ember-data/adapters/json-api';
import DataAdapterMixin from 'ember-simple-auth/mixins/data-adapter-mixin';
import ENV from '../config/environment';

export default JSONAPIAdapter.extend(DataAdapterMixin, {
  authorizer: 'authorizer:boty',
  host: ENV['data-adapter'].host,
  namespace: 'api/v1'
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
