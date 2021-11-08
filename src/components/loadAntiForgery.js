import React from "react";
import { loadStripe } from '@stripe/stripe-js/pure';

import Load from "./load";
import Error from "../pages/error";

import { setAntiForgeryToken } from "../api/auth";
import Fade from "./effects/fade";

loadStripe.setLoadParameters({ advancedFraudSignals: false });

export default class LoadAntiForgery extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			loaded: false,
			showSpinner: false,
			error: false
		};
	}

	render() {
		if (this.state.error) {
			return (
				<Error code="503" shortDescription="Service Unavailable" longDescription="Failed to establish a connection to the server" />
			);
		}

		return this.state.loaded ? this.props.children : !this.state.showSpinner ? null : (
			<Fade>
				<div className="container" style={{ marginTop: "100px" }}>
					<Load />
				</div>
			</Fade>

		);
	}

	async componentDidMount() {
		try {

			setTimeout(() => {
				this.setState({ showSpinner: true });
			}, 1000);

			await setAntiForgeryToken();

			window.stripe = process.env.NODE_ENV == "production"
				? await loadStripe("pk_live_51Hcvk4B8DUEVWcSDwjMf0bvWv4NiSZizxfj495VdwB3UvqPZCNYt30781RdZ4tG8QnylVc98ywuj7k13wAec6cCq00I21LkJCn")
				: await loadStripe("pk_test_51Hcvk4B8DUEVWcSDhAutXkeJErW0lmmZvTahVkIxQij2cNun9JXuh3FfIt2QXlOQVO519maTYUn8V0tcT4fnuvMH000mz5kD2V");

			this.setState({ loaded: true });
		}
		catch {
			this.setState({ error: true });
		}
	}
}
