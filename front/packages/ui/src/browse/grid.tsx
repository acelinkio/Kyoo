/*
 * Kyoo - A portable and vast media library solution.
 * Copyright (c) Kyoo.
 *
 * See AUTHORS.md and LICENSE file in the project root for full license information.
 *
 * Kyoo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * Kyoo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Kyoo. If not, see <https://www.gnu.org/licenses/>.
 */

import { type KyooImage, WatchStatusV } from "@kyoo/models";
import {
	Icon,
	Link,
	P,
	Poster,
	PosterBackground,
	Skeleton,
	SubP,
	focusReset,
	important,
	ts,
} from "@kyoo/primitives";
import Done from "@material-symbols/svg-400/rounded/check-fill.svg";
import { useState } from "react";
import { type ImageStyle, Platform, View } from "react-native";
import { type Stylable, type Theme, max, percent, px, rem, useYoshiki } from "yoshiki/native";
import { ItemContext } from "../components/context-menus";
import type { Layout } from "../fetch";

export const ItemWatchStatus = ({
	watchStatus,
	unseenEpisodesCount,
	...props
}: {
	watchStatus?: WatchStatusV | null;
	unseenEpisodesCount?: number | null;
}) => {
	const { css } = useYoshiki();

	if (watchStatus !== WatchStatusV.Completed && !unseenEpisodesCount) return null;

	return (
		<View
			{...css(
				{
					position: "absolute",
					top: 0,
					right: 0,
					minWidth: max(rem(1), ts(3.5)),
					aspectRatio: 1,
					justifyContent: "center",
					alignItems: "center",
					m: ts(0.5),
					pX: ts(0.5),
					bg: (theme) => theme.darkOverlay,
					borderRadius: 999999,
				},
				props,
			)}
		>
			{watchStatus === WatchStatusV.Completed ? (
				<Icon icon={Done} size={16} />
			) : (
				<P {...css({ marginVertical: 0, verticalAlign: "middle", textAlign: "center" })}>
					{unseenEpisodesCount}
				</P>
			)}
		</View>
	);
};

export const ItemProgress = ({ watchPercent }: { watchPercent: number }) => {
	const { css } = useYoshiki("episodebox");

	return (
		<>
			<View
				{...css({
					backgroundColor: (theme) => theme.user.overlay0,
					width: percent(100),
					height: ts(0.5),
					position: "absolute",
					bottom: 0,
				})}
			/>
			<View
				{...css({
					backgroundColor: (theme) => theme.user.accent,
					width: percent(watchPercent),
					height: ts(0.5),
					position: "absolute",
					bottom: 0,
				})}
			/>
		</>
	);
};

export const ItemGrid = ({
	href,
	slug,
	name,
	type,
	subtitle,
	poster,
	watchStatus,
	watchPercent,
	unseenEpisodesCount,
	...props
}: {
	href: string;
	slug: string;
	name: string;
	subtitle: string | null;
	poster: KyooImage | null;
	watchStatus: WatchStatusV | null;
	watchPercent: number | null;
	type: "movie" | "show" | "collection";
	unseenEpisodesCount: number | null;
} & Stylable<"text">) => {
	const [moreOpened, setMoreOpened] = useState(false);
	const { css } = useYoshiki("grid");

	return (
		<Link
			href={moreOpened ? undefined : href}
			onLongPress={() => setMoreOpened(true)}
			{...css(
				{
					flexDirection: "column",
					alignItems: "center",
					width: percent(100),
					child: {
						poster: {
							borderColor: (theme) => theme.background,
							borderWidth: ts(0.5),
							borderStyle: "solid",
						},
						more: {
							display: "none",
						},
					},
					fover: {
						self: focusReset,
						poster: {
							borderColor: (theme: Theme) => theme.accent,
						},
						title: {
							textDecorationLine: "underline",
						},
						more: {
							display: "flex",
						},
					},
				},
				props,
			)}
		>
			<PosterBackground
				src={poster}
				alt={name}
				quality="low"
				layout={{ width: percent(100) }}
				{...(css("poster") as { style: ImageStyle })}
			>
				<ItemWatchStatus watchStatus={watchStatus} unseenEpisodesCount={unseenEpisodesCount} />
				{type === "movie" && watchPercent && <ItemProgress watchPercent={watchPercent} />}
				{type !== "collection" && (
					<ItemContext
						type={type}
						slug={slug}
						status={watchStatus}
						isOpen={moreOpened}
						setOpen={(v) => setMoreOpened(v)}
						{...css([
							{
								position: "absolute",
								top: 0,
								right: 0,
								bg: (theme) => theme.dark.background,
							},
							"more",
							Platform.OS === "web" && moreOpened && { display: important("flex") },
						])}
					/>
				)}
			</PosterBackground>
			<P numberOfLines={subtitle ? 1 : 2} {...css([{ marginY: 0, textAlign: "center" }, "title"])}>
				{name}
			</P>
			{subtitle && (
				<SubP
					{...css({
						marginTop: 0,
						textAlign: "center",
					})}
				>
					{subtitle}
				</SubP>
			)}
		</Link>
	);
};

ItemGrid.Loader = (props: object) => {
	const { css } = useYoshiki();

	return (
		<View
			{...css(
				{
					flexDirection: "column",
					alignItems: "center",
					width: percent(100),
				},
				props,
			)}
		>
			<Poster.Loader layout={{ width: percent(100) }} />
			<Skeleton />
			<Skeleton {...css({ width: percent(50) })} />
		</View>
	);
};

ItemGrid.layout = {
	size: px(150),
	numColumns: { xs: 3, sm: 4, md: 5, lg: 6, xl: 8 },
	gap: { xs: ts(1), sm: ts(2), md: ts(4) },
	layout: "grid",
} satisfies Layout;
