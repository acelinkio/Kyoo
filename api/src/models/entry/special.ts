import { t } from "elysia";
import { type Prettify, comment } from "~/utils";
import { bubbleImages, madeInAbyss, registerExamples } from "../examples";
import { Progress } from "../history";
import {
	DbMetadata,
	EpisodeId,
	Resource,
	SeedImage,
	TranslationRecord,
} from "../utils";
import { EmbeddedVideo } from "../video";
import { BaseEntry, EntryTranslation } from "./base-entry";

export const BaseSpecial = t.Intersect(
	[
		t.Object({
			kind: t.Literal("special"),
			order: t.Number({
				minimum: 1,
				description: "Absolute playback order. Can be mixed with episodes.",
			}),
			number: t.Integer({ minimum: 1 }),
			externalId: EpisodeId,
		}),
		BaseEntry(),
	],
	{
		description: comment`
			A special is either an OAV episode (side story & co) or an important episode that was released standalone
			(outside of a season.)
		`,
	},
);

export const Special = t.Intersect([
	Resource(),
	EntryTranslation(),
	BaseSpecial,
	t.Object({
		videos: t.Optional(t.Array(EmbeddedVideo)),
		progress: Progress,
	}),
	DbMetadata,
]);
export type Special = Prettify<typeof Special.static>;

export const SeedSpecial = t.Intersect([
	t.Omit(BaseSpecial, ["thumbnail", "nextRefresh"]),
	t.Object({
		thumbnail: t.Nullable(SeedImage),
		translations: TranslationRecord(EntryTranslation()),
		videos: t.Optional(t.Array(t.String({ format: "uuid" }), { default: [] })),
	}),
]);
export type SeedSpecial = Prettify<typeof SeedSpecial.static>;

const ep = madeInAbyss.entries.find((x) => x.kind === "special")!;
registerExamples(Special, {
	...ep,
	...ep.translations.en,
	...bubbleImages,
	slug: `${madeInAbyss.slug}-sp3`,
});
