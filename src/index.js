import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import Cookies from "universal-cookie";

import Index from "./pages/index";
import Layout from "./pages/_layout";
import Document from "./pages/document";

import Dashboard_Index from "./pages/dashboard/index";
import Dashboard_Core from "./pages/dashboard/core";
import Dashboard_Test from "./pages/dashboard/test";

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
					<Route path="/:document" component={Document}/>
				</Switch>
			</Layout>
		</>
	</Router>,
	document.getElementById("root")
);

function Return(){
	const cookies = new Cookies();
	var returnPath = cookies.get("return_path");
	if(returnPath) window.location.pathname = returnPath;
	else window.location.pathname = "";
}