import Ember from 'ember';
import Base from 'ember-simple-auth/authorizers/base';
export default Base.extend({
  authorize(data, block) {
    const { token }  = data;
    if (!(token === null || token === '')) {
      block('Authorization', `Bearer ${token}`);
    }
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
