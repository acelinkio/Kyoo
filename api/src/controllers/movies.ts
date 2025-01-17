import { and, eq, sql } from "drizzle-orm";
import { Elysia, t } from "elysia";
import { KError } from "~/models/error";
import { comment } from "~/utils";
import { db } from "../db";
import { shows, showTranslations } from "~/db/schema";
import { getColumns, sqlarr } from "~/db/schema/utils";
import { bubble } from "~/models/examples";
import { Movie, MovieStatus, MovieTranslation } from "~/models/movie";
import {
	Filter,
	type Image,
	Sort,
	type FilterDef,
	Genre,
	isUuid,
	keysetPaginate,
	Page,
	processLanguages,
	createPage,
} from "~/models/utils";

const movieFilters: FilterDef = {
	genres: {
		column: shows.genres,
		type: "enum",
		values: Genre.enum,
		isArray: true,
	},
	rating: { column: shows.rating, type: "int" },
	status: { column: shows.status, type: "enum", values: MovieStatus.enum },
	runtime: { column: shows.runtime, type: "float" },
	airDate: { column: shows.startAir, type: "date" },
	originalLanguage: { column: shows.originalLanguage, type: "string" },
};

export const movies = new Elysia({ prefix: "/movies", tags: ["movies"] })
	.model({
		movie: Movie,
		"movie-translation": MovieTranslation,
	})
	.get(
		"/:id",
		async ({
			params: { id },
			headers: { "accept-language": languages },
			query: { preferOriginal },
			error,
			set,
		}) => {
			const langs = processLanguages(languages);

			const ret = await db.query.shows.findFirst({
				columns: {
					kind: false,
					startAir: false,
					endAir: false,
				},
				extras: {
					airDate: sql<string>`${shows.startAir}`.as("airDate"),
					status: sql<MovieStatus>`${shows.status}`.as("status"),
				},
				where: and(
					eq(shows.kind, "movie"),
					isUuid(id) ? eq(shows.id, id) : eq(shows.slug, id),
				),
				with: {
					translations: {
						columns: {
							pk: false,
						},
						where: !langs.includes("*")
							? eq(showTranslations.language, sql`any(${sqlarr(langs)})`)
							: undefined,
						orderBy: [
							sql`array_position(${sqlarr(langs)}, ${showTranslations.language})`,
						],
						limit: 1,
					},
					originalTranslation: {
						columns: {
							poster: true,
							thumbnail: true,
							banner: true,
							logo: true,
						},
						extras: {
							// TODO: also fallback on user settings (that's why i made a select here)
							preferOriginal:
								sql<boolean>`(select coalesce(${preferOriginal ?? null}::boolean, false))`.as(
									"preferOriginal",
								),
						},
					},
				},
			});

			if (!ret) {
				return error(404, {
					status: 404,
					message: "Movie not found",
				});
			}
			const translation = ret.translations[0];
			if (!translation) {
				return error(422, {
					status: 422,
					message: "Accept-Language header could not be satisfied.",
				});
			}
			set.headers["content-language"] = translation.language;
			const ot = ret.originalTranslation;
			return {
				...ret,
				...translation,
				...(ot?.preferOriginal && {
					...(ot.poster && { poster: ot.poster }),
					...(ot.thumbnail && { thumbnail: ot.thumbnail }),
					...(ot.banner && { banner: ot.banner }),
					...(ot.logo && { logo: ot.logo }),
				}),
			};
		},
		{
			detail: {
				description: "Get a movie by id or slug",
			},
			params: t.Object({
				id: t.String({
					description: "The id or slug of the movie to retrieve.",
					example: bubble.slug,
				}),
			}),
			query: t.Object({
				preferOriginal: t.Optional(
					t.Boolean({
						description: comment`
							Prefer images in the original's language. If true, will return untranslated images instead of the translated ones.

							If unspecified, kyoo will look at the current user's settings to decide what to do.
						`,
					}),
				),
			}),
			headers: t.Object({
				"accept-language": t.String({
					default: "*",
					example: "en-us, ja;q=0.5",
					description: comment`
						List of languages you want the data in.
						This follows the [Accept-Language offical specification](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language).
					`,
				}),
			}),
			response: {
				200: { ...Movie, description: "Found" },
				404: {
					...KError,
					description: "No movie found with the given id or slug.",
					examples: [
						{ status: 404, message: "Movie not found", details: undefined },
					],
				},
				422: {
					...KError,
					description: comment`
						The Accept-Language header can't be satisfied (all languages listed are
						unavailable.) Try with another languages or add * to the list of languages
						to fallback to any language.
					`,
					examples: [
						{
							status: 422,
							message: "Accept-Language header could not be satisfied.",
						},
					],
				},
			},
		},
	)
	.get(
		"random",
		async ({ error, redirect }) => {
			const [movie] = await db
				.select({ id: shows.id })
				.from(shows)
				.where(eq(shows.kind, "movie"))
				.orderBy(sql`random()`)
				.limit(1);
			if (!movie)
				return error(404, {
					status: 404,
					message: "No movies in the database",
				});
			return redirect(`/movies/${movie.id}`);
		},
		{
			detail: {
				description: "Get a random movie",
			},
			response: {
				302: t.Void({
					description:
						"Redirected to the [/movies/{id}](#tag/movies/GET/movies/{id}) route.",
				}),
				404: {
					...KError,
					description: "No movie found with the given id or slug.",
					examples: [
						{ status: 404, message: "Movie not found", details: undefined },
					],
				},
			},
		},
	)
	.get(
		"",
		async ({
			query: { limit, after, sort, filter, preferOriginal },
			headers: { "accept-language": languages },
			request: { url },
		}) => {
			const langs = processLanguages(languages);

			// we keep the pk for after handling. it will be removed by elysia's validators after.
			const { kind, startAir, endAir, ...moviesCol } = getColumns(shows);

			const transQ = db
				.selectDistinctOn([showTranslations.pk])
				.from(showTranslations)
				.orderBy(
					showTranslations.pk,
					sql`array_position(${sqlarr(langs)}, ${showTranslations.language})`,
				)
				.as("t");
			const { pk, poster, thumbnail, banner, logo, ...transCol } =
				getColumns(transQ);

			const items = await db
				.select({
					...moviesCol,
					...transCol,
					status: sql<MovieStatus>`${moviesCol.status}`,
					airDate: startAir,
					poster: sql<Image>`coalesce(${showTranslations.poster}, ${poster})`,
					thumbnail: sql<Image>`coalesce(${showTranslations.thumbnail}, ${thumbnail})`,
					banner: sql<Image>`coalesce(${showTranslations.banner}, ${banner})`,
					logo: sql<Image>`coalesce(${showTranslations.logo}, ${logo})`,
				})
				.from(shows)
				.innerJoin(transQ, eq(shows.pk, transQ.pk))
				.leftJoin(
					showTranslations,
					and(
						eq(shows.pk, showTranslations.pk),
						eq(showTranslations.language, shows.originalLanguage),
						// TODO: check user's settings before fallbacking to false.
						sql`coalesce(${preferOriginal ?? null}::boolean, false)`,
					),
				)
				.where(and(filter, keysetPaginate({ table: shows, after, sort })))
				.orderBy(
					...(sort.random
						? [sql`md5(${sort.random.seed} || ${shows.pk})`]
						: []),
					...sort.sort.map((x) =>
						x.desc ? sql`${shows[x.key]} desc nulls last` : shows[x.key],
					),
					shows.pk,
				)
				.limit(limit);

			return createPage(items, { url, sort, limit });
		},
		{
			detail: { description: "Get all movies" },
			query: t.Object({
				sort: Sort(["slug", "rating", "airDate", "createdAt", "nextRefresh"], {
					remap: { airDate: "startAir" },
					default: ["slug"],
					description: "How to sort the query",
				}),
				filter: t.Optional(Filter({ def: movieFilters })),
				limit: t.Integer({
					minimum: 1,
					maximum: 250,
					default: 50,
					description: "Max page size.",
				}),
				after: t.Optional(
					t.String({
						description: comment`
							Id of the cursor in the pagination.
							You can ignore this and only use the prev/next field in the response.
						`,
					}),
				),
				preferOriginal: t.Optional(
					t.Boolean({
						description: comment`
							Prefer images in the original's language. If true, will return untranslated images instead of the translated ones.

							If unspecified, kyoo will look at the current user's settings to decide what to do.
						`,
					}),
				),
			}),
			headers: t.Object({
				"accept-language": t.String({
					default: "*",
					example: "en-us, ja;q=0.5",
					description: comment`
						List of languages you want the data in.
						This follows the [Accept-Language offical specification](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language).

						In this request, * is always implied (if no language could satisfy the request, kyoo will use any language available.)
					`,
				}),
			}),
			response: {
				200: Page(Movie, {
					description: "Paginated list of movies that match filters.",
				}),
				422: {
					...KError,
					description: "Invalid query parameters.",
					examples: [
						{
							status: 422,
							message:
								"Invalid property: slug. Expected one of genres, rating, status, runtime, airDate, originalLanguage.",
							details: {
								in: "slug eq bubble",
							},
						},
					],
				},
			},
		},
	);
