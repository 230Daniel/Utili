
var stripe = window.Stripe("pk_test_51Hcvk4B8DUEVWcSDhAutXkeJErW0lmmZvTahVkIxQij2cNun9JXuh3FfIt2QXlOQVO519maTYUn8V0tcT4fnuvMH000mz5kD2V");

function checkout(slots, currency) {

    var price = "";

    switch (slots) {
        case 1:
            if (currency === "gbp") price = "price_1I0bUCB8DUEVWcSDYIEsUvWA";
            if (currency === "usd") price = "price_1I0bUeB8DUEVWcSD6ksIYGra";
            if (currency === "eur") price = "price_1I0bWFB8DUEVWcSDut7TNTEJ";
            break;

        case 3:
            if (currency === "gbp") price = "price_1I0bb9B8DUEVWcSDduM316AQ";
            if (currency === "usd") price = "price_1I0bcoB8DUEVWcSDuzcORFfl";
            if (currency === "eur") price = "price_1I0bdNB8DUEVWcSDpBaFzZMm";
            break;

        case 5:
            if (currency === "gbp") price = "price_1I0bcOB8DUEVWcSDgqZAKvV1";
            if (currency === "usd") price = "price_1I0cR7B8DUEVWcSDnikkbgGr";
            if (currency === "eur") price = "price_1I0beqB8DUEVWcSDJaeyovfY";
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