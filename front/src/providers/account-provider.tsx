import { useQueryClient } from "@tanstack/react-query";
import { type ReactNode, useContext, useEffect, useMemo, useRef } from "react";
import { Platform } from "react-native";
import { z } from "zod";
import { AccountP, UserP } from "~/models";
import { useFetch } from "~/query";
import { AccountContext } from "./account-context";
import { removeAccounts, updateAccount } from "./account-store";
import { useSetError } from "./error-provider";
import { useStoreValue } from "./settings";

export const AccountProvider = ({ children }: { children: ReactNode }) => {
	const [setError, clearError] = useSetError("account");
	const accounts = useStoreValue("accounts", z.array(AccountP)) ?? [];

	const ret = useMemo(() => {
		const acc = accounts.find((x) => x.selected);
		return {
			apiUrl: Platform.OS === "web" ? "/api" : acc?.apiUrl!,
			authToken: acc?.token ?? null,
			selectedAccount: acc ?? null,
			accounts: accounts.map((account) => ({
				...account,
				select: () => updateAccount(account.id, { ...account, selected: true }),
				remove: () => removeAccounts((x) => x.id === account.id),
			})),
		};
	}, [accounts]);

	// update user's data from kyoo on startup, it could have changed.
	const {
		isSuccess: userIsSuccess,
		isPlaceholderData: userIsPlaceholder,
		data: user,
		error: userError,
	} = useFetch({
		path: ["auth", "me"],
		parser: UserP,
		placeholderData: ret.selectedAccount,
		enabled: !!ret.selectedAccount,
		options: {
			apiUrl: ret.apiUrl,
			authToken: ret.authToken?.access_token,
		},
	});
	console.log(user);
	// Use a ref here because we don't want the effect to trigger when the selected
	// value has changed, only when the fetch result changed
	// If we trigger the effect when the selected value change, we enter an infinite render loop
	const selectedRef = useRef(ret.selectedAccount);
	selectedRef.current = ret.selectedAccount;
	useEffect(() => {
		if (!selectedRef.current || !userIsSuccess || userIsPlaceholder) return;
		// The id is different when user is stale data, we need to wait for the use effect to invalidate the query.
		if (user?.id !== selectedRef.current.id) return;
		const nUser = { ...selectedRef.current, ...user };
		updateAccount(nUser.id, nUser);
	}, [user, userIsSuccess, userIsPlaceholder]);

	const queryClient = useQueryClient();
	useEffect(() => {
		if (!userError) return clearError();
		setError({
			error: userError,
			retry: () => {
				queryClient.resetQueries();
			},
		});
	}, [userError, queryClient, setError, clearError]);

	const selectedId = ret.selectedAccount?.id;
	useEffect(() => {
		selectedId;
		// if the user change account (or connect/disconnect), reset query cache.
		queryClient.resetQueries();
	}, [selectedId, queryClient]);

	return <AccountContext.Provider value={ret}>{children}</AccountContext.Provider>;
};

export const useAccount = () => {
	const { selectedAccount } = useContext(AccountContext);
	return selectedAccount;
};
