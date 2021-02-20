import React from "react";
import { Link } from "react-router-dom";

import "../../styles/dashboard-layout.css";

class Sidebar extends React.Component{
	constructor(props){
		super(props)
	}

	render(){
		return(
			<div className={`sidebar ${this.props.collapsed ? "collapsed" : ""}`}>
					
			</div>
		);
	}
}

export default Sidebar;
