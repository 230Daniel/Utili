import React from "react";
import { Link } from "react-router-dom";

class Sidebar extends React.Component{
	render(){
		return(
			<div>
				<div>
					Sidebar
				</div>
				<div>
					{this.props.children}
				</div>
			</div>
		);
	}
}

export default Sidebar;
