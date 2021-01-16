
var stripe = window.Stripe("pk_live_51Hcvk4B8DUEVWcSDwjMf0bvWv4NiSZizxfj495VdwB3UvqPZCNYt30781RdZ4tG8QnylVc98ywuj7k13wAec6cCq00I21LkJCn");

function checkout(slots, currency) {

    var price = "";

    switch (slots) {
        case 1:
            if (currency === "gbp") price = "price_1I9tE0B8DUEVWcSDmXZo0tHg";
            if (currency === "usd") price = "price_1I9tE0B8DUEVWcSDS7A4O1Yo";
            if (currency === "eur") price = "price_1I9tE0B8DUEVWcSDt0axJ6Jy";
            break;

        case 3:
            if (currency === "gbp") price = "price_1I9tFEB8DUEVWcSDeiZ30gkH";
            if (currency === "usd") price = "price_1I9tFEB8DUEVWcSDdGJrifkV";
            if (currency === "eur") price = "price_1I9tFEB8DUEVWcSDyTu8P9hn";
            break;

        case 5:
            if (currency === "gbp") price = "price_1I9tJiB8DUEVWcSD23Zy6gBD";
            if (currency === "usd") price = "price_1I9tJiB8DUEVWcSD7pIVqJJ3";
            if (currency === "eur") price = "price_1I9tJiB8DUEVWcSDGKA09sOM";
            break;
    }

    var createCheckoutSession = function(priceId) {
        return fetch("/create-checkout-session", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                priceId: priceId
            })
        }).then(function(result) {
            return result.json();
        });
    };

    createCheckoutSession(price).then(function(data) {
        stripe
            .redirectToCheckout({
                sessionId: data.sessionId
            })
            .then(handleResult);
    });
}

function billingPortal() {
    fetch('/customer-portal', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then((response) => response.json())
        .then((data) => {
            window.location.href = data.url;
        })
        .catch((error) => {
            console.error('Error:', error);
        });
}

function changeCurrency() {

    switch ($("#currencySelect").val()) {
    case "gbp":
        $("#price-1").html("£1.50");
        $("#price-1-per").html("£1.50 / server");
        $("#price-3").html("£3.75");
        $("#price-3-per").html("£1.25 / server");
        $("#price-5").html("£5.00");
        $("#price-5-per").html("£1.00 / server");
        break;

    case "usd":
        $("#price-1").html("$2.00");
        $("#price-1-per").html("$2.00 / server");
        $("#price-3").html("$5.00");
        $("#price-3-per").html("$1.67 / server");
        $("#price-5").html("$7.00");
        $("#price-5-per").html("$1.40 / server");
        break;

    case "eur":
        $("#price-1").html("€1.75");
        $("#price-1-per").html("€1.75 / server");
        $("#price-3").html("€4.25");
        $("#price-3-per").html("€1.42 / server");
        $("#price-5").html("€5.50");
        $("#price-5-per").html("€1.10 / server");
        break;
    }
}