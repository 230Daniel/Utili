import React from "react";

class CardListComponent extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			values: this.props.values,
			selected: this.props.selected,
			options: this.props.values.filter(x => !this.props.selected.includes(x.id)).map(x => x.id),
			selecting: false,
			query: ""
		};
		this.search = React.createRef();
	}

	render() {
		var disabled = this.props.max && this.state.selected?.length >= this.props.max;
		return (
			<div className="dashboard-card-list-component" onFocus={() => this.searchOpened()} onBlur={() => this.searchClosed()}>
				<div className="dashboard-card-list-component-search">
					<input placeholder={this.props.prompt} value={this.state.selecting ? this.state.query : ""} ref={this.search} disabled={disabled} onInput={() => this.searchUpdated()} />
				</div>
				{this.renderOptions()}
				{this.renderSelected()}
			</div>
		);
	}

	renderOptions() {
		var options = this.sort(this.state.options.filter(x => this.getValue(x).toLowerCase().includes(this.state.query.toLowerCase())));
		return (
			<div className={`dashboard-card-list-component-options${this.state.selecting ? "" : " collapsed"}`}>
				{options.map((item, i) => {
					return (
						<div className="dashboard-card-list-component-option" onClick={() => this.selectValue(item)} tabIndex={-1} key={i}>
							{this.getValue(item)}
						</div>
					);
				})}
			</div>
		);
	}

	renderSelected() {
		var selected = this.sort(this.state.selected);
		return (
			<div className={this.state.selecting ? "collapsed" : ""}>
				{selected.map((item, i) => {
					return (
						<div className="dashboard-card-list-component-selected" key={i}>
							{this.getValue(item)}
							<div className="dashboard-card-list-component-selected-remove" onClick={() => this.unselectValue(item)}>
								<img width={20} src="/bin.svg" />
							</div>
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

	searchClosed() {
		this.setState({ selecting: false });
	}

	getValue(id) {
		return this.state.values.find(x => x.id === id).value.toString();
	}

	sort(ids) {
		if (this.props.noReorder) return ids;
		return ids.map(x => this.state.values.find(y => y.id === x))
			.orderBy(x => x.value)
			.map(x => { if (x) return x.id; else return null; })
			.filter(x => x !== null);
	}

	selectValue(id) {
		this.searchClosed();
		var newSelected = this.state.selected;
		newSelected.push(id);
		var newOptions = this.state.options;
		newOptions.splice(newOptions.indexOf(id), 1);
		this.setState({ selected: newSelected, options: newOptions });
		this.props.onChanged();
	}

	unselectValue(id) {
		var newOptions = this.state.options;
		newOptions.push(id);
		var newSelected = this.state.selected;
		newSelected.splice(newSelected.indexOf(id), 1);
		this.setState({ selected: newSelected, options: newOptions });
		this.props.onChanged();
	}

	getSelected() {
		return this.state.selected;
	}
}

export default CardListComponent;
