import Search from "@material-symbols/svg-400/rounded/search-fill.svg";
import { useTranslation } from "react-i18next";
import { Video } from "~/models";
import { type QueryIdentifier, useFetch } from "~/query";
import { useQueryState } from "~/utils";
import { AddPage } from "./add";

export const MatchPage = () => {
	const [id] = useQueryState("id", undefined!);
	const { data } = useFetch(MatchPage.query(id));
	const { t } = useTranslation();

	return (
		<AddPage
			icon={Search}
			title={t("admin.unmatched.match-file", { file: data?.path ?? "" })}
			videos={[{ id: id, episodes: data?.guess.episodes ?? [] }]}
			allowLibrary
		/>
	);
};

MatchPage.query = (id: string): QueryIdentifier<Video> => ({
	parser: Video,
	path: ["api", "videos", id],
});
