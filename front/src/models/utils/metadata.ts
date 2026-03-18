import { z } from "zod/v4";

export const Metadata = z.record(
	z.string(),
	z.array(
		z.object({
			dataId: z.string(),
			link: z.string().nullable(),
			label: z.string().optional().nullable(),
		}),
	),
);
export type Metadata = z.infer<typeof Metadata>;
