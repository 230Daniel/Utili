import React from "react";

class CardRemoverComponent extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			values: this.props.values
		};
	}

	render() {
		return (
			<div className="dashboard-card-list-component">
				{this.renderSelected()}
			</div>
		);
	}

	renderSelected() {
		return (
			<div className={this.state.selecting ? "collapsed" : ""}>
				{this.state.values.map((item, i) => {
					return (
						<div className="dashboard-card-list-component-selected" key={i}>
							{this.props.render ? this.props.render(item) : item}
							<div className="dashboard-card-list-component-selected-remove" onClick={() => this.unselectValue(item)}>
								<img width={20} src="/bin.svg" />
							</div>
						</div>
					);
				})}
			</div>
		);
	}

	unselectValue(value) {
		var newValues = this.state.values;
		newValues.splice(newValues.indexOf(value), 1);
		this.setState({ values: newValues });
		this.props.onChanged();
	}

	getValues() {
		return this.state.values;
	}
}

export default CardRemoverComponent;
