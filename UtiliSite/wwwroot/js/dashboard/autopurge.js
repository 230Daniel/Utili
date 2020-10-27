


$("[name|='removeChannel']").click(function(e) {
    var formData = $(this).serialize();
    console.log(formData);
    $.ajax({
        type: "POST",
        data: formData,
        success: function() {
            $("#success").toast("show");
        },
        error: function(xhr) {
            if (xhr.status == 429) {
                $("#ratelimit").toast("show");
            } else {
                $("#error").toast("show");
            }
        }
    });
    return false;
});

$("[name|='addChannel']").click(function(e) {
    var formData = $(this).serialize();
    console.log(formData);
    $.ajax({
        type: "POST",
        data: formData,
        success: function() {
            $("#success").toast("show");
        },
        error: function(xhr) {
            if (xhr.status == 429) {
                $("#ratelimit").toast("show");
            } else {
                $("#error").toast("show");
            }
        }
    });
    return false;
});