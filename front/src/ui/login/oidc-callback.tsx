import { useRouter } from "expo-router";
import { useEffect } from "react";
import { P } from "~/primitives";
import { useToken } from "~/providers/account-context";
import { useQueryState } from "~/utils";
import { oidcLogin } from "./logic";

export const OidcCallbackPage = () => {
	const { authToken } = useToken();
	const [apiUrl] = useQueryState("apiUrl", undefined!);
	const [provider] = useQueryState("provider", undefined!);
	const [code] = useQueryState("token", undefined!);
	const [error] = useQueryState("error", undefined!);
	const [link] = useQueryState("link", undefined!);

	const router = useRouter();

	// biome-ignore lint/correctness/useExhaustiveDependencies: useMountEffect
	useEffect(() => {
		function onError(error: string) {
			router.replace({ pathname: "/login", params: { error, apiUrl } });
		}
		async function run() {
			const { error: loginError } = await oidcLogin(
				provider,
				code,
				link ? authToken : null,
				apiUrl,
			);
			if (loginError) onError(loginError);
			else router.replace(link ? "/settings" : "/");
		}

		if (error) onError(error);
		else run();
	}, []);
	return <P>{"Loading"}</P>;
};
