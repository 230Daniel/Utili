import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router, Switch, Route, useParams } from "react-router-dom";
import Cookies from "universal-cookie";

import Index from "./pages/index";
import Layout from "./pages/_layout";
import Document from "./pages/document";

import Dashboard_Index from "./pages/dashboard/index";
import Dashboard_Core from "./pages/dashboard/core";
import Dashboard_Test from "./pages/dashboard/test";
import Error from "./pages/error";
import backend from "./config/backend.json";

ReactDOM.render(
	<Router>
		<>
			<Layout>
				<Switch>
					<Route exact path="/" component={Index}/>
					<Route exact path="/dashboard/" component={Dashboard_Index}/>
					<Route exact path="/dashboard/:guildId" component={Dashboard_Core}/>
					<Route exact path="/dashboard/:guildId/test" component={Dashboard_Test}/>
					<Route exact path="/return" render={() => Return()}/>
					<Route exact path="/invite" component={Invite}/>
					<Route exact path="/invite/:guildId" component={Invite}/>
					<Route exact path="/:document" component={Document}/>
				</Switch>
			</Layout>
		</>
	</Router>,
	document.getElementById("root")
);

function Return(){
	if(!window.location.href.includes("error")){
		const cookies = new Cookies();
		var returnPath = cookies.get("return_path");
		if(returnPath){
			window.location.pathname = returnPath;
			return;
		}
	}
	window.location.href = `${window.location.protocol}//${window.location.host}`;
}

function Invite(props){
	var { guildId } = useParams();

	const cookies = new Cookies();
	cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" } );

	var url = 	"https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
              	`&client_id=${backend.discord.clientId}` +
				`&redirect_uri=http%3A%2F%2F${window.location.host}%2Freturn`;
	
	if(guildId) url += `&guild_id=${guildId}`;
	window.location.href = url;
	return null;
}
