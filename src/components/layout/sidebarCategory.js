import React from "react";

class SidebarCategory extends React.Component{
	constructor(props){
		super(props);
		var tmp = "uguu";
		var page = window.location.pathname.split("/").pop();
		if(page.match(/[0-9]$/)) page = "";
		var collapsed = !this.props.children.some(x => x.props.to === page);
		this.state = {
			collapsed: collapsed
		};
	}

	render(){
		return(
			<div className={`sidebar-category${this.state.collapsed ? " collapsed" : ""}`} onClick={(e) => this.toggle(e)}>
				<div className="sidebar-category-title">{this.props.title}</div>
				<div className="sidebar-category-items">
					{this.props.children}
				</div>
			</div>
		);
	}

	toggle(e){
		if(e.target.classList.contains("sidebar-category-title")){
			this.setState({collapsed: !this.state.collapsed});
		} else if(window.innerWidth <= 600) { 
			this.props.collapseSidebar();
		}
	}
}

export default SidebarCategory;
