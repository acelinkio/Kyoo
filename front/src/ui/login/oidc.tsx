import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Image, Platform, View } from "react-native";
import { z } from "zod/v4";
import { AuthInfo } from "~/models/auth-info";
import { Button, HR, Link, P, Skeleton } from "~/primitives";
import { Fetch, type QueryIdentifier } from "~/query";

export const OidcLogin = ({
	apiUrl,
	children,
	error,
}: {
	apiUrl: string;
	children: ReactNode;
	error?: string;
}) => {
	const { t } = useTranslation();

	const or = (
		<>
			<View className="my-2 w-full flex-row items-center">
				<HR className="grow" />
				<P>{t("misc.or")}</P>
				<HR className="grow" />
			</View>
			{children}
		</>
	);

	return (
		<Fetch
			query={OidcLogin.query(apiUrl)}
			Render={(info) => (
				<>
					<View className="my-2 items-center">
						{Object.entries(info.oidc).map(([id, provider]) => (
							<Button
								as={Link}
								key={id}
								href={provider.connect}
								replace
								className="w-full sm:w-3/4"
								left={
									provider.logo ? (
										<Image
											source={{ uri: provider.logo }}
											className="mx-2 h-6 w-6"
											resizeMode="contain"
										/>
									) : null
								}
								text={t("login.via", { provider: provider.name })}
							/>
						))}
					</View>
					{info.allowRegister
						? or
						: error && (
								<P className="text-red-500 dark:text-red-500">{error}</P>
							)}
				</>
			)}
			Loader={() => (
				<>
					<View className="my-2 items-center">
						{[...Array(3)].map((_, i) => (
							<Button key={i} className="w-full sm:w-3/4">
								<Skeleton />
							</Button>
						))}
					</View>
					{or}
				</>
			)}
		/>
	);
};

OidcLogin.query = (apiUrl?: string): QueryIdentifier<AuthInfo> => ({
	path: ["auth", "info"],
	parser: AuthInfo,
	options: { apiUrl },
});
