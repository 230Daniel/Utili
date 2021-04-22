import React from "react";
import { Link } from "react-router-dom";
import Helmet from "react-helmet";
import Fade from "../components/effects/fade";
import Divider from "../components/layout/divider";

class Contact extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			feature: ""
		};
		this.features = [];
		this.text = "dd";
		this.i = 0;
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Commands - Utili</title>
				</Helmet>
				<div className="container" style={{paddingBottom: "40px"}}>
					<Fade>
						<h1>Contact Us</h1>
						<Divider top="40" bottom="30">Discord Server</Divider>
						<div style={{fontSize: "16px"}}>
							<p>The fastest way to contact us is by joining our <a href="https://discord.gg/WsxqABZ" className="link">Discord server</a>.</p>
						</div>
						<Divider top="30" bottom="30">Email</Divider>
						<div style={{fontSize: "16px"}}>
							<span style={{color: "red"}}>There is currently an issue with our Email server - Please send everything to daniel.baynton@hotmail.com instead.<br/><br/></span>
							<p>Alternatively, contact us by email:</p>
							<ul>
								<li>Support - <a className="link" href="mailto:support@utili.xyz">support@utili.xyz</a></li>
								<li>Data protection - <a className="link" href="mailto:dpo@utili.xyz">dpo@utili.xyz</a></li>
								<li>Legal inquiries - <a className="link" href="mailto:legal@utili.xyz">legal@utili.xyz</a></li>
								<li>Everything else - <a className="link" href="mailto:info@utili.xyz">info@utili.xyz</a></li>
							</ul>
						</div>
					</Fade>
				</div>
				
			</>
		);
	}
}

export default Contact;
