import React from "react";
import Load from "../../components/load";
import { get, post } from "../../api/auth";
import { withRouter } from "react-router";

class CustomerPortal extends React.Component{
	constructor(props){
		super(props);
		this.currency = this.props.match.params.currency;
		this.slots = parseInt(this.props.match.params.slots);
		this.stripe = window.Stripe("pk_live_51Hcvk4B8DUEVWcSDwjMf0bvWv4NiSZizxfj495VdwB3UvqPZCNYt30781RdZ4tG8QnylVc98ywuj7k13wAec6cCq00I21LkJCn");
	}
	
	render(){
		return(
			<>
				<h1>Redirecting...</h1>
				<Load loaded={false}/>
			</>
		);
		
	}

	async componentDidMount(){
		var priceId = this.getPriceId(this.currency, this.slots);
		var response = await post("stripe/create-checkout-session", { priceId: priceId });
		var responseJson = await response.json();
		await this.stripe.redirectToCheckout({sessionId: responseJson.sessionId});
	}

	getPriceId(currency, slots){
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

export default withRouter(CustomerPortal);
