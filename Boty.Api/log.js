'use strict';
const winston = require('winston');

let level = 'info';
if (process.env.DEBUG) {
  level = 'debug';
}

let logger = new (winston.Logger)({
  transports: [
    new (winston.transports.Console)({ handleExceptions: true, humanReadableUnhandledException: true, json: false, timestamp: process.stdout.isTTY, colorize: process.stdout.isTTY, level: level })
  ],
  exceptionHandlers: [
    new (winston.transports.Console)({ json: false, timestamp: process.stdout.isTTY, colorize: process.stdout.isTTY })
  ],
  exitOnError: false
});

module.exports = logger;

