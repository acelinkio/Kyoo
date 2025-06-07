// Typed as UserConfig for handy autocomplete
import type { UserConfig } from "vite";
import { one } from "one/vite";

export default {
	plugins: [one()],
} satisfies UserConfig;
