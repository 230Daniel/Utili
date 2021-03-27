import React from "react";
import { Link, Redirect } from "react-router-dom";
import { Helmet } from "react-helmet";
import { get } from "../../api/auth";

import Fade from "../../components/effects/fade"
import Divider from "../../components/layout/divider";

import "../../styles/premium.css";
import Subscriptions from "../../components/subscriptions";

class PremiumThankYou extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			loading: true,
			currency: "GBP",
			currencyLocked: false,
			subscriptions: []
		};
	}

	render(){
		return (
			<>
				<Helmet>
					<title>Thank You - Utili</title>
				</Helmet>
				<Fade>
					<div className="container premium">
						<h1>Thank you!</h1>
						<p style={{fontSize: "18px", textAlign: "center", marginTop: "-18px", color: "var(--colour-text-muted)"}}>Welcome to Utili Premium</p>
						<div style={{marginTop: "30px"}}>
							<Subscriptions alwaysShow={true}/>
						</div>
						
					</div>
				</Fade>
			</>
		)
	}
}

export default PremiumThankYou;
