import React from "react";
import { Switch, Route, Prompt } from "react-router-dom";

import Sidebar from "../../components/layout/sidebar";
import { CheckBackend } from "../_layout";
import Navbar from "../../components/layout/navbar";
import Footer from "../../components/layout/footer";

import ResetPage from "../../components/effects/reset";

import "../../styles/layout.css";

import DashboardCore from "./core";

class Layout extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			requiresSave: false,
			saveStatus: "waiting"
		}
		this.body = React.createRef();
	}

	render(){
		return(
			<>
				<main>
					<Navbar/>
					<CheckBackend>
						<div className="dashboard-container">
							<Sidebar/>
								<div className="dashboard">
									<Switch>
										<Route exact path="/dashboard/" render={() => window.location.pathname = "dashboard"}/>
										<Route exact path="/dashboard/:guildId" render={(props) => (<DashboardCore {...props} onChanged={() => this.setState({requiresSave: true, saveStatus: "waiting"})} ref={this.body} />)}/>
									</Switch>
								</div>
							<Prompt when={this.state.requiresSave} message="You have unsaved changes, are you sure you want to leave this page?"/>
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

	renderSaveButton(){
		return(
			<div className={`saveNotification ${this.state.requiresSave ? "visible" : ""}`}>
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
				return "Changes Saved";
			case "error":
				return "Error";
		}
	}

	async save(){
		if(this.state.saveStatus !== "waiting") return;
		this.setState({saveStatus: "saving"});
		var success = await this.body.current.save();
		if(success){
			this.setState({saveStatus: "saved"});
			setTimeout(() => {
				this.setState({requiresSave: false});
			}, 500);
		}
		else this.setState({saveStatus: "error"});
	}

	onUnload = e => {
		if(this.state.requiresSave){
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
