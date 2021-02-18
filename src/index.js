import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router, Switch, Route, useParams } from "react-router-dom";
import Cookies from "universal-cookie";

import Index from "./pages/index";
import Layout from "./pages/_layout";
import DashboardLayout from "./pages/dashboard/_layout";
import Document from "./pages/document";

import DashboardIndex from "./pages/dashboard/index";
import DashboardCore from "./pages/dashboard/core";
import DashboardTest from "./pages/dashboard/test";

import backend from "./config/backend.json";

ReactDOM.render(
	<Router>
		<>
			<Route path="/dashboard/*">
				<DashboardLayout>
					<Route exact path="/dashboard/" render={() => window.location.pathname = "dashboard"}/>
					<Route exact path="/dashboard/:guildId" component={DashboardCore}/>
					<Route exact path="/dashboard/:guildId/test" component={DashboardTest}/>
				</DashboardLayout>
			</Route>
			<Route path="*">
				<Layout>
					<Switch>
						<Route exact path="/" component={Index}/>
						<Route exact path="/dashboard/" component={DashboardIndex}/>
						<Route exact path="/return/" render={() => Return()}/>
						<Route exact path="/invite/" component={Invite}/>
						<Route exact path="/invite/:guildId" component={Invite}/>
						<Route exact path="/:document" component={Document}/>
					</Switch>
				</Layout>
			</Route>
		</>
	</Router>,
	document.getElementById("root")
);

function Return(){
	const cookies = new Cookies();
	window.location.search = "";
	if(!window.location.href.includes("error")){
		var returnPath = cookies.get("return_path");
		if(returnPath){
			window.location.href = `${window.location.origin}${returnPath}`;
			return;
		}
	}
	else{
		var returnPath = cookies.get("return_path_error");
		if(returnPath){
			window.location.href = `${window.location.origin}${returnPath}`;
			return;
		}
	}
	window.location.href = `${window.location.origin}`;
}

function Invite(props){
	var { guildId } = useParams();

	const cookies = new Cookies();
	cookies.set("return_path", `/dashboard/${guildId}`, { path: "/", maxAge: 60, sameSite: "strict" } );
	cookies.set("return_path_error", `/dashboard`, { path: "/", maxAge: 60, sameSite: "strict" } );

	var url = 	"https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
              	`&client_id=${backend.discord.clientId}` +
				`&redirect_uri=http%3A%2F%2F${window.location.host}%2Freturn`;
	
	if(guildId) url += `&guild_id=${guildId}`;
	window.location.href = url;
	return null;
}
