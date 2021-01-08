function onLoad() {
    sizeElements();
    hideElements();

    $(".info-hover").tooltip();

    $("div[href]").click(function(){
        window.location.href = this.getAttribute("href");
    });

    $("select[hideshowcomplex]").on("change", function() {
        var complex = this.parentNode.parentNode.childNodes[7];
        if(this.value === "8"){
            complex.removeAttribute("hidden");
        }
        else{
            complex.setAttribute("hidden", "");
        }
    });

    sessionStorage.removeItem("sidebarpos");
}

$(window).resize(function() {
    sizeElements();
    hideElements();
});

function sizeElements() {
    var vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
    var vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
    var mobile = window.mobileAndTabletCheck();
    var navHeight = $("#navbar").outerHeight();

    if (mobile) {
        $(".mobile-grow").each(function(){
            var height = $(this).height();
            console.log($(this).height(height + 5));
            $(this).css("display", "flex");
            $(this).css("align-items", "center");
        });

        $(".mobile-grow-children > *").each(function(){
            var height = $(this).height();
            console.log($(this).height(height + 5));
            $(this).css("display", "flex");
            $(this).css("align-items", "center");
        });
    }

    var elements = document.querySelectorAll(".dynamic-height");
    for(var i = 0; i < elements.length; i++) {
        var children = $(".dynamic-height > :not(.footer)");
        var footers = elements[i].querySelectorAll(".footer");
        for(var j = 0; j < footers.length; j++) {
            var margin = vh - children.outerHeight(true) - navHeight;
            if (margin < vh * 0.03) margin = vh * 0.03;
            footers[i].style.marginTop = margin + "px";
        }

        elements[i].style.height = vh - navHeight + "px";
    }

    $(".footer").removeAttr("hidden");

    if (mobile || vw <= 675) {
        $(".container").css("max-width", "100vw");
    } else {
        $(".container").css("max-width", "75vw");
    }
}

function hideElements() {
    var vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
    var mobile = window.mobileAndTabletCheck();

    if (mobile) {
        $(".info-hover").attr("hidden", "");
    } else {
        $(".info-hover").removeAttr("hidden", "");
    }

    if (mobile || vw <= 675) {
        $("#sidebarToggler").removeAttr("hidden", "");
    } else {
        $("#sidebarToggler").attr("hidden", "");
        toggleSidebar(true);
    }
}

function scrollSidebar() {
    var scrollpos = sessionStorage.getItem("sidebarpos");
    var sidebar = document.querySelector("#sidebar");
    if(sidebar && scrollpos) sidebar.scrollTo(0, scrollpos);
    sessionStorage.removeItem("sidebarpos");
}

function toggleSidebar(onlyShow) {
    var sidebar = $("#sidebar");
    var main = $("#main");

    if (sidebar.hasAttr("hidden")) {
        sidebar.removeAttr("hidden");
        main.css("width", "calc(100vw - 225px)");
    } else if(!onlyShow) {
        sidebar.attr("hidden", "");
        main.css("width", "100vw");
    }
}

$.fn.hasAttr = function(name) {  
    return this.attr(name) !== undefined;
};

// https://stackoverflow.com/questions/11381673/detecting-a-mobile-browser
window.mobileAndTabletCheck = function() {
  var check = false;
  (function(a){if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|android|ipad|playbook|silk/i.test(a)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0,4))) check = true;})(navigator.userAgent||navigator.vendor||window.opera);
  return check;
};
