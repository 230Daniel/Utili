import React from "react";

import "../../styles/fade.css";

class Fade extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			isVisible: false
		};
	}

	render(){
		return(
			<div className={`fade-in${this.state.isVisible ? " visible" : ""}`}>
				{this.props.children}
			</div>
		);
	}

	componentDidMount(){
		setTimeout(() => {
			console.log("fade in");
			this.setState({isVisible: true});
		}, 100);
	}
}

export default Fade;
