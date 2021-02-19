import React from "react";

import "../../styles/card.css";

class Card extends React.Component{
	render(){
		return(
			<div className="dashboard-card" style={{width: this.props.size}}>
				<div className="dashboard-card-title">{this.props.title}</div>
				{this.renderChildren()}
			</div>
		);
	}

	renderChildren(){
		return React.Children.map(this.props.children, child => {
			if (React.isValidElement(child)) {
				return React.cloneElement(child, { titleSize: this.props.titleSize, onChanged: this.props.onChanged });
			}
			return child;
		});
	}
}


export default Card;
