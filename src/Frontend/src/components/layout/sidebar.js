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
				<SidebarCategory title="Channels" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="Autopurge" to="autopurge"/>
					<SidebarItem name="Channel Mirroring" to="channelmirroring"/>
					<SidebarItem name="Sticky Notices" to="notices"/>
				</SidebarCategory>
				<SidebarCategory title="Messages" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="Message Filter" to="messagefilter"/>
					<SidebarItem name="Message Logging" to="messagelogs"/>
					<SidebarItem name="Message Pinning" to="messagepinning"/>
					<SidebarItem name="Message Voting" to="votechannels"/>
				</SidebarCategory>
				<SidebarCategory title="Users" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="Inactive Role" to="inactiverole"/>
					<SidebarItem name="Join Message" to="joinmessage"/>
					<SidebarItem name="Reputation" to="reputation"/>
				</SidebarCategory>
				<SidebarCategory title="Roles" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="Join Roles" to="joinroles"/>
					<SidebarItem name="Role Linking" to="rolelinking"/>
					<SidebarItem name="Role Persist" to="rolepersist"/>
				</SidebarCategory>
				<SidebarCategory title="Voice Channels" collapseSidebar={() => this.props.collapseSidebar()}>
					<SidebarItem name="Voice Link" to="voicelink"/>
					<SidebarItem name="Voice Roles" to="voiceroles"/>
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
