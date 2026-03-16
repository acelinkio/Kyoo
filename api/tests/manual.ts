import { migrate } from "~/db";
import { setupLogging } from "../src/logtape";
import { createVideo } from "./helpers";

// test file used to run manually using
// `JWT_SECRET="this is a secret" JWT_ISSUER="https://kyoo.zoriya.dev" bun tests/manual.ts`

await setupLogging();
await migrate();

const [resp, body] = await createVideo({
	guess: {
		title: "mia",
		episodes: [{ season: 1, episode: 13 }],
		from: "test",
		history: [
			{
				title: "toto",
				from: "tata",
			},
		],
	},
	part: null,
	path: "/video/mia s1e13.mkv",
	rendering: "sha2",
	version: 1,
});
console.log(body);

process.exit(0);
