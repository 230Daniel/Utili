import React from "react";
import { Link } from "react-router-dom";
import Helmet from "react-helmet";
import Fade from "../components/effects/fade";
import Divider from "../components/layout/divider";

class Commands extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			feature: ""
		};
		this.features = [];
		this.text = "dd";
		this.i = 0;
	}

	render(){
		return(
			<>
				<Helmet>
					<title>Commands - Utili</title>
				</Helmet>
				<div className="container" style={{paddingBottom: "40px"}}>
					<Fade>
						<h1>Commands</h1>
						<Divider top="40" bottom="30">Command Usage</Divider>
						<div className="commands-section" style={{padding: "8px"}}>
							<p>All commands start with your command prefix which you can configure on the dashboard.<br/>
							Do not include brackets when executing commands.</p>
							<ul>
								<li>[arg] - Required argument</li>
								<li>(arg) - Optional argument</li>
							</ul>
							<p>If you require assistance, don't hesitate to <Link className="link" to="contact">contact us</Link>.</p>
						</div>
						<Divider top="22" bottom="30">Information</Divider>
						<CommandsSection>
							<tr>
								<td>help</td>
								<td>Get linked to this website</td>
							</tr>
							<tr>
								<td>about</td>
								<td>Get some information about Utili</td>
							</tr>
							<tr>
								<td>ping</td>
								<td>Get the current status of Utili</td>
							</tr>
						</CommandsSection>
						<Divider top="30" bottom="30">Utility</Divider>
						<CommandsSection>
							<tr>
								<td>prune [amount]</td>
								<td>Delete an amount of messages from the channel, starting from the bottom</td>
							</tr>
							<tr>
								<td>prune (amount) [before/after] [message id]</td>
								<td>Delete messages before or after a specific message</td>
							</tr>
							<tr>
								<td>react (channel)  [message id] [emoji]</td>
								<td>Add a reaction to a message</td>
							</tr>
							<tr>
								<td>random</td>
								<td>Select a random user from the server</td>
							</tr>
							<tr>
								<td>random (channel) [message id] [emoji]</td>
								<td>Select a random user which reacted to a message with an emoji</td>
							</tr>
							<tr>
								<td>whohas [role] (page)</td>
								<td>Get a paged list of users who have a role</td>
							</tr>
						</CommandsSection>
						<Divider top="30" bottom="30">Message Pinning</Divider>
						<CommandsSection>
							<tr>
								<td>pin (channel) [message id] (pin channel)</td>
								<td>Pin a message and clone it to your pin channel</td>
							</tr>
						</CommandsSection>
						<Divider top="30" bottom="30">Reputation</Divider>
						<CommandsSection>
							<tr>
								<td>rep [user]</td>
								<td>Get a user's reputation</td>
							</tr>
							<tr>
								<td>rep top</td>
								<td>Get the leaderboard</td>
							</tr>
							<tr>
								<td>rep give [user] [amount]</td>
								<td>Give a user some reputation</td>
							</tr>
							<tr>
								<td>rep take [user] [amount]</td>
								<td>Take some reputation from a user</td>
							</tr>
							<tr>
								<td>rep set [user] [amount]</td>
								<td>Set a user's reputation</td>
							</tr>
						</CommandsSection>
					</Fade>
				</div>
				
			</>
		);
	}
}

export default Commands;

class CommandsSection extends React.Component{
	render(){
		return(
			<table className="commands-section">
				<colgroup>
					<col span="1" style={{width: "40%"}}/>
					<col span="1" style={{width: "60%"}}/>
				</colgroup>
				<tbody>
					{this.props.children}
				</tbody>
			</table>
		);
	}
}