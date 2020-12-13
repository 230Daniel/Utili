
function onLoad() {
    sizeDynamicHeight();
    $(".info-hover").tooltip();
}

$(window).resize(function() {
    sizeDynamicHeight();
});

function sizeDynamicHeight() {
    var vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
    var navHeight = $("#navbar").outerHeight();

    var elements = document.getElementsByClassName("dynamic-height");
    for(var i = 0; i < elements.length; i++) {
        elements[i].style.height = vh - navHeight + "px";
    }
}
