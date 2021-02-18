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
			<NavDropdown id="theme" title={capitalise(this.state.theme)}>
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
		cookies.set("theme", theme, { path: "/" });
		this.setState({
			theme: theme
		});
	}

	apply(){
		switch(this.state.theme){
			case "dark":
				c("nav", "#313131");
				c("text", "#ffffff");
				c("text-muted", "#d3d3d3");
				c("background", "#1e1e1e");
				c("shadow", "#131313");
				c("dropdown", "#121212");
				c("dropdown-hover", "#252525");
				c("scrollbar", "#1e1e1e");
				c("scrollbar-thumb", "#272727");
				c("card", "#272727");
				c("card-title", "#353535");
				break;
			case "light":
				c("nav", "#e2e2e2");
				c("text", "#000000");
				c("text-muted", "'#d3d3d3'");
				c("background", "#eeeeee");
				c("shadow", "#a3a3a3");
				c("dropdown", "#ffffff");
				c("dropdown-hover", "#efefef");
				c("scrollbar", "#eeeeee");
				c("scrollbar-thumb", "#ababab");
				c("card", "#e2e2e2");
				c("card-title", "#d3d3d3");
				break;
		}
	}
}

function capitalise(s){
	if(typeof(s) !== "string") return "";
	return s.charAt(0).toUpperCase() + s.slice(1);
}

function c(variable, value){
	document.documentElement.style.setProperty(`--colour-${variable}`, value);
}

export default ThemeSelector;
