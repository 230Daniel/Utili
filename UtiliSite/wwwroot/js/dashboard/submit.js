$("[name|='form']").submit(function(e) {
    var formData = $(this).serialize();
    $.ajax({
        type: "POST",
        data: formData,
        success: function() {
            $("#success").toast("show");
        },
        error: function(xhr) {
            if (xhr.status == 469) {
                $("#ratelimit").toast("show");
            } else {
                $("#error").toast("show");
            }
        }
    });
    return false;
});