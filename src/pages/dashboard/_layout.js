import React from "react";
import { Switch, Route, Redirect, Prompt, withRouter } from "react-router-dom";

import Sidebar from "../../components/layout/sidebar";
import { CheckBackend } from "../_layout";
import Navbar from "../../components/layout/navbar";
import Error from "../error";
import "../../styles/layout.css";

import Core from "./core";
import Autopurge from "./autopurge";
import ChannelMirroring from "./channelmirroring";
import InactiveRole from "./inactiverole";
import JoinMessage from "./joinmessage";
import JoinRoles from "./joinroles";
import MessageFilter from "./messagefilter";
import MessageLogs from "./messagelogs";
import MessagePinning from "./messagepinning";
import Notices from "./notices";
import Reputation from "./reputation"
import RolePersist from "./rolepersist";
import VoiceLink from "./voicelink";
import VoiceRoles from "./voiceroles";
import VoteChannels from "./votechannels";

class Layout extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			saveStatus: "hidden",
			sidebarCollapsed: true
		}
		this.body = React.createRef();
		this.sidebar = React.createRef();

		this.props.history.listen((location, action) => {
			this.setState({
				saveStatus: "hidden"
			});
		});
	}

	render(){
		return(
			<>
				<main>
					<Navbar buttonLeft={true} onButtonLeftClick={() => this.toggleSidebar()}/>
					<CheckBackend>
						<div className={`dashboard-container${this.state.sidebarCollapsed ? "" : " collapsed"}`}>
							<Sidebar ref={this.sidebar} collapsed={this.state.sidebarCollapsed} collapseSidebar={() => this.toggleSidebar(true)}/>
							<div className="dashboard">
								<Switch>
									<Redirect exact from="/dashboard/" to="/dashboard"/>
									<Route exact path="/dashboard/:guildId" render={(props) => (<Core {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/autopurge" render={(props) => (<Autopurge {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/channelmirroring" render={(props) => (<ChannelMirroring {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/inactiverole" render={(props) => (<InactiveRole {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/joinmessage" render={(props) => (<JoinMessage {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/joinroles" render={(props) => (<JoinRoles {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/messagefilter" render={(props) => (<MessageFilter {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/messagelogs" render={(props) => (<MessageLogs {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/messagepinning" render={(props) => (<MessagePinning {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/notices" render={(props) => (<Notices {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/reputation" render={(props) => (<Reputation {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/rolepersist" render={(props) => (<RolePersist {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/voicelink" render={(props) => (<VoiceLink {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/voiceroles" render={(props) => (<VoiceRoles {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route exact path="/dashboard/:guildId/votechannels" render={(props) => (<VoteChannels {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
									<Route component={NotFound}/>
								</Switch>
							</div>
							<Prompt when={this.doesRequireSave()} message="You have unsaved changes, are you sure you want to leave this page?"/>
						</div>
					</CheckBackend>
				</main>
				<footer>
					{this.renderSaveButton()}
				</footer>
			</>
		);
	}

	doesRequireSave(){
		switch(this.state.saveStatus){
			case "waiting":
			case "saving":
			case "error":
				return true;
			default:
				return false;
		}
	}

	shouldSaveButtonBeVisible(){
		switch(this.state.saveStatus){
			case "waiting":
			case "saving":
			case "saved":
			case "error":
				return true;
			default:
				return false;
		}
	}

	renderSaveButton(){
		return(
			<div className={`saveNotification ${this.shouldSaveButtonBeVisible() ? "visible" : ""}`}>
				<button className={`savebutton-${this.state.saveStatus}`} onClick={async () => await this.save()}>{this.getSaveButtonContent()}</button>
			</div>
		);
	}

	getSaveButtonContent(){
		switch(this.state.saveStatus){
			case "waiting":
				return "Save Changes";
			case "saving":
				return "Saving...";
			case "saved":
			case "hidden":
				return "Changes Saved";
			case "error":
				return "Error";
		}
	}

	toggleSidebar(collapse){
		if(collapse === undefined) collapse = !this.state.sidebarCollapsed;
		this.setState({sidebarCollapsed: collapse});
	}

	requireSave(){
		if(!this.doesRequireSave()){
			this.setState({saveStatus: "waiting"});
		}
	}

	async save(){
		if(!this.doesRequireSave()) return;
		this.setState({saveStatus: "saving"});
		var success = await this.body.current.save();
		if(success){
			this.setState({saveStatus: "saved"});
			setTimeout(() => {
				if(!this.doesRequireSave())	this.setState({saveStatus: "hidden"});
			}, 500);
		}
		else this.setState({saveStatus: "error"});
	}

	onUnload = e => {
		if(this.doesRequireSave()){
			e.preventDefault();
			e.returnValue = "You have unsaved changes";
			return "You have unsaved changes";
		}
	 }
 
	 componentDidMount() {
		window.addEventListener("beforeunload", this.onUnload);
	 }
 
	 componentWillUnmount() {
		 window.removeEventListener("beforeunload", this.onUnload);
	 }
}

function NotFound(props){
	return(
		<Error 
		code="404" 
		shortDescription="Not found" 
		longDescription="Sorry, we couldn't find that page on our servers."
		/>
	);
}

export default withRouter(Layout);
