import React from "react";
import { getDetails as getAuthDetails, signIn, signOut, get, post } from "../../api/auth";

class Test extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			content: undefined
		};
	}

	render(){
		return(
			<div>
				{JSON.stringify(this.state.content)}
			</div>	
		);
	}

	async componentDidMount(){
		var response = await get(`dashboard/${this.guildId}/test`);
		if(response){
			var json = await response.json();
			this.setState({content: json});
		} else this.props.history.push("/dashboard");
	}
}

export default Test;
