import Cookies from "universal-cookie";

export async function setAntiForgeryToken() {
	if (window.__antiForgeryToken) return;
	var response = await fetch(`${window.__config.backend}/authentication/antiforgery`, { mode: "cors", credentials: "include" });
	var token = await response.json();
	window.__antiForgeryToken = token;
}

export async function getDetails() {
	try {
		var response = await fetch(`${window.__config.backend}/authentication/me`, { mode: "cors", credentials: "include" });
		var details = await response.json();
		setAntiForgeryToken();
		return details;
	} catch {
		setAntiForgeryToken();
		return null;
	}
}

export async function signOut() {
	await fetch(`${window.__config.backend}/authentication/signout`, { method: "POST", credentials: "include", headers: { "X-XSRF-TOKEN": window.__antiForgeryToken } });
}

export async function signIn() {
	const cookies = new Cookies();
	cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" });
	window.location.href = `${window.__config.backend}/authentication/signin`;
}

export async function get(endpoint) {
	var result = await fetch(`${window.__config.backend}/${endpoint}`, { method: "GET", credentials: "include", headers: { "X-XSRF-TOKEN": window.__antiForgeryToken } });
	switch (result.status) {
		case 401:
			signIn();
			break;
		case 403:
			window.location.pathname = "dashboard";
			break;
		case 404:
			if (endpoint.includes("dashboard")) {
				window.location.pathname = `/invite/${endpoint.split("/")[1]}`;
			}
			break;
		default:
			break;
	}
	return result;
}

export async function post(endpoint, body) {
	try {
		var result = await fetch(`${window.__config.backend}/${endpoint}`, {
			method: "POST",
			headers: {
				"Content-Type": "application/json",
				"X-XSRF-TOKEN": window.__antiForgeryToken
			},
			credentials: "include",
			body: JSON.stringify(body)
		});
		switch (result.status) {
			case 401:
				signIn();
				break;
			case 403:
				window.location.pathname = "dashboard";
				break;
			case 404:
				if (endpoint.includes("dashboard")) {
					window.location.pathname = `/invite/${endpoint.split("/")[1]}`;
				}
				break;
			case 200:
				break;
			default:
				throw new Error(`Server returned unexpected response code ${result.status}`);
		}
	} catch (e) {
		console.log(e);
		return { ok: false };
	}
	return result;
}
