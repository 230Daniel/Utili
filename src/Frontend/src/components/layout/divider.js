import React from "react";

import "../../styles/divider.css";

class Divider extends React.Component {
	render() {
		return (
			<div className="divider" style={{ marginTop: this.props.top + "px", marginBottom: this.props.bottom + "px" }}>
				<div></div>
				<span>{this.props.children}</span>
			</div>
		);
	}
}

export default Divider;
