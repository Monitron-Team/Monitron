/* jshint node: true */

module.exports = function(environment) {
  var ENV = {
    modulePrefix: 'boty-wui',
    environment: environment,
    rootURL: '/',
    locationType: 'auto',

    EmberENV: {
      FEATURES: {
        // Here you can enable experimental features on an ember canary build
        // e.g. 'with-controller': true
      },
      EXTEND_PROTOTYPES: {
        Date: false,
      }
    },

    APP: {
      // Here you can pass flags/options to your application instance
      // when it is created
    },
  };

  ENV['ember-simple-auth'] = {
    store: 'simple-auth-session-store:local-storage',
    authorizer: 'authorizer:boty',
    crossOriginWhitelist: ['http://localhost:9898/'],
    routeAfterAuthentication: '/me',
    authenticationRoute: 'log-in'
  }

  if (environment === 'development') {
    // ENV.APP.LOG_RESOLVER = true;
    // ENV.APP.LOG_ACTIVE_GENERATION = true;
    // ENV.APP.LOG_TRANSITIONS = true;
    // ENV.APP.LOG_TRANSITIONS_INTERNAL = true;
    // ENV.APP.LOG_VIEW_LOOKUPS = true;
    ENV['ember-simple-auth'].crossOriginWhitelist = 'http://localhost:9898';
    ENV['data-adapter'] = {
      host: 'http://localhost:9898'
    };
    ENV['domain'] = "boss.monitron.test";
  }

  if (environment === 'test') {
    // Testem prefers this...
    ENV.rootURL = '/';
    ENV.locationType = 'none';

    // keep test console output quieter
    ENV.APP.LOG_ACTIVE_GENERATION = false;
    ENV.APP.LOG_VIEW_LOOKUPS = false;

    ENV.APP.rootElement = '#ember-testing';
  }

  if (environment === 'vagrant') {
    ENV['ember-simple-auth'].crossOriginWhitelist = 'http://boss.monitron.test:9898';
    ENV['domain'] = "boss.monitron.test";
    ENV['data-adapter'] = {
      host: 'http://boss.monitron.test:9898'
    };
  }

  if (environment === 'production') {
    ENV['ember-simple-auth'].crossOriginWhitelist = 'http://monitron.ddns.net:9898';
    ENV['domain'] = "monitron.ddns.net";
    ENV['data-adapter'] = {
      host: 'http://monitron.ddns.net:9898'
    };
  }


  return ENV;
};
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
