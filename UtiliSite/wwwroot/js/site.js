// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

$('#coreSettings').submit(function(e) {
    var formData = $(this).serialize();
    $.ajax({
        type: 'POST',
        data: formData,
        success: function(result) {
            $('#coreSettingsSuccess').toast('show');
        }
    });
});