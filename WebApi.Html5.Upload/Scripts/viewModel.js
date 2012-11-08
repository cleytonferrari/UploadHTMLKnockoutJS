function ViewModel() {
    var self = this;
    self.uploads = ko.observableArray();

    self.enviarArquivo = function () {

        var photo = document.getElementById("arquivo");
        var files = photo.files;

        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new window.FormData();
                for (var i = 0; i < files.length; i++) {
                    data.append("file" + i, files[i]);
                }

                $.ajax({
                    type: "POST",
                    url: "/api/upload",
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (res) {
                        var array = self.uploads();
                        ko.utils.arrayPushAll(array, res);
                        self.uploads.valueHasMutated();
                    }

                });
            } else {
                console.log("Browser não suportado");
            }
        }
    };



}

var vm = new ViewModel();

$(document).ready(function () {
    ko.applyBindings(vm);
});