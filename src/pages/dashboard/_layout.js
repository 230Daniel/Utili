import React from "react";
import { Switch, Route } from "react-router-dom";

import Sidebar from "../../components/layout/sidebar";
import { CheckBackend } from "../_layout";

import "../../styles/layout.css";

import DashboardCore from "./core";

class Layout extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			requiresSave: false
		}
		this.body = React.createRef();
	}

	render(){
		return(
			<>
				<Sidebar>
					<Switch>
						<Route exact path="/dashboard/" render={() => window.location.pathname = "dashboard"}/>
						<Route exact path="/dashboard/:guildId" render={(props) => (<DashboardCore {...props} ref={this.body} />)}/>
					</Switch>
					<button onClick={() => this.save()}>save</button>
				</Sidebar>
			</>
		);
	}

	async save(){
		var success = await this.body.current.save();
		console.log(success);
	}
}

export default Layout;
