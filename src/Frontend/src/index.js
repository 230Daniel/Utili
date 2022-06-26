import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router, Switch, Route, useParams, Redirect } from "react-router-dom";
import Cookies from "universal-cookie";

import Layout from "./pages/_layout";
import Index from "./pages/index";
import Commands from "./pages/commands";
import Premium from "./pages/premium/index";
import CustomerPortal from "./pages/premium/customerportal";
import Checkout from "./pages/premium/checkout";
import Contact from "./pages/contact";
import DashboardLayout from "./pages/dashboard/_layout";
import Document from "./pages/document";

import DashboardIndex from "./pages/dashboard/index";

import { getClientId } from "./api/auth";
import PremiumServers from "./pages/premium/servers";
import PremiumThankYou from "./pages/premium/thankyou";

defineExtensions();

ReactDOM.render(
	<Router>
		<>
			<Switch>
				<Route path="/dashboard/*">
					<DashboardLayout />
				</Route>
				<Route path="*">
					<Layout>
						<Switch>
							<Route exact path="/" component={Index} />
							<Route exact path="/commands/" component={Commands} />
							<Route exact path="/dashboard/" component={DashboardIndex} />
							<Route exact path="/contact/" component={Contact} />
							<Route exact path="/return/" component={Return} />
							<Route exact path="/invite/" component={Invite} />
							<Route exact path="/invite/:guildId" component={Invite} />
							<Route path="/:document" component={Document} />
							{window.__config.enablePremium &&
								<>
									<Route exact path="/premium/" component={Premium} />
									<Route exact path="/premium/servers" component={PremiumServers} />
									<Route exact path="/premium/thankyou" component={PremiumThankYou} />
									<Route exact path="/premium/customerportal" component={CustomerPortal} />
									<Route exact path="/premium/checkout/:currency/:slots" component={Checkout} />
								</>
							}
						</Switch>
					</Layout>
				</Route>
			</Switch>
		</>
	</Router>,
	document.getElementById("root")
);

function Return() {
	const cookies = new Cookies();
	if (!window.location.href.includes("error")) {
		var returnPath = cookies.get("return_path");
		if (returnPath) {
			return (<Redirect to={returnPath} />);
		}
	}
	else {
		var returnPath = cookies.get("return_path_error");
		if (returnPath) {
			return (<Redirect to={returnPath} />);
		}
	}
	return (<Redirect to="/" />);
}

function Invite() {
	var { guildId } = useParams();

	const cookies = new Cookies();
	cookies.set("return_path", `/dashboard/${guildId}`, { path: "/", maxAge: 60, sameSite: "strict" });
	cookies.set("return_path_error", `/dashboard`, { path: "/", maxAge: 60, sameSite: "strict" });

	var url = "https://discord.com/api/oauth2/authorize?permissions=8&scope=bot&response_type=code" +
		`&client_id=${window.__config.clientId}` +
		`&redirect_uri=${encodeURIComponent(window.location.origin)}%2Freturn`;

	if (guildId) url += `&guild_id=${guildId}`;
	window.location.href = url;
	return null;
}

function defineExtensions() {
	Object.defineProperty(Array.prototype, "orderBy", {
		value: function orderBy(selector) {

			var sorted = [];
			var count = this.length;
			for (var i = 0; i < count; i++)
				sorted.push(selector(this[i]));
			sorted = sorted.sort();

			var reconstructed = [];
			for (var i = 0; i < count; i++) {
				var index = this.findIndex(x => selector(x) == sorted[i]);
				reconstructed.push(this[index]);
				this.splice(index, 1);
			}

			for (var i = 0; i < count; i++) {
				this.push(reconstructed[i]);
			}
			return reconstructed;
		},
		writable: true,
		configurable: true
	});

	Object.defineProperty(Array.prototype, "orderByInt", {
		value: function orderByInt(selector) {

			var sorted = [];
			var count = this.length;
			for (var i = 0; i < count; i++)
				sorted.push(+selector(this[i]));
			sorted = sorted.sort((a, b) => a - b);

			var reconstructed = [];
			for (var i = 0; i < count; i++) {
				var index = this.findIndex(x => +selector(x) == sorted[i]);
				reconstructed.push(this[index]);
				this.splice(index, 1);
			}

			for (var i = 0; i < count; i++) {
				this.push(reconstructed[i]);
			}
			return reconstructed;
		},
		writable: true,
		configurable: true
	});
}
