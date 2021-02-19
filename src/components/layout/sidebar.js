import React from "react";
import { Link } from "react-router-dom";

import "../../styles/dashboard-layout.css";

class Sidebar extends React.Component{
	constructor(props){
		super(props)
		this.state = {
			collapsed: "true"
		}
	}

	render(){
		console.log(this.state.collapsed);
		return(
			<div className={`sidebar ${this.state.collapsed ? "collapsed" : ""}`}>
					
			</div>
		);
	}

	async toggle(){
		await this.setState({collapsed: !this.state.collapsed});
		var container = document.getElementsByClassName("dashboard-container")[0];
		if(!this.state.collapsed){
			container.classList.add("collapsed");
		} else {
			container.classList.remove("collapsed");
		}
	}
}

export default Sidebar;
