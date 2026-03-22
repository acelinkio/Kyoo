import { z } from "zod/v4";
import { Metadata } from "./utils/metadata";
import { zdate } from "./utils/utils";

export const SearchMovie = z
	.object({
		id: z.string(),
		slug: z.string(),
		name: z.string(),
		description: z.string().nullable(),
		airDate: zdate().nullable(),
		poster: z.string().nullable(),
		originalLanguage: z.string().nullable(),
		externalId: Metadata,
	})
	.transform((x) => ({ kind: "search-movie", ...x }));
export type SearchMovie = z.infer<typeof SearchMovie>;

export const SearchSerie = z
	.object({
		id: z.string(),
		slug: z.string(),
		name: z.string(),
		description: z.string().nullable(),
		startAir: zdate().nullable(),
		endAir: zdate().nullable(),
		poster: z.string().nullable(),
		originalLanguage: z.string().nullable(),
		externalId: Metadata,
	})
	.transform((x) => ({ kind: "search-serie", ...x }));
export type SearchSerie = z.infer<typeof SearchSerie>;
