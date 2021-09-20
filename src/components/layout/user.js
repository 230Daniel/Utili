import React from "react";
import { Nav, NavDropdown } from "react-bootstrap";

import { getDetails, signIn, signOut } from "../../api/auth";

class User extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			user: null
		};
	}

	render(){
		if(!this.state.user){
			return(
				<Nav.Link onClick={() => this.signIn()}>Sign in</Nav.Link>
			);
		}
		return(
			<NavDropdown className="user-dropdown" title={<><img src={this.state.user.avatarUrl}/>{this.state.user.username}</>}>
				<NavDropdown.Item onClick={() => this.signOut()}>Sign out</NavDropdown.Item>
			</NavDropdown>
		);
	}

	async componentDidMount(){
		this.getDetails();
	}

	async getDetails(){
		var user = await getDetails();
		this.setState({user: user})
	}

	async signIn(){
		await signIn();
		await this.getDetails();
	}

	async signOut(){
		await signOut();
		await this.getDetails();
	}
}

export default User;
