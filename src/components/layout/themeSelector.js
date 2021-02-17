import React from "react";
import { NavDropdown } from "react-bootstrap";
import Cookies from "universal-cookie";

class ThemeSelector extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			theme: undefined
		};
	}

	render(){
		this.apply();
		return(
			<NavDropdown title={capitalise(this.state.theme)}>
				<NavDropdown.Item onClick={() => this.save("dark")}>Dark</NavDropdown.Item>
				<NavDropdown.Item onClick={() => this.save("light")}>Light</NavDropdown.Item>
			</NavDropdown>
		);
	}

	componentDidMount(){
		this.load();
	}

	onUpdate(e){
		console.log(e);
	}

	load(){
		var cookies = new Cookies();
		var theme = cookies.get("theme");
		if(!theme) theme = "dark";
		this.setState({
			theme: theme
		});
	}

	save(theme){
		var cookies = new Cookies();
		cookies.set("theme", theme);
		this.setState({
			theme: theme
		});
	}

	apply(){
		switch(this.state.theme){
			case "dark":
				s("colour-nav", "#313131");
				s("colour-text", "#ffffff");
				s("colour-text-muted", "#d3d3d3");
				s("colour-background", "#1e1e1e");
				s("colour-shadow", "#232323");
				s("colour-dropdown", "#121212");
				s("colour-dropdown-hover", "#252525");
				break;
			case "light":
				s("colour-nav", "#e2e2e2");
				s("colour-text", "#000000");
				s("colour-text-muted", "'#d3d3d3'");
				s("colour-background", "#eeeeee");
				s("colour-shadow", "#d3d3d3");
				s("colour-dropdown", "#ffffff");
				s("colour-dropdown-hover", "#efefef");
				break;
		}
	}
}

function capitalise(s){
	if(typeof(s) !== "string") return "";
	return s.charAt(0).toUpperCase() + s.slice(1);
}

function s(variable, value){
	document.documentElement.style.setProperty(`--${variable}`, value);
}

export default ThemeSelector;
