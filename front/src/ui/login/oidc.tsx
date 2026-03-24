import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Image, Platform, View } from "react-native";
import { z } from "zod/v4";
import { Button, HR, Link, P, Skeleton } from "~/primitives";
import { Fetch, type QueryIdentifier } from "~/query";

export const OidcLogin = ({
	apiUrl,
	children,
}: {
	apiUrl: string;
	children: ReactNode;
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
								href={provider.link}
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
					{info.allowRegister && or}
				</>
			)}
			Loader={() => (
				<>
					<View className="my-2 items-center">
						{[...Array(3)].map((_, i) => (
							<View
								key={i}
								className="my-1.5 w-full rounded-4xl border-3 border-accent px-4 py-3 sm:w-3/4"
							>
								<Skeleton className="mx-auto my-1 h-5 w-2/3" />
							</View>
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

const AuthInfo = z
	.object({
		publicUrl: z.string(),
		allowRegister: z.boolean().optional().default(true),
		oidc: z.record(
			z.string(),
			z.object({
				name: z.string(),
				logo: z.string().nullable().optional(),
			}),
		),
	})
	.transform((x) => {
		const baseUrl = Platform.OS === "web" ? x.publicUrl : "kyoo://";
		return {
			...x,
			oidc: Object.fromEntries(
				Object.entries(x.oidc).map(([provider, info]) => [
					provider,
					{
						...info,
						link: `${x.publicUrl}/auth/oidc/login/${provider}?redirectUrl=${baseUrl}/login/callback`,
					},
				]),
			),
		};
	});
type AuthInfo = z.infer<typeof AuthInfo>;
