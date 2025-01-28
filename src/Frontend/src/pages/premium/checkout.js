import React from "react";
import Load from "../../components/load";
import { get, post } from "../../api/auth";
import { withRouter } from "react-router";

class CustomerPortal extends React.Component {
	constructor(props) {
		super(props);
		this.currency = this.props.match.params.currency;
		this.slots = parseInt(this.props.match.params.slots);
	}

	render() {
		return (
			<>
				<h1>Redirecting...</h1>
				<Load loaded={false} />
			</>
		);

	}

	async componentDidMount() {
		var priceId = this.getPriceId(this.currency, this.slots);
		var response = await post("stripe/create-checkout-session", { priceId: priceId });
		var responseJson = await response.json();
		await window.stripe.redirectToCheckout({ sessionId: responseJson.sessionId });
	}

	getPriceId(currency, slots) {
		if (window.__config.stripeTestMode) {
			switch (slots) {
				case 1:
					if (currency === "gbp") return "price_1QmIfTB8DUEVWcSDmbgml4eG";
					if (currency === "usd") return "price_1QmIfTB8DUEVWcSDd0CfTjmC";
					if (currency === "eur") return "price_1QmIfUB8DUEVWcSDp5ztR4Ue";
					break;

				case 3:
					if (currency === "gbp") return "price_1I0bb9B8DUEVWcSDduM316AQ";
					if (currency === "usd") return "price_1I0bcoB8DUEVWcSDuzcORFfl";
					if (currency === "eur") return "price_1I0bdNB8DUEVWcSDpBaFzZMm";
					break;

				case 5:
					if (currency === "gbp") return "price_1I0bcOB8DUEVWcSDgqZAKvV1";
					if (currency === "usd") return "price_1I0cR7B8DUEVWcSDnikkbgGr";
					if (currency === "eur") return "price_1I0beqB8DUEVWcSDJaeyovfY";
					break;

				default:
					return null;
			}
		} else {
			switch (slots) {
				case 1:
					if (currency === "gbp") return "price_1I9tE0B8DUEVWcSDmXZo0tHg";
					if (currency === "usd") return "price_1I9tE0B8DUEVWcSDS7A4O1Yo";
					if (currency === "eur") return "price_1I9tE0B8DUEVWcSDt0axJ6Jy";
					break;

				case 3:
					if (currency === "gbp") return "price_1I9tFEB8DUEVWcSDeiZ30gkH";
					if (currency === "usd") return "price_1I9tFEB8DUEVWcSDdGJrifkV";
					if (currency === "eur") return "price_1I9tFEB8DUEVWcSDyTu8P9hn";
					break;

				case 5:
					if (currency === "gbp") return "price_1I9tJiB8DUEVWcSD23Zy6gBD";
					if (currency === "usd") return "price_1I9tJiB8DUEVWcSD7pIVqJJ3";
					if (currency === "eur") return "price_1I9tJiB8DUEVWcSDGKA09sOM";
					break;

				default:
					return null;
			}
		}
	}
}

export default withRouter(CustomerPortal);
