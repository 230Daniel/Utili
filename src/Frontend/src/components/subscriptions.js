import React from "react";
import { Link } from "react-router-dom";
import Divider from "./layout/divider";
import Load from "./load";
import { get } from "../api/auth";

class Subscriptions extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}

	render() {
		if (!this.state.subscriptions) return null;

		var showSubscriptions = this.state.subscriptions.filter(x => x.status !== 5).length > 0;
		var activeSubscriptions = this.state.subscriptions.filter(x => x.status === 0).length;
		var pastDueSubscriptions = this.state.subscriptions.filter(x => x.status === 1).length;
		var canceledSubscriptions = this.state.subscriptions.filter(x => x.status === 3).length;
		var incompleteSubscriptions = this.state.subscriptions.filter(x => x.status === 4).length;
		canceledSubscriptions += this.state.subscriptions.filter(x => x.status === 2).length;
		activeSubscriptions += this.state.subscriptions.filter(x => x.status === 6).length;

		var slots = 0;
		var pastDueSlots = 0;
		for (var i = 0; i < this.state.subscriptions.length; i++) {
			var subscription = this.state.subscriptions[i];
			if (subscription.status === 0 || subscription.status === 6) slots += subscription.slots;
			if (subscription.status === 1) pastDueSlots += subscription.slots;
		}

		if (!showSubscriptions && !this.props.alwaysDisplay) return null;

		return (
			<>
				<Divider bottom={25}>Your Subscriptions</Divider>
				<div className="your-subscriptions">
					{<span>You have {activeSubscriptions} active subscription{this.s(activeSubscriptions)} with {slots} slot{this.s(slots)}<br /></span>}
					{pastDueSubscriptions > 0 && <span style={{ color: "red" }}>You have {pastDueSubscriptions} overdue subscription{this.s(pastDueSubscriptions)} with {pastDueSlots} slot{this.s(pastDueSlots)}<br /></span>}
					{incompleteSubscriptions > 0 && <span style={{ color: "red" }}>You have {incompleteSubscriptions} incomplete subscription{this.s(incompleteSubscriptions)}<br /></span>}
					{canceledSubscriptions > 0 && <span style={{ color: "var(--colour-text-muted)" }}>You have {canceledSubscriptions} canceled subscription{this.s(canceledSubscriptions)}<br /></span>}
					<br />
					<Link className="link" to="/premium/servers">Select server{this.s(slots + pastDueSlots)} ➔</Link><br />
					<Link className="link" to="/premium/customerportal">Manage Billing ➔</Link><br />
				</div>
			</>
		);
	}

	async componentDidMount() {
		var response = await get(`premium/subscriptions`);
		this.state.subscriptions = await response?.json();
		this.setState({});
	}

	s(quantity) {
		return quantity == 1 ? "" : "s";
	}
}

export default Subscriptions;
