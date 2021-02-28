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
				<NavDropdown.Item onClick={() => this.save("classic")}>Classic</NavDropdown.Item>
				<NavDropdown.Item onClick={() => this.save("light")}>Light</NavDropdown.Item>
			</NavDropdown>
		);
	}

	componentDidMount(){
		this.load();
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
		cookies.set("theme", theme, { path: "/", expires: new Date("9999-01-01 00:00:00") });
		this.setState({
			theme: theme
		});
		if(this.props.onChange) this.props.onChange();
	}

	apply(){
		switch(this.state.theme){
			case "dark":
				c("nav", "#313131");
				c("text", "#ffffff");
				c("text-muted", "#d3d3d3");
				c("text-invert", "#ffffff");
				c("text-invert-muted", "#d3d3d3");
				c("background", "#1e1e1e");
				c("sidebar-category", "#1e1e1e");
				c("sidebar-item", "#272727")
				c("shadow", "#131313");
				c("shadow-invert", "#131313");
				c("dropdown", "#121212");
				c("dropdown-hover", "#252525");
				c("scrollbar", "#1e1e1e");
				c("scrollbar-thumb", "#272727");
				c("card", "#272727");
				c("card-hover", "#232323");
				c("card-title", "#353535");
				c("card-input-important", "#121212")
				break;
			case "classic":
				c("nav", "#e9ecef");
				c("text", "#495057");
				c("text-muted", "#666666");
				c("text-invert", "#ffffff");
				c("text-invert-muted", "#d3d3d3");
				c("background", "#2d2d2d");
				c("sidebar-category", "#d9d9d9");
				c("sidebar-item", "#efefef")
				c("shadow", "#2d2d2d");
				c("shadow-invert", "#1b1b1b");
				c("dropdown", "#ffffff");
				c("dropdown-hover", "#efefef");
				c("scrollbar", "#2d2d2d");
				c("scrollbar-thumb", "#484848");
				c("card", "#ffffff");
				c("card-hover", "#fafafa");
				c("card-title", "#e9ecef");
				c("card-input-important", "#bbbbbb")
				break;
			case "light":
				c("nav", "#e2e2e2");
				c("text", "#495057");
				c("text-muted", "'#d3d3d3'");
				c("text-invert", "#000000");
				c("text-invert-muted", "'#d3d3d3'");
				c("background", "#eeeeee");
				c("sidebar-category", "#d9d9d9");
				c("sidebar-item", "#efefef")
				c("shadow", "#a3a3a3");
				c("shadow-invert", "#a3a3a3");
				c("dropdown", "#ffffff");
				c("dropdown-hover", "#efefef");
				c("scrollbar", "#eeeeee");
				c("scrollbar-thumb", "#ababab");
				c("card", "#f9f9f9");
				c("card-hover", "#f3f3f3");
				c("card-title", "#d9d9d9");
				c("card-input-important", "#d2d2d2")
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

function s(variable, value){
	document.documentElement.style.setProperty(`--${variable}`, value);
}

export default ThemeSelector;
