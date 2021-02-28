import React from "react";
import Loader from "react-loader-spinner";
import Fade from "./effects/fade";
import Cookies from "universal-cookie";

class Load extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			theme: "dark"
		}
	}

	render(){
		if(!this.props.loaded){
			return(
				<Fade key={0}>
					<div style={{display: "flex", justifyContent: "center", marginTop: "75px"}}>
					<Loader type="ThreeDots" color={this.getColour()} height={60} width={60}/>
					</div>
				</Fade>
			);
		}
		else{
			return(
				<Fade key={1}>
					{this.props.children}
				</Fade>
			);
		}
	}

	getColour(){
		switch(this.state.theme){
			case "dark":
				return "white";
			case "classic":
				return "white";
			case "light":
				return "black";
			default:
				return "red";
		}
	}

	componentDidMount(){
		var cookies = new Cookies();
		var theme = cookies.get("theme");
		if(!theme) theme = "dark";
		this.setState({theme: theme});
	}
}

export default Load;
