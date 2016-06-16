import Ember from 'ember';

export default Ember.Component.extend({
  file: null,
  fileName: "No file selecetd",
  uploading: false,
  lastUpload: null,
  actions: {
    selectFile(file) {
      this.set('file', file);
      this.set('fileName', file.name);
    },
    fileUpload() {
      let controller = this;
      function uploadFinished(err) {
        if (err) {
          controller.set('errorMessage', err);
        } else {
          controller.set('file', null);
          controller.set('lastUpload', controller.get('fileName'));
          controller.set('fileName', "No file selected");
        }

        controller.set('uploading', false);
      }
      let file = this.get('file');
      controller.set('errorMessage', null);
      controller.set('lastUpload', null);
      this.set('uploading', true);
      if (file === null) {
        uploadFinished('No file was selected');
        return;
      }

    	var formData = new FormData();
			formData.append('plugin', file.getSource());
			Ember.$.ajax({
				type: 'POST',
				url: 'http://localhost:9898/api/v1/node-plugins/upload',
				data: formData,
				cache: false,
				contentType: false,
				processData: false,
				success: function(data) {
          uploadFinished(null);
				},
				error: function(err) {
          uploadFinished(err.responseText);
				}
			});
    }
  },
  didInsertElement: function() {
    var self = this;

    var fileInput = new mOxie.FileInput({
      browse_button: this.$('#file-picker').get(0),
      multiple: false,
			accept: {
				title: "Plugin Packs",
				extensions: "tar"
			}
    });

    fileInput.onchange = function(e) {
      self.get('controller').send('selectFile', fileInput.files[0]);
    };

    fileInput.init();
  }
});
/* vim: set tabstop=2 shiftwidth=2 expandtab: */
