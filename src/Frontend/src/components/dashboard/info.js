import React from "react";

import "../../styles/card.css";

class Card extends React.Component {
	render() {
		return (
			<div className="dashboard-card ml-auto" style={{ width: this.props.size }}>
				<div className="dashboard-card-title">{this.props.title}</div>
				<div className="dashboard-card-info-content">
					{this.props.children}
				</div>
			</div>
		);
	}
}


export default Card;
