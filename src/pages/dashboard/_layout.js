import React from "react";
import { Switch, Route, Prompt } from "react-router-dom";

import Sidebar from "../../components/layout/sidebar";
import { CheckBackend } from "../_layout";
import Navbar from "../../components/layout/navbar";
import Footer from "../../components/layout/footer";
import "../../styles/layout.css";

import DashboardCore from "./core";

class Layout extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			saveStatus: "hidden",
			sidebarCollapsed: true
		}
		this.body = React.createRef();
		this.sidebar = React.createRef();
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
									<Route exact path="/dashboard/" render={() => window.location.pathname = "dashboard"}/>
									<Route exact path="/dashboard/:guildId" render={(props) => (<DashboardCore {...props} onChanged={() => this.requireSave()} ref={this.body} />)}/>
								</Switch>
							</div>
							<Prompt when={this.doesRequireSave()} message="You have unsaved changes, are you sure you want to leave this page?"/>
						</div>
					</CheckBackend>
				</main>
				<footer>
					<Footer/>
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

export default Layout;
