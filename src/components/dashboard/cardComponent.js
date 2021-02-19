import React from "react";

class CardComponent extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			value: this.props.value
		}
		this.input = React.createRef();
		this.value = this.props.value;
	}

	render(){
		return(
			<div className="dashboard-card-component">
				<div className="dashboard-card-component-title" style={{width: this.props.titleSize - 20}}>
					{this.props.title}
				</div>
				<div className={`dashboard-card-component-${this.props.type}`} style={{width: this.props.titleSize - 20}}>
					{this.renderInput()}
				</div>
			</div>
		);
	}

	renderInput(){
		switch(this.props.type){
			case "text":
				return(
					<input type="text" value={this.state.value} ref={this.input} onChange={() => this.updateValue()}/>
				);
			case "checkbox":
				return(
					<div onClick={() => this.input.current.click()}>
						<input type="checkbox" checked={this.state.value} ref={this.input} onClick={(e) => { e.stopPropagation(); }} onChange={() => this.updateValue()}/>
					</div>
				);
			default:
				return null;
		}
	}

	updateValue(){
		this.setState({value: this.getValue()});
		this.value = this.state.value;
	}
	
	getValue(){
		switch(this.props.type){
			case "text":
				return this.input.current.value;
			case "checkbox":
				return this.input.current.checked;
			default:
				return null;
		}
	}
}

export default CardComponent;
