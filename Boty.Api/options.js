const mongo_url = 'mongodb://boss.monitron.test/monitron';
const mongo_options = {
  user: 'monitron_mgmt',
  password: 'HellIsForHeroes@!'
};

module.exports = {
  mongo_url: mongo_url,
  mongo_options: mongo_options,
  xmpp: {
    username: 'monitron_admin@boss.monitron.test',
    password: 'HellIsForHeroes@@'
  }
};
/* vim: set sw=2 ts=2 tw=2 et : */
