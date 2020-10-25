

$("[name|='form']").submit(function() {
    var formData = $(this).serialize();
    console.log(formData);
    $.ajax({
        type: "POST",
        data: formData,
        success: function() {
            $("#success").toast("show");
        },
        error: function(xhr) {
            console.log(xhr.error);
            if (xhr.status == 469) {
                $("#ratelimit").toast("show");
            } else {
                $("#error").toast("show");
            }
        }
    });
    return false;
});