
function onLoad() {

    sizeElements();
    $(".info-hover").tooltip();

    $("div[href]").click(function(){
        window.location.href = this.getAttribute("href");
    });
}

$(window).resize(function() {
    sizeElements();
});

function sizeElements() {
    var vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
    var navHeight = $("#navbar").outerHeight();

    var elements = document.getElementsByClassName("dynamic-height");
    for(var i = 0; i < elements.length; i++) {
        elements[i].style.height = vh - navHeight + "px";
    }
}