import React from "react";
import { Link } from "react-router-dom";

import "../../styles/dashboard-layout.css";

class Sidebar extends React.Component{
	render(){
		return(
			<div className="dashboard-container">
				<div className="sidebar">
					
				</div>
				<div className="dashboard">
					{this.props.children}
				</div>
			</div>
		);
	}
}

export default Sidebar;
