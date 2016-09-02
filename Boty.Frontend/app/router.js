import Ember from 'ember';
import config from './config/environment';

const Router = Ember.Router.extend({
  location: config.locationType
});

Router.map(function() {
  this.route('plugins');
  this.route('about');
  this.route('node-plugins');
  this.route('accounts');
  this.route('register');
  this.route('log-in');
  this.route('me');
  

  this.route('contact', function() {
    this.route('edit', {path: '/:contact_id'});
  });
  this.route('serial');

});

export default Router;
