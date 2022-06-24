import { parseHTML } from "cheerio";
import React from "react";
import { Link } from "react-router-dom";

class SidebarItem extends React.Component{
	render(){
		var path = window.location.pathname.replace(/\/+$/, '');
		var pages = path.split("/");
		var to = this.props.to;
		if(!path.match(/[0-9]$/)) pages.pop();

		to = to === "" ? `${this.path(pages)}` : `${this.path(pages)}/${to}`;
		
		return(
			<Link to={to} className="sidebar-item">
				{this.props.name}
			</Link>
		);
	}

	path(pages){
		return pages.join("/");
	}
}

export default SidebarItem;
