import React from "react";
import "../styles/price-selector.css";

class PriceSelector extends React.Component {
	render() {
		return (
			<div className="price-selector">
				<div className={`price left ${this.props.currency === "GBP" ? "selected" : ""}`} onClick={() => this.props.onChanged("GBP")}>GBP</div>
				<div className={`price ${this.props.currency === "EUR" ? "selected" : ""}`} onClick={() => this.props.onChanged("EUR")}>EUR</div>
				<div className={`price right ${this.props.currency === "USD" ? "selected" : ""}`} onClick={() => this.props.onChanged("USD")}>USD</div>
			</div>
		);
	}
}

export default PriceSelector;
