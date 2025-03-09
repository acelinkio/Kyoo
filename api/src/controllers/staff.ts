import { and, eq, sql } from "drizzle-orm";
import Elysia, { t } from "elysia";
import { db } from "~/db";
import { staff } from "~/db/schema/staff";
import { KError } from "~/models/error";
import { Role, Staff } from "~/models/staff";
import {
	Page,
	Sort,
	createPage,
	isUuid,
	keysetPaginate,
	sortToSql,
} from "~/models/utils";
import { desc } from "~/models/utils/descriptions";

const staffSort = Sort(["slug", "name", "latinName"], { default: ["slug"] });

export const staffH = new Elysia({ tags: ["staff"] })
	.model({
		staff: Staff,
		role: Role,
	})
	.get(
		"/staff/:id",
		async ({ params: { id }, error }) => {
			const [ret] = await db
				.select()
				.from(staff)
				.where(isUuid(id) ? eq(staff.id, id) : eq(staff.slug, id))
				.limit(1);
			if (!ret) {
				return error(404, {
					status: 404,
					message: `No staff found with the id or slug: '${id}'`,
				});
			}
			return ret;
		},
		{
			detail: {
				description: "Get a staff member by id or slug.",
			},
			params: t.Object({
				id: t.String({
					description: "The id or slug of the staff to retrieve.",
					example: "hiroyuki-sawano",
				}),
			}),
			response: {
				200: "staff",
				404: {
					...KError,
					description: "No staff found with the given id or slug.",
				},
			},
		},
	)
	.get(
		"/staff/random",
		async ({ error, redirect }) => {
			const [member] = await db
				.select({ slug: staff.slug })
				.from(staff)
				.orderBy(sql`random()`)
				.limit(1);
			if (!member)
				return error(404, {
					status: 404,
					message: "No staff in the database.",
				});
			return redirect(`/staff/${member.slug}`);
		},
		{
			detail: {
				description: "Get a random staff member.",
			},
			response: {
				302: t.Void({
					description:
						"Redirected to the [/staff/{id}](#tag/staff/GET/staff/{id}) route.",
				}),
				404: {
					...KError,
					description: "No staff in the database.",
				},
			},
		},
	)
	.get(
		"/staff",
		async ({ query: { limit, after, sort, query }, request: { url } }) => {
			const items = await db
				.select()
				.from(staff)
				.where(
					and(
						query ? sql`${staff.name} %> ${query}::text` : undefined,
						keysetPaginate({ table: staff, after, sort }),
					),
				)
				.orderBy(
					...(query
						? [sql`word_similarity(${query}::text, ${staff.name})`]
						: sortToSql(sort, staff)),
					staff.pk,
				)
				.limit(limit);
			return createPage(items, { url, sort, limit });
		},
		{
			detail: {
				description: "Get all staff members known by kyoo.",
			},
			query: t.Object({
				sort: staffSort,
				query: t.Optional(t.String({ description: desc.query })),
				limit: t.Integer({
					minimum: 1,
					maximum: 250,
					default: 50,
					description: "Max page size.",
				}),
				after: t.Optional(t.String({ description: desc.after })),
			}),
			response: {
				200: Page(Staff),
				422: KError,
			},
		},
	);
