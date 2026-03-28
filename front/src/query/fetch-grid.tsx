import ArrowBack from "@material-symbols/svg-400/rounded/arrow_back-fill.svg";
import ArrowForward from "@material-symbols/svg-400/rounded/arrow_forward-fill.svg";
import {
	type ComponentType,
	type ReactElement,
	useEffect,
	useState,
} from "react";
import { useTranslation } from "react-i18next";
import { View } from "react-native";
import {
	type Breakpoint,
	IconButton,
	tooltip,
	useBreakpointMap,
} from "~/primitives";
import { type QueryIdentifier, useInfiniteFetch } from "./query";

export type GridLayout = {
	numColumns: Breakpoint<number>;
	numLines: Breakpoint<number>;
	gap: Breakpoint<number>;
};

export const InfiniteGrid = <Data,>({
	query,
	layout,
	Render,
	Loader,
	Empty,
	Header,
	Footer,
	getItemKey,
}: {
	query: QueryIdentifier<Data>;
	layout: GridLayout;
	Render: (props: { item: Data; index: number }) => ReactElement | null;
	Loader: (props: { index: number }) => ReactElement | null;
	Empty?: ReactElement;
	Header?: ComponentType<{ controls: ReactElement }> | ReactElement;
	Footer?: ComponentType | ReactElement;
	getItemKey?: (item: Data, index: number) => string | number;
}): ReactElement | null => {
	const { t } = useTranslation();
	const [pageIndex, setPageIndex] = useState(0);
	const { numColumns, numLines, gap } = useBreakpointMap(layout);

	query = {
		...query,
		params: { ...query.params, limit: numColumns * numLines },
	};
	const { data, fetchNextPage, hasNextPage, isFetching, isFetchingNextPage } =
		useInfiniteFetch(query);

	if (!query.infinite)
		console.warn("A non infinite query was passed to an InfiniteGrid.");

	const queryIdentity = JSON.stringify(query);
	useEffect(() => {
		queryIdentity;
		setPageIndex(0);
	}, [queryIdentity]);

	const pages = data?.pages ?? [];
	const items = pages[pageIndex]?.items ?? [];

	const controls = (
		<View className="flex-row items-center">
			<IconButton
				icon={ArrowBack}
				onPress={() => setPageIndex((x) => x - 1)}
				disabled={pageIndex <= 0}
				{...tooltip(t("misc.prev-page"))}
			/>
			<IconButton
				icon={ArrowForward}
				onPress={async () => {
					if (pageIndex < pages.length - 1) {
						setPageIndex((x) => x + 1);
						return;
					}
					if (!hasNextPage || isFetchingNextPage) return;
					const res = await fetchNextPage();
					if (!res.isError) setPageIndex((x) => x + 1);
				}}
				disabled={
					pageIndex === pages.length - 1 && (isFetchingNextPage || !hasNextPage)
				}
				{...tooltip(t("misc.next-page"))}
			/>
		</View>
	);

	const header =
		typeof Header === "function" ? (
			<Header controls={controls} />
		) : (
			(Header ?? controls)
		);

	const footer = typeof Footer === "function" ? <Footer /> : (Footer ?? null);

	if (isFetching && pages.length === 0) {
		return (
			<>
				{header}
				<View className="flex-row" style={{ gap }}>
					{[...Array(numColumns)].map((_, columnIndex) => (
						<View key={columnIndex} className="flex-1" style={{ gap }}>
							{[...Array(numColumns)].map((__, idx) => (
								<Loader key={idx} index={idx} />
							))}
						</View>
					))}
				</View>
				{footer}
			</>
		);
	}

	if (items.length === 0) {
		return (
			<>
				{header}
				{Empty}
				{footer}
			</>
		);
	}

	const columns = items.reduce(
		(acc, item, index) => {
			acc[index % numColumns].push(item);
			return acc;
		},
		[...Array(numColumns)].map(() => [] as Data[]),
	);

	return (
		<>
			{header}
			{
				<View className="flex-row" style={{ gap }}>
					{columns.map((column, columnIndex) => (
						<View key={columnIndex} className="flex-1" style={{ gap }}>
							{column.map((item, index) => {
								const itemIndex = index * numColumns + columnIndex;
								return (
									<Render
										key={getItemKey?.(item, itemIndex) ?? itemIndex}
										item={item}
										index={itemIndex}
									/>
								);
							})}
						</View>
					))}
				</View>
			}
			{footer}
		</>
	);
};
