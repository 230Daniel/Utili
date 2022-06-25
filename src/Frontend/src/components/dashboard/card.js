import React from "react";

import "../../styles/card.css";

class Card extends React.Component {
	render() {
		return (
			<div className={`dashboard-card`} style={{ minWidth: this.props.size }}>
				{this.props.title &&
					<div className="dashboard-card-title">{this.renderTitle(this.props.title)}
						{this.props.onRemoved &&
							<div className="dashboard-card-list-component-selected-remove" onClick={this.props.onRemoved}>
								<img width={20} src="/bin.svg" />
							</div>}
					</div>}
				{this.renderChildren()}
			</div>
		);
	}

	renderTitle(title) {
		let regex = new RegExp("^<:.+:([0-9]+)>$");
		if (regex.test(title)) {
			var id = regex.exec(title)[1];
			return (
				<img src={`https://cdn.discordapp.com/emojis/${id}`} width="24px" />
			);
		} else {
			return title;
		}
	}

	renderChildren() {
		return React.Children.map(this.props.children, child => {
			if (React.isValidElement(child)) {
				return React.cloneElement(child, { titleSize: this.props.titleSize, inputSize: this.props.inputSize, onChanged: this.props.onChanged });
			}
			return child;
		});
	}
}


export default Card;
