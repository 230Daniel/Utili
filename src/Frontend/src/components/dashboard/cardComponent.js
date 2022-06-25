import React from "react";
import { Duration } from "luxon";

class CardComponent extends React.Component {
	constructor(props) {
		super(props);
		this.input = React.createRef();

		this.state = {
			value: this.props.value
		};
	}

	render() {
		var visible = this.props.visible;
		if (this.props.visible === undefined) visible = true;
		if (!visible) return null;

		var type = this.props.type;
		if (type == "number") type = "text";

		var pl = this.props.noPaddingLeft ? "0" : null;
		var pr = this.props.noPaddingRight ? "0" : null;

		return (
			<div className="dashboard-card-component" style={{ height: this.props.height, width: this.props.forceWidth, paddingLeft: pl, paddingRight: pr }}>
				{this.props.title &&
					<div className="dashboard-card-component-title" style={{ width: this.props.titleSize - 20 }}>
						{this.props.title}
					</div>
				}
				<div className={`dashboard-card-component-${type}`} style={{ width: this.props.inputSize - 20 }}>
					{this.renderInput()}
				</div>
			</div>
		);
	}

	renderInput() {
		if (this.state.value === null) return null;
		switch (this.props.type) {
			case "text":
				return (
					<input type="text" value={this.state.value} ref={this.input} onChange={() => this.updateValue()} />
				);
			case "number":
				return (
					<input type="number" value={this.state.value} ref={this.input} onChange={() => this.updateValue()} />
				);
			case "checkbox":
				return (
					<div onClick={() => this.input.current.click()}>
						<input type="checkbox" checked={this.state.value} ref={this.input} onClick={(e) => { e.stopPropagation(); }} onChange={() => this.updateValue()} />
					</div>
				);
			case "select":
				return (
					<select ref={this.input} onChange={() => this.updateValue()} value={this.state.value}>
						<option value={-1} hidden>Select...</option>
						{this.props.options.map((option, i) => {
							return (
								<option value={i} key={i}>{option}</option>
							);
						})}
					</select>
				);
			case "select-value":
				return (
					<select ref={this.input} onChange={() => this.updateValue()} value={this.state.value}>
						{!this.props.hideNone &&
							<option value={0}>None</option>}
						{this.props.values.map((value, i) => {
							return (
								<option value={value.id} key={i}>{value.value}</option>
							);
						})}
					</select>
				);
			case "timespan":
				return (
					<div ref={this.input}>
						<input type="number" value={formatNumber(this.state.value.days)} placeholder="dd" onChange={() => this.updateValue()} /><span>:</span>
						<input type="number" value={formatNumber(this.state.value.hours)} placeholder="hh" onChange={() => this.updateValue()} /><span>:</span>
						<input type="number" value={formatNumber(this.state.value.minutes)} placeholder="mm" onChange={() => this.updateValue()} /><span>:</span>
						<input type="number" value={formatNumber(this.state.value.seconds)} placeholder="ss" onChange={() => this.updateValue()} />
					</div>
				);
			case "text-multiline":
				return (
					<textarea value={this.state.value?.replace("\n", "\r\n")} ref={this.input} onChange={() => this.updateValue()} style={{ paddingTop: this.props.padding, paddingBottom: this.props.padding }} />
				);
			case "colour":
				return (
					<input type="color" value={"#" + this.state.value} ref={this.input} onChange={() => this.updateValue()} />
				);
			default:
				return null;
		}
	}

	updateValue() {
		var value = this.getValue();
		this.state.value = value;
		this.setState({});
		this.props.onChanged();
	}

	getValue() {
		var visible = this.props.visible;
		if (this.props.visible === undefined) visible = true;
		if (!visible) return this.state.value;

		switch (this.props.type) {
			case "text":
			case "number":
			case "select":
			case "select-value":
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
			case "text-multiline":
				return this.input.current.value.replace("\r\n", "\n");
			case "colour":
				return this.input.current.value.replace("#", "");
			default:
				return null;
		}
	}
}

export default CardComponent;

function zeroNull(value) {
	if (value) return value;
	return 0;
}

function formatNumber(value) {
	if (value === 0) return "";
	return value.toString().padStart(2, "0");
}
