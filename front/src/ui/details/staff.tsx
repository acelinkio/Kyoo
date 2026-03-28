import { useTranslation } from "react-i18next";
import { View } from "react-native";
import { Role } from "~/models";
import { Container, H2, P, Poster, Skeleton, SubP } from "~/primitives";
import { InfiniteGrid, type QueryIdentifier } from "~/query";
import { EmptyView } from "../empty-view";

const CharacterCard = ({ item }: { item: Role }) => {
	const { t } = useTranslation();
	return (
		<View className="flex-row items-center overflow-hidden rounded-xl bg-card">
			<Poster src={item.staff.image} quality="low" className="w-28" />
			<View className="flex-1 items-center justify-center py-5">
				<P className="text-center font-semibold" numberOfLines={2}>
					{item.staff.name}
				</P>
				<SubP className="mt-1 text-center" numberOfLines={2}>
					{item.character
						? t("show.staff-as", {
								character: item.character.name,
							})
						: item.kind}
				</SubP>
			</View>
			{item.character && (
				<Poster src={item.character.image} quality="low" className="w-28" />
			)}
		</View>
	);
};

CharacterCard.Loader = () => (
	<View className="h-32 flex-row overflow-hidden rounded-xl bg-card">
		<Skeleton variant="custom" className="h-full w-1/3 rounded-none" />
		<View className="flex-1 items-center justify-center px-3">
			<Skeleton className="h-5 w-4/5" />
			<Skeleton className="mt-2 h-4 w-3/5" />
			<Skeleton className="mt-2 h-3 w-2/5" />
		</View>
		<Skeleton variant="custom" className="h-full w-1/3 rounded-none" />
	</View>
);

export const Staff = ({
	kind,
	slug,
}: {
	kind: "serie" | "movie";
	slug: string;
}) => {
	const { t } = useTranslation();

	return (
		<Container className="mb-4">
			<InfiniteGrid
				query={Staff.query(kind, slug)}
				layout={{
					numColumns: { xs: 1, md: 2, xl: 3 },
					numLines: 3,
					gap: { xs: 8, lg: 12 },
				}}
				Header={({ controls }) => (
					<View className="mb-3 flex-row items-center justify-between">
						<H2>{t("show.staff")}</H2>
						{controls}
					</View>
				)}
				Empty={<EmptyView message={t("show.staff-none")} />}
				Render={({ item }) => <CharacterCard item={item} />}
				Loader={() => <CharacterCard.Loader />}
				getItemKey={(item) =>
					`${item.staff.id}-${item.kind}-${item.character?.name ?? "none"}`
				}
			/>
		</Container>
	);
};

Staff.query = (
	kind: "serie" | "movie",
	slug: string,
): QueryIdentifier<Role> => ({
	path: ["api", `${kind}s`, slug, "staff"],
	parser: Role,
	infinite: true,
});
