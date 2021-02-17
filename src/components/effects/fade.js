import React from "react";
import VizSensor from "react-visibility-sensor";

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
			<VizSensor
			minTopValue={this.props.minTopValue}
			partialVisibility={true}
			onChange={(isVisible) =>{
				this.onVisibleChange(isVisible);
			}}>
				<div className={`fade-in${this.state.isVisible ? " visible" : ""}`}>
					{this.props.children}
				</div>
			</VizSensor>
		);
	}

	onVisibleChange(isVisible){
		if(this.props.fadeOut || !this.state.isVisible){
			setTimeout(() => {
				this.setState({isVisible: isVisible});
			}, this.props.delay);
		}
	}
}

export default Fade;
