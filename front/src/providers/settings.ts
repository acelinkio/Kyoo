import { getServerData } from "one";
import { Platform } from "react-native";
import { MMKV, useMMKVString } from "react-native-mmkv";
import type { ZodTypeAny, z } from "zod";

const storage = new MMKV();

function toBase64(utf8: string) {
	if (typeof window !== "undefined") return window.btoa(utf8);
	return Buffer.from(utf8, "utf8").toString("base64");
}

function fromBase64(b64: string) {
	if (typeof window !== "undefined") return window.atob(b64);
	return Buffer.from(b64, "base64").toString("utf8");
}

export const setCookie = (key: string, val?: unknown) => {
	const value = toBase64(typeof val !== "string" ? JSON.stringify(val) : val);
	const d = new Date();
	// A year
	d.setTime(d.getTime() + 365 * 24 * 60 * 60 * 1000);
	const expires = value ? `expires=${d.toUTCString()}` : "expires=Thu, 01 Jan 1970 00:00:01 GMT";
	document.cookie = `${key}=${value};${expires};path=/;samesite=strict`;
};

export const readCookie = <T extends ZodTypeAny>(key: string, parser: T) => {
	const cookies = getServerData("cookies");
	console.log("cookies", cookies);
	const decodedCookie = decodeURIComponent(cookies);
	const ca = decodedCookie.split(";");

	const name = `${key}=`;
	const ret = ca.find((x) => x.trimStart().startsWith(name));
	if (ret === undefined) return undefined;
	const str = fromBase64(ret.substring(name.length));
	return parser.parse(JSON.parse(str)) as z.infer<T>;
};

export const useStoreValue = <T extends ZodTypeAny>(key: string, parser: T) => {
	if (Platform.OS === "web" && typeof window === "undefined") {
		return readCookie(key, parser);
	}
	const [val] = useMMKVString(key);
	if (val === undefined) return val;
	return parser.parse(JSON.parse(val)) as z.infer<T>;
};

export const storeValue = (key: string, value: unknown) => {
	storage.set(key, JSON.stringify(value));
};

export const readValue = <T extends ZodTypeAny>(key: string, parser: T) => {
	if (Platform.OS === "web" && typeof window === "undefined") {
		return readCookie(key, parser);
	}
	const val = storage.getString(key);
	if (val === undefined) return val;
	return parser.parse(JSON.parse(val)) as z.infer<T>;
};
