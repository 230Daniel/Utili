

$("[name|='form']").submit(function(e) {
    var formData = $(this).serialize();

    var buttonUsed = $("button[clicked=true]")[0].name;
    if (buttonUsed === "noajax") return true;

    $.ajax({
        type: "POST",
        data: formData,
        success: function(data, textStatus, xhr) {
            if (xhr.status === 201) {
                location.reload();
                return false;
            }
            successNotification();
        },
        error: function(xhr) {
            if (xhr.status === 429) {
                errorNotificationTooFast();
            } else {
                errorNotificationFailure();
            }
        }
    });
    return false;
});


$("form button[type=submit]").click(function() {
    $("button[type=submit]", $(this).parents("form")).removeAttr("clicked");
    $(this).attr("clicked", "true");
});

