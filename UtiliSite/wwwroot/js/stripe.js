
var stripe = window.Stripe("pk_test_51Hcvk4B8DUEVWcSDhAutXkeJErW0lmmZvTahVkIxQij2cNun9JXuh3FfIt2QXlOQVO519maTYUn8V0tcT4fnuvMH000mz5kD2V");

function checkout(price) {

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