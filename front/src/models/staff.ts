import { z } from "zod/v4";
import { Show } from "./show";
import { KImage } from "./utils/images";
import { Metadata } from "./utils/metadata";
import { zdate } from "./utils/utils";

export const Character = z.object({
	name: z.string(),
	latinName: z.string().nullable(),
	image: KImage.nullable(),
});
export type Character = z.infer<typeof Character>;

export const Staff = z.object({
	id: z.string(),
	slug: z.string(),
	name: z.string(),
	latinName: z.string().nullable(),
	image: KImage.nullable(),
	externalId: Metadata,
	createdAt: zdate(),
	updatedAt: zdate(),
});
export type Staff = z.infer<typeof Staff>;

const BaseRole = z.object({
	kind: z.enum([
		"actor",
		"director",
		"writter",
		"producer",
		"music",
		"crew",
		"other",
	]),
	character: Character.nullable(),
});

export const Role = BaseRole.extend({
	staff: Staff,
});
export type Role = z.infer<typeof Role>;

export const RoleWithShow = BaseRole.extend({
	show: Show,
}).transform((x) => ({
	...x,
	id: `${x.show.id}-${x.kind}-${x.character?.name ?? "none"}`,
}));
export type RoleWithShow = z.infer<typeof RoleWithShow>;
