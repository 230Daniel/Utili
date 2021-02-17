import Cookies from "universal-cookie";

export async function getDetails(){
	var response = await fetch(`https://localhost:5001/auth`, { mode: "cors", credentials: "include" });
	if(response.ok) return await response.json();
	else return null;
}

export async function signOut(){
	await fetch(`https://localhost:5001/auth/signout`, { method: "POST", credentials: "include" });
}

export async function signIn(){
	const cookies = new Cookies();
	cookies.set("return_path", window.location.pathname, { path: "/return", maxAge: 60, sameSite: "strict" });
	window.location.href = "https://localhost:5001/auth/signin";
}
