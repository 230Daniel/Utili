import React from "react";

class CardAdderComponent extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			values: this.props.values,
			selected: this.props.selected,
			options: this.props.values.filter(x => !this.props.selected.includes(x)),
			selecting: false,
			query: ""
		};
		this.search = React.createRef();
	}

	render() {
		this.state.options = this.props.values.filter(x => !this.props.selected.includes(x));
		return (
			<div className="dashboard-card-list-component">
				<div className="dashboard-card-adder-component-search">
					<input
						placeholder={this.props.prompt}
						value={this.state.query}
						ref={this.search}
						disabled={this.props.disabled}
						onFocus={() => this.searchOpened()}
						onInput={() => this.searchUpdated()}
						onBlur={(e) => this.searchClosed(e)} />
				</div>
				{this.renderOptions()}
			</div>
		);
	}

	renderOptions() {
		var options = this.state.options.filter(x => x.value.toLowerCase().includes(this.state.query.toLowerCase())).orderBy(x => x.value);
		return (
			<div className={`dashboard-card-list-component-options${this.state.selecting ? "" : " collapsed"}`}>
				{options.map((item, i) => {
					return (
						<div className="dashboard-card-list-component-option" onClick={() => this.selectValue(item)} tabIndex={-1} key={i}>
							{item.value}
						</div>
					);
				})}
			</div>
		);
	}

	searchOpened() {
		this.setState({ selecting: true, query: "" });
	}

	searchUpdated() {
		this.setState({ selecting: true, query: this.search.current.value });
	}

	searchClosed(e) {
		if (e.relatedTarget && e.relatedTarget.tabIndex === -1) return;
		this.setState({ selecting: false, query: "" });
	}

	getValue(id) {
		return this.state.values.find(x => x.id === id).value.toString();
	}

	selectValue(id) {
		var newSelected = this.state.selected;
		newSelected.push(id);
		var newOptions = this.state.options;
		newOptions.splice(newOptions.indexOf(id), 1);
		this.setState({ selecting: false, query: "", selected: newSelected, options: newOptions });
		this.props.onChanged();
		this.props.onSelected(id);
	}

	unselectValue(id) {
		var newOptions = this.state.options;
		newOptions.push(id);
		var newSelected = this.state.selected;
		newSelected.splice(newSelected.indexOf(id), 1);
		this.setState({ selected: newSelected, options: newOptions });
		this.props.onChanged();
		this.props.onUnselected(id);
	}
}

export default CardAdderComponent;
