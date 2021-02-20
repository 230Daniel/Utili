import React from "react";
import { Link } from "react-router-dom";

import SidebarCategory from "./sidebarCategory";
import SidebarItem from "./sidebarItem";
import "../../styles/dashboard-layout.css";

class Sidebar extends React.Component{
	constructor(props){
		super(props)
	}

	render(){
		return(
			<div className={`sidebar ${this.props.collapsed ? "collapsed" : ""}`}>
				<div onClick={() => this.collapseSidebar()}>
					<SidebarItem name="Core" to=""/>
				</div>
				<SidebarCategory title="owooo" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="uguu" to="uguu"/>
					<SidebarItem name="ghhghh" to="ghhghh"/>
				</SidebarCategory>
				<SidebarCategory title="owooo" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="ddd" to="ddd"/>
					<SidebarItem name="eee" to="eee"/>
				</SidebarCategory>
			</div>
		);
	}

	collapseSidebar(){
		if(window.innerWidth <= 600)
			this.props.collapseSidebar();
	}
}

export default Sidebar;
