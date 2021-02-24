import React from "react";

import "../../styles/card.css";

class Card extends React.Component{
	render(){
		return(
			<div className={`dashboard-card`} style={{width: this.props.size}}>
				{this.props.title && 
					<div className="dashboard-card-title">{this.props.title}
					{this.props.onRemoved &&
					<div className="dashboard-card-list-component-selected-remove" onClick={this.props.onRemoved}>
					<img width={20} src="/bin.svg"/>
				</div>}
				</div>}
				{this.renderChildren()}
			</div>
		);
	}

	renderChildren(){
		return React.Children.map(this.props.children, child => {
			if (React.isValidElement(child)) {
				return React.cloneElement(child, { titleSize: this.props.titleSize, inputSize: this.props.inputSize, onChanged: this.props.onChanged });
			}
			return child;
		});
	}
}


export default Card;
