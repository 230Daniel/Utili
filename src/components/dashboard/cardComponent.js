import React from "react";
import { Duration } from "luxon";

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
				<div className={`dashboard-card-component-${this.props.type}`} style={{width: this.props.inputSize - 20}}>
					{this.renderInput()}
				</div>
			</div>
		);
	}

	renderInput(){
		if(this.state.value === null) return null;
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
			case "select":
				return(
					<select ref={this.input} onChange={() => this.updateValue()} value={this.state.value}>
						<option value={-1} hidden>Select...</option>
						{this.props.options.map((option, i) => {
							return(
								<option value={i} key={i}>{option}</option>
							);
						})}
					</select>
				);
			case "timespan":
				return(
					<div ref={this.input}>
						<input type="number" value={formatNumber(this.state.value.days)} placeholder="dd" onChange={() => this.updateValue()}/><span>:</span>
						<input type="number" value={formatNumber(this.state.value.hours)} placeholder="hh" onChange={() => this.updateValue()}/><span>:</span>
						<input type="number" value={formatNumber(this.state.value.minutes)} placeholder="mm" onChange={() => this.updateValue()}/><span>:</span>
						<input type="number" value={formatNumber(this.state.value.seconds)} placeholder="ss" onChange={() => this.updateValue()}/>
					</div>
				);
			default:
				return null;
		}
	}

	updateValue(){
		var value = this.getValue();
		this.setState({value: value});
		this.value = value;
		this.props.onChanged();
	}
	
	getValue(){
		switch(this.props.type){
			case "text":
			case "select":
				return this.input.current.value;
			case "checkbox":
				return this.input.current.checked;
			case "timespan":
				var milliseconds = 
					parseInt(zeroNull(this.input.current.children[0].value)) * 24 * 60 * 60 * 1000 +
					parseInt(zeroNull(this.input.current.children[2].value)) * 60 * 60 * 1000 +
					parseInt(zeroNull(this.input.current.children[4].value)) * 60 * 1000 +
					parseInt(zeroNull(this.input.current.children[6].value)) * 1000;
				return Duration.fromMillis(milliseconds).shiftTo("days", "hours", "minutes", "seconds");
			default:
				return null;
		}
	}
}

export default CardComponent;

function zeroNull(value){
	if(value) return value;
	return 0;
}

function formatNumber(value, length){
	if(value === 0) return "";
	return value.toString().padStart(2, "0");
}
