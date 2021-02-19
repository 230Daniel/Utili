import React from "react";
import Helmet from "react-helmet";
import Fade from "../components/effects/fade";

import "../styles/index.css";

class Index extends React.Component{
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
					<title>Utili</title>
				</Helmet>
				<Fade>
					<div className="index-container">
						<div className="left">
							<img src="/Utili.svg"/>
						</div>
						<div className="right">
							<div display="block">
								<div className="title">Utili</div>
								<span className="subtitle" id="newline">A Discord bot with </span>
								<span className="subtitle text-highlight">{this.state.feature}</span> <br/>
								<a className="subtitle a-blue" href="/dashboard">Get started ➔</a>
							</div>
						</div>
					</div>
				</Fade>
			</>
		);
	}

	componentDidMount(){
		this.reset();
	}

	type(){
		if(this.i < this.text.length){
			this.setState({feature: this.state.feature + this.text.charAt(this.i)});
			this.i++
			setTimeout(() => this.type(), 80);
		} else {
			setTimeout(() => this.delete(), 1500);
		}
	}

	delete(){
		if(this.i > 0){
			this.setState({feature: this.state.feature.slice(0, -1)});
			this.i--;
			setTimeout(() => this.delete(), 80);
		} else {
			setTimeout(() => this.reset(), 300);
		}
	}

	reset(){
		if(this.features.length === 0){
			this.features = [
				"autopurge", 
				"channel mirroring", 
				"sticky notices",
				"message filtering",
				"logging",
				"vote channels",
				"join messages",
				"reputation",
				"join roles",
				"role linking",
				"role persist",
				"voice-text linking"];
		}
		
		var index = Math.floor(Math.random() * this.features.length);
		this.text = this.features[index];
		this.features.splice(index, 1);
		this.i = 0;
		this.type();
	}
}

export default Index;
