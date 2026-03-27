CREATE INDEX "evj_video_pk" ON "kyoo"."entry_video_join" USING btree ("video_pk");--> statement-breakpoint
ALTER TABLE "kyoo"."entries" DROP COLUMN "next_refresh";--> statement-breakpoint
ALTER TABLE "kyoo"."seasons" DROP COLUMN "next_refresh";
ALTER TABLE "kyoo"."shows" ALTER COLUMN "next_refresh" SET DATA TYPE date;
