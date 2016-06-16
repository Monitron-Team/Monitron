import Ember from 'ember';
import Base from 'ember-simple-auth/authenticators/base';
import ENV from '../config/environment';

export default Base.extend({
  tokenEndpoint: ENV['data-adapter'].host + '/api/v1/sessions/create',
  restore(data) {
    return new Ember.RSVP.Promise(function(resolve, reject) {
      if (!Ember.isEmpty(data.token)) {
        resolve(data);
      } else {
        reject();
      }
    });
  },

  authenticate(options) {
      return new Ember.RSVP.Promise((resolve, reject) => {
          Ember.$.ajax({
              url: this.tokenEndpoint,
              type: 'POST',
              data: JSON.stringify({
                  username: options.identification,
                  password: options.password
              }),
              contentType: 'application/json;charset=utf-8',
              dataType: 'json'
          }).then(function(response) {
              Ember.run(function() {
                  resolve({
                      token: response.id_token
                  });
              });
          }, function(xhr, status, error) {
              var response = xhr.responseText;
              Ember.run(function() {
                  reject(response);
              });
          });
      });
  },

  invalidate() {
      console.log('invalidate...');
      return Ember.RSVP.resolve();
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
