import { useRouter } from "expo-router";
import { useEffect, useRef } from "react";
import { P } from "~/primitives";
import { useQueryState } from "~/utils";

export const OidcCallbackPage = () => {
	const [apiUrl] = useQueryState("apiUrl", undefined!);
	const [provider] = useQueryState("provider", undefined!);
	const [code] = useQueryState("token", undefined!);
	const [error] = useQueryState("error", undefined!);

	const hasRun = useRef(false);
	const router = useRouter();

	useEffect(() => {
		if (hasRun.current) return;
		hasRun.current = true;

		function onError(error: string) {
			router.replace({ pathname: "/login", params: { error, apiUrl } });
		}
		async function run() {
			const { error: loginError } = await oidcLogin(provider, code, apiUrl);
			if (loginError) onError(loginError);
			else router.replace("/");
		}

		if (error) onError(error);
		else run();
	}, [provider, code, apiUrl, router, error]);
	return <P>{"Loading"}</P>;
};
