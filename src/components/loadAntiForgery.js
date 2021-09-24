import React from "react";
import Load from "./load";

import Error from "../pages/error";

import { setAntiForgeryToken } from "../api/auth";
import Fade from "./effects/fade";

export default class LoadAntiForgery extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			loaded: false,
			showSpinner: false,
			error: false
		}
	}

	render(){
		if(this.state.error){
			return(
				<Error code="503" shortDescription="Service Unavailable" longDescription="Failed to establish a connection to the server"/>
			)
		}

		return this.state.loaded ? this.props.children : !this.state.showSpinner ? null : (
			<Fade>
				<div className="container" style={{marginTop: "100px"}}>
					<Load/>
				</div>
			</Fade>
			
		);
	}

	async componentDidMount(){
		try{
			setTimeout(() => {
				this.setState({showSpinner: true});
			}, 1000);
			await setAntiForgeryToken();
			this.setState({loaded: true});
		}
		catch{
			this.setState({error: true});
		}
	}
}