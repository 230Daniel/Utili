import React from "react";
import { Link, Redirect } from "react-router-dom";
import { Helmet } from "react-helmet";
import { get } from "../../api/auth";

import { getBestSupportedCurrency } from "../../helpers/currency";

import Fade from "../../components/effects/fade";
import Load from "../../components/load";
import Divider from "../../components/layout/divider";
import PriceSelector from "../../components/priceSelector";
import Subscriptions from "../../components/subscriptions";

import "../../styles/premium.css";

class PremiumIndex extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			loading: true,
			currency: "GBP",
			currencyLocked: false,
			subscriptions: []
		};
	}

	render() {
		return (
			<>
				<Helmet>
					<title>Premium - Utili</title>
				</Helmet>
				<Fade>
					<div className="container premium">
						<h1>Premium</h1>
						<Load loaded={!this.state.loading}>
							<p>Premium benefits include...</p>
							<ul>
								<li>More frequent autopurge purges</li>
								<li>Unlimited message logs storage</li>
								<li>Unlimited role linking rules</li>
								<li>Up to 5 emojis in vote channels</li>
								<li>The option to auto-kick inactive users</li>
								<li>A coloured role on the Utili discord server</li>
							</ul>
							<Subscriptions />
							<Divider top={25} bottom={25}>Premium Plans</Divider>
							<PriceSelector currency={this.state.currency} onChanged={(currency) => this.setCurrency(currency)} />
							<div className="premium-plans">
								<PremiumPlan servers={1} price={this.getPrice(1)} currencyCode={this.state.currency}>One Utili Premium slot for a server of your choice</PremiumPlan>
								<PremiumPlan servers={3} price={this.getPrice(3)} currencyCode={this.state.currency}>Three Utili Premium slots for servers of your choice</PremiumPlan>
								<PremiumPlan servers={5} price={this.getPrice(5)} currencyCode={this.state.currency}>Five Utili Premium slots for servers of your choice</PremiumPlan>
							</div>
						</Load>
					</div>
				</Fade>
			</>
		);
	}

	getPrice(servers) {
		var prices;
		switch (this.state.currency) {
			case "GBP":
				prices = [1.5, 0, 3.75, 0, 5];
				break;
			case "EUR":
				prices = [1.75, 0, 4.25, 0, 5.5];
				break;
			case "USD":
				prices = [2, 0, 5, 0, 7];
				break;
		}
		return prices[servers - 1];
	}

	setCurrency(currency) {
		if (this.state.currencyLocked) return;
		this.setState({ currency: currency });
	}

	async componentDidMount() {
		var response = await get(`stripe/currency`);
		var currency = await response.json();

		if (currency) {
			this.state.currency = currency.toUpperCase();
			this.state.currencyLocked = true;
		} else {
			this.state.currency = getBestSupportedCurrency();
		}

		this.setState({ loading: false });
	}
}

class PremiumPlan extends React.Component {
	render() {
		return (
			<div className="premium-plan">
				<h2>{this.props.servers} Server{this.props.servers == 1 ? "" : "s"}</h2>
				<span className="muted">{this.props.children}</span>
				<span className="price">{this.getCurrency()}{this.props.price.toFixed(2)}</span>
				<span className="price-sub">per month</span>
				<span className="subscribe-sub">{this.getCurrency()}{(this.props.price / this.props.servers).toFixed(2)} / server</span>
				<Link className="subscribe" to={`/premium/checkout/${this.props.currencyCode.toLowerCase()}/${this.props.servers}`}>
					Subscribe
				</Link>
			</div>
		);
	}

	getCurrency() {
		switch (this.props.currencyCode) {
			case "GBP":
				return "£";
			case "EUR":
				return "€";
			case "USD":
				return "$";
		}
	}
}

export default PremiumIndex;
