// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

$('#settings0').submit(function(e) {
    var formData = $(this).serialize();
    $.ajax({
        type: 'POST',
        data: formData,
        success: function(result) {
            $('#success0').toast('show');
        }
    });
});

$('#settings1').submit(function(e) {
    var formData = $(this).serialize();
    $.ajax({
        type: 'POST',
        data: formData,
        success: function(result) {
            $('#success1').toast('show');
        }
    });
});