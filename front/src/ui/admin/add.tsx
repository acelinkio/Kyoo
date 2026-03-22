import Add from "@material-symbols/svg-400/rounded/add.svg";
import MovieIcon from "@material-symbols/svg-400/rounded/movie.svg";
import OpenInNew from "@material-symbols/svg-400/rounded/open_in_new.svg";
import SearchIcon from "@material-symbols/svg-400/rounded/search-fill.svg";
import TVIcon from "@material-symbols/svg-400/rounded/tv.svg";
import { useRouter } from "expo-router";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ActivityIndicator, Pressable, View } from "react-native";
import type { Metadata } from "~/models";
import { SearchMovie, SearchSerie } from "~/models";
import {
	HR,
	IconButton,
	Input,
	Link,
	Modal,
	P,
	PosterBackground,
	Skeleton,
	SubP,
	Tabs,
	tooltip,
} from "~/primitives";
import { InfiniteFetch, type QueryIdentifier, useMutation } from "~/query";
import { cn, getDisplayDate, useQueryState } from "~/utils";
import { EmptyView } from "../empty-view";

const ExternalIdLinks = ({ externalId }: { externalId: Metadata }) => {
	const links = Object.entries(externalId).flatMap(([provider, ids]) =>
		ids
			.filter((x) => x.link)
			.map((x) => ({ provider, link: x.link!, label: x.label })),
	);

	if (links.length === 0) return null;

	return (
		<View className="absolute top-1 right-1 flex-row gap-1">
			{links.map(({ provider, link, label }) => (
				<IconButton
					key={`${provider}-${link}`}
					icon={OpenInNew}
					as={Link}
					href={link}
					target="_blank"
					className="bg-gray-800/70 hover:bg-gray-800 focus:bg-gray-800"
					iconClassName="h-5 w-5 fill-slate-200 dark:fill-slate-200"
					{...tooltip(label ?? provider)}
				/>
			))}
		</View>
	);
};

const SearchResultItem = ({
	name,
	subtitle,
	poster,
	externalId,
	onSelect,
	isPending,
}: {
	name: string;
	subtitle: string | null;
	poster: string | null;
	externalId: Metadata;
	onSelect: () => void;
	isPending: boolean;
}) => {
	return (
		<Pressable
			onPress={onSelect}
			disabled={isPending}
			className="group items-center p-1 outline-0"
		>
			<PosterBackground
				src={
					poster
						? {
								id: poster,
								source: poster,
								blurhash: "",
								low: poster,
								medium: poster,
								high: poster,
							}
						: null
				}
				quality="medium"
				className={cn(
					"w-full",
					"ring-accent group-hover:ring-3 group-focus-visible:ring-3",
				)}
			>
				{isPending && (
					<View className="absolute inset-0 items-center justify-center bg-black/50">
						<ActivityIndicator size="large" />
					</View>
				)}
				<ExternalIdLinks externalId={externalId} />
			</PosterBackground>
			<P
				numberOfLines={subtitle ? 1 : 2}
				className="text-center group-focus-within:underline group-hover:underline"
			>
				{name}
			</P>
			{subtitle && <SubP className="text-center">{subtitle}</SubP>}
		</Pressable>
	);
};

SearchResultItem.Loader = () => {
	return (
		<View className="w-full items-center p-1">
			<View className="aspect-2/3 w-full overflow-hidden rounded">
				<Skeleton variant="custom" className="h-full w-full" />
			</View>
			<Skeleton className="mt-1" />
			<Skeleton className="w-1/2" />
		</View>
	);
};

const AddHeader = ({
	query,
	setQuery,
	kind,
	setKind,
}: {
	query: string;
	setQuery: (q: string) => void;
	kind: "movie" | "serie";
	setKind: (k: "movie" | "serie") => void;
}) => {
	const { t } = useTranslation();

	return (
		<View className="gap-3 p-4">
			<View className="flex-1 flex-wrap content-center items-center gap-2 sm:flex-row">
				<Input
					value={query}
					onChangeText={setQuery}
					placeholder={t("admin.add.searchPlaceholder")}
					left={
						<IconButton icon={SearchIcon} {...tooltip(t("navbar.search"))} />
					}
					containerClassName="flex-1"
				/>
				<Tabs
					value={kind}
					setValue={setKind}
					tabs={[
						{
							icon: MovieIcon,
							label: t("admin.add.movies"),
							value: "movie",
						},
						{
							icon: TVIcon,
							label: t("admin.add.series"),
							value: "serie",
						},
					]}
				/>
			</View>
			<HR />
		</View>
	);
};

export const AddPage = () => {
	const { t } = useTranslation();
	const router = useRouter();
	const [query, setQuery] = useQueryState("q", "");
	const [kind, setKind] = useQueryState<"movie" | "serie">("kind", "movie");

	const { mutateAsync, isPending } = useMutation<SearchMovie | SearchSerie>({
		method: "POST",
		path: ["scanner", kind === "movie" ? "movies" : "series"],
		compute: (item) => ({
			body: {
				title: item.name,
				year:
					"airDate" in item
						? item.airDate?.getFullYear()
						: item.startAir?.getFullYear(),
				externalId: Object.fromEntries(
					Object.entries(item.externalId).map(([k, v]) => [k, v[0].dataId]),
				),
				videos: [],
			},
		}),
		invalidate: null,
	});

	if (query.length === 0) {
		return (
			<Modal icon={Add} title={t("admin.add.title")}>
				<AddHeader
					query={query}
					setQuery={setQuery}
					kind={kind}
					setKind={setKind}
				/>
				<P className="self-center py-8 text-center">
					{t("admin.add.typeToSearch")}
				</P>
			</Modal>
		);
	}

	return (
		<Modal icon={Add} title={t("admin.add.title")} scroll={false}>
			<InfiniteFetch
				layout={{
					layout: "grid",
					gap: 8,
					numColumns: { xs: 2, sm: 3, md: 4 },
					size: 200,
				}}
				query={AddPage.query(kind, query)}
				Header={
					<AddHeader
						query={query}
						setQuery={setQuery}
						kind={kind}
						setKind={setKind}
					/>
				}
				Empty={<EmptyView message={t("admin.add.noResults")} />}
				Render={({ item }) => (
					<SearchResultItem
						name={item.name}
						subtitle={getDisplayDate(item)}
						poster={item.poster}
						externalId={item.externalId}
						onSelect={async () => {
							await mutateAsync(item);
							if (router.canGoBack()) router.back();
						}}
						isPending={isPending}
					/>
				)}
				Loader={SearchResultItem.Loader}
			/>
		</Modal>
	);
};

AddPage.query = (
	kind: "movie" | "serie",
	query: string,
): QueryIdentifier<SearchMovie | SearchSerie> => ({
	parser: kind === "movie" ? SearchMovie : SearchSerie,
	path: ["scanner", kind === "movie" ? "movies" : "series"],
	params: {
		query: query,
	},
	infinite: true,
	enabled: query.length > 0,
});
