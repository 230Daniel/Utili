import { useLocation } from "react-router-dom";

function ResetPage(props) {
	// useLocation hook means this component always re-renders
	useLocation();
	window.scrollTo(0, 0);
	var elems = document.querySelectorAll(".navbar-toggler");
	[].forEach.call(elems, function (el) {
		if (el.className.indexOf("collapsed") === -1) {
			setTimeout(() => {
				el.click();
			}, 0);
		}
	});
	return null;
}

export default ResetPage;
