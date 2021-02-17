import React from "react";
import { getDetails as getAuthDetails, signIn, signOut } from "../../api/auth";

class Test extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			content: undefined,
			user: undefined
		};
	}

	render(){
		return(
			<div>
				{JSON.stringify(this.state.content)}
				{JSON.stringify(this.state.user)}
				<button onClick={async () => { await signIn() }}>Sign in</button>
				<button onClick={async () => { await signOut() }}>Sign Out</button>
			</div>	
		);
	}

	async componentDidMount(){
		var user = await getAuthDetails();
		this.setState({user: user});
		var response = await fetch(`https://localhost:5001/dashboard/${this.guildId}/test`, { mode: "cors", credentials: "include" });
		if(response.ok){
			var json = await response.json();
			this.setState({content: json});
		}
	}
}

export default Test;
