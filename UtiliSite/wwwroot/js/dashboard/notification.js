function successNotification() {
    document.getElementById("navbar").style.backgroundColor = "#43b581";
    document.getElementById("main-nav").setAttribute("hidden", "");
    document.getElementById("success").removeAttribute("hidden");
    setTimeout(() => {
        document.getElementById("navbar").style.backgroundColor = "#e9ecef";
        document.getElementById("success").setAttribute("hidden", "");
        document.getElementById("main-nav").removeAttribute("hidden");
    }, 2500);
    
};
function errorNotificationTooFast() {
    document.getElementById("navbar").style.backgroundColor = "#b54343";
    document.getElementById("main-nav").setAttribute("hidden", "");
    document.getElementById("error-too-fast").removeAttribute("hidden");
    setTimeout(() => {
        document.getElementById("navbar").style.backgroundColor = "#e9ecef";
        document.getElementById("error-too-fast").setAttribute("hidden", "");
        document.getElementById("main-nav").removeAttribute("hidden");
    }, 2500);
};

function errorNotificationFailure() {
    document.getElementById("navbar").style.backgroundColor = "#b54343";
    document.getElementById("main-nav").setAttribute("hidden", "");
    document.getElementById("error-failure").removeAttribute("hidden");
    setTimeout(() => {
        document.getElementById("navbar").style.backgroundColor = "#e9ecef";
        document.getElementById("error-failure").setAttribute("hidden", "");
        document.getElementById("main-nav").removeAttribute("hidden");
    }, 2500);
};