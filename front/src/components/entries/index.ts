import type { Entry } from "~/models";

export * from "./entry-box";
export * from "./entry-line";

export const entryDisplayNumber = (entry: Partial<Entry>) => {
	switch (entry.kind) {
		case "episode":
			if (!entry.seasonNumber) return `SP${entry.episodeNumber}`;
			return `S${entry.seasonNumber}:E${entry.episodeNumber}`;
		case "special":
			return `SP${entry.number}`;
		case "movie":
			return "";
		default:
			return "??";
	}
};
