export async function ping() {
	try {
		var response = await fetch(`${window.__config.backend}/status`, { method: "GET" });
		return response.ok;
	}
	catch {
		return false;
	}
}
