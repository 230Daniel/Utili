import React from "react";
import { withRouter } from "react-router-dom";

class Test extends React.Component{
	constructor(props){
		super(props);
		this.guildId = this.props.match.params.guildId;
		this.state = {
			content: undefined
		};
	}

	render(){
		if(this.state.content === undefined) return null;
		return(
			<div>
				{this.state.content.guildId} <br/>
				{this.state.content.content}
				{JSON.stringify(this.state.content)}
			</div>	
		);
	}

	async componentDidMount(){
		var response = await fetch(`https://localhost:5001/dashboard/${this.guildId}/test`);
		var json = await response.json();
		this.setState({content: json});
	}
}

export default withRouter(Test);
