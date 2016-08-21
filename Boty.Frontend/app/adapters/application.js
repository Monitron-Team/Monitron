import DS from 'ember-data';
import DataAdapterMixin from 'ember-simple-auth/mixins/data-adapter-mixin';
import ENV from '../config/environment';

export default DS.RESTAdapter.extend(DataAdapterMixin, {
  authorizer: 'authorizer:boty',
  host: ENV['data-adapter'].host,
  namespace: 'api/v1'
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
