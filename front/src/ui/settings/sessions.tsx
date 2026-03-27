import Devices from "@material-symbols/svg-400/outlined/devices.svg";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { z } from "zod/v4";
import type { KyooError } from "~/models";
import { Button, P } from "~/primitives";
import { type QueryIdentifier, useFetch, useMutation } from "~/query";
import { Preference, SettingsContainer } from "./base";

export const SessionsSettings = () => {
	const { t } = useTranslation();
	const [error, setError] = useState<string | null>(null);
	const { data: sessions } = useFetch(SessionsSettings.query());
	const items = sessions ?? [];
	const { mutateAsync: revokeSession, isPending } = useMutation({
		method: "DELETE",
		compute: (id: string) => ({
			path: ["auth", "sessions", id],
		}),
		invalidate: ["auth", "users", "me", "sessions"],
	});

	return (
		<SettingsContainer title={t("settings.sessions.label")}>
			{error && <P className="mx-6 text-red-500">{error}</P>}
			{items.map((session) => (
				<Preference
					key={session.id}
					icon={Devices}
					label={session.device}
					description={
						session.current
							? t("settings.sessions.current")
							: t("settings.sessions.description", {
									createdDate: session.createdDate.toLocaleString(),
									lastUsed: session.lastUsed.toLocaleString(),
								})
					}
				>
					<Button
						text={t("settings.sessions.revoke")}
						disabled={isPending}
						onPress={async () => {
							setError(null);
							try {
								await revokeSession(session.id);
							} catch (e) {
								setError((e as KyooError).message);
							}
						}}
					/>
				</Preference>
			))}
		</SettingsContainer>
	);
};

SessionsSettings.query = (): QueryIdentifier<Session[]> => ({
	path: ["auth", "users", "me", "sessions"],
	parser: z.array(Session),
});

const Session = z.object({
	id: z.string(),
	createdDate: z.coerce.date(),
	lastUsed: z.coerce.date(),
	device: z.string(),
	current: z.boolean(),
});
type Session = z.infer<typeof Session>;
