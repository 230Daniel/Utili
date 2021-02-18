import backend from "../config/backend.json";

export async function ping(){
	try{
		var response = await fetch(`${backend.host}/status`, { method: "GET" });
		return response.ok;
	}
	catch{
		return false;
	}
}
