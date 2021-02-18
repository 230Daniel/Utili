import React from "react";

import Navbar from "../components/layout/navbar";
import Footer from "../components/layout/footer";

import Error from "../pages/error";
import { ping as pingApi } from "../api/ping";
import "../styles/layout.css";
import { Helmet } from "react-helmet";

export default function Layout(props){
	return(
		<>
			<main>
				<Navbar/>
				<CheckBackend>
					{props.children}
				</CheckBackend>
			</main>
			<footer>
				<Footer/>
			</footer>
		</>
	);
}

class CheckBackend extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			ok: true
		}
	}

	render(){
		if(this.state.ok){
			return(
				<>
					{this.props.children}
				</>
			);
		}
		return(
			<>
				<Error code="503" shortDescription="Service Unavailable" longDescription="Failed to establish a connection to the server"/>
			</>
		);
	}

	async componentDidMount(){
		await this.ping();
	}

	async ping(){
		var ping = await pingApi();
		if(ping !== this.state.ok){
			if(ping) window.location.reload();
			else{
				this.setState({ok: ping});
				setTimeout(() => {this.ping();}, 10000);
			}
			
		}
	}
}