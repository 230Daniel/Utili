import React from "react";
import Load from "../../components/load";
import { get } from "../../api/auth";

class CustomerPortal extends React.Component{
	render(){
		return(
			<>
				<h1>Redirecting...</h1>
				<Load loaded={false}/>
			</>
		);
		
	}

	async componentDidMount(){
		var response = await get("stripe/customer-portal");
		var json = await response.json();
		window.location.href = json.url;
	}
}

export default CustomerPortal;
