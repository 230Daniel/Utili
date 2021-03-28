import React from "react";
import { Helmet } from "react-helmet";

import Fade from "../../components/effects/fade"
import Load from "../../components/load";
import Subscriptions from "../../components/subscriptions";

import "../../styles/premium.css";

class PremiumThankYou extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			loading: true
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
							{this.renderContent()}
						</div>
					</div>
				</Fade>
			</>
		)
	}

	renderContent(){
		if(this.state.loading){
			return (
				<Load/>
			);
		} else {
			return(
				<Subscriptions alwaysDisplay={true}/>
			);
		}
	}

	componentDidMount(){
		setTimeout(() => {
			this.setState({loading: false});
		}, 2000);
	}
}

export default PremiumThankYou;
