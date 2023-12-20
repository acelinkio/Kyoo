// <auto-generated />
using System;
using System.Collections.Generic;
using Kyoo.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Kyoo.Postgresql.Migrations
{
	[DbContext(typeof(PostgresContext))]
	partial class PostgresContextModelSnapshot : ModelSnapshot
	{
		protected override void BuildModel(ModelBuilder modelBuilder)
		{
#pragma warning disable 612, 618
			modelBuilder
				.HasAnnotation("ProductVersion", "7.0.12")
				.HasAnnotation("Relational:MaxIdentifierLength", 63);

			NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "genre", new[] { "action", "adventure", "animation", "comedy", "crime", "documentary", "drama", "family", "fantasy", "history", "horror", "music", "mystery", "romance", "science_fiction", "thriller", "war", "western" });
			NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "status", new[] { "unknown", "finished", "airing", "planned" });
			NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "watch_status", new[] { "completed", "watching", "droped", "planned" });
			NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

			modelBuilder.Entity("Kyoo.Abstractions.Models.Collection", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<string>("Name")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Overview")
						.HasColumnType("text")
						.HasColumnName("overview");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.HasKey("Id")
						.HasName("pk_collections");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_collections_slug");

					b.ToTable("collections", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Episode", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<int?>("AbsoluteNumber")
						.HasColumnType("integer")
						.HasColumnName("absolute_number");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<int?>("EpisodeNumber")
						.HasColumnType("integer")
						.HasColumnName("episode_number");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<string>("Name")
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Overview")
						.HasColumnType("text")
						.HasColumnName("overview");

					b.Property<string>("Path")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("path");

					b.Property<DateTime?>("ReleaseDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("release_date");

					b.Property<int>("Runtime")
						.HasColumnType("integer")
						.HasColumnName("runtime");

					b.Property<Guid?>("SeasonId")
						.HasColumnType("uuid")
						.HasColumnName("season_id");

					b.Property<int?>("SeasonNumber")
						.HasColumnType("integer")
						.HasColumnName("season_number");

					b.Property<Guid>("ShowId")
						.HasColumnType("uuid")
						.HasColumnName("show_id");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.HasKey("Id")
						.HasName("pk_episodes");

					b.HasIndex("SeasonId")
						.HasDatabaseName("ix_episodes_season_id");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_episodes_slug");

					b.HasIndex("ShowId", "SeasonNumber", "EpisodeNumber", "AbsoluteNumber")
						.IsUnique()
						.HasDatabaseName("ix_episodes_show_id_season_number_episode_number_absolute_numb");

					b.ToTable("episodes", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.EpisodeWatchStatus", b =>
				{
					b.Property<Guid>("UserId")
						.HasColumnType("uuid")
						.HasColumnName("user_id");

					b.Property<Guid?>("EpisodeId")
						.HasColumnType("uuid")
						.HasColumnName("episode_id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<DateTime?>("PlayedDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("played_date");

					b.Property<WatchStatus>("Status")
						.HasColumnType("watch_status")
						.HasColumnName("status");

					b.Property<int?>("WatchedPercent")
						.HasColumnType("integer")
						.HasColumnName("watched_percent");

					b.Property<int?>("WatchedTime")
						.HasColumnType("integer")
						.HasColumnName("watched_time");

					b.HasKey("UserId", "EpisodeId")
						.HasName("pk_episode_watch_status");

					b.HasIndex("EpisodeId")
						.HasDatabaseName("ix_episode_watch_status_episode_id");

					b.ToTable("episode_watch_status", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Movie", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<DateTime?>("AirDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("air_date");

					b.Property<string[]>("Aliases")
						.IsRequired()
						.HasColumnType("text[]")
						.HasColumnName("aliases");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<Genre[]>("Genres")
						.IsRequired()
						.HasColumnType("genre[]")
						.HasColumnName("genres");

					b.Property<string>("Name")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Overview")
						.HasColumnType("text")
						.HasColumnName("overview");

					b.Property<string>("Path")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("path");

					b.Property<int>("Rating")
						.HasColumnType("integer")
						.HasColumnName("rating");

					b.Property<int>("Runtime")
						.HasColumnType("integer")
						.HasColumnName("runtime");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.Property<Status>("Status")
						.HasColumnType("status")
						.HasColumnName("status");

					b.Property<Guid?>("StudioId")
						.HasColumnType("uuid")
						.HasColumnName("studio_id");

					b.Property<string>("Tagline")
						.HasColumnType("text")
						.HasColumnName("tagline");

					b.Property<string[]>("Tags")
						.IsRequired()
						.HasColumnType("text[]")
						.HasColumnName("tags");

					b.Property<string>("Trailer")
						.HasColumnType("text")
						.HasColumnName("trailer");

					b.HasKey("Id")
						.HasName("pk_movies");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_movies_slug");

					b.HasIndex("StudioId")
						.HasDatabaseName("ix_movies_studio_id");

					b.ToTable("movies", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.MovieWatchStatus", b =>
				{
					b.Property<Guid>("UserId")
						.HasColumnType("uuid")
						.HasColumnName("user_id");

					b.Property<Guid>("MovieId")
						.HasColumnType("uuid")
						.HasColumnName("movie_id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<DateTime?>("PlayedDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("played_date");

					b.Property<WatchStatus>("Status")
						.HasColumnType("watch_status")
						.HasColumnName("status");

					b.Property<int?>("WatchedPercent")
						.HasColumnType("integer")
						.HasColumnName("watched_percent");

					b.Property<int?>("WatchedTime")
						.HasColumnType("integer")
						.HasColumnName("watched_time");

					b.HasKey("UserId", "MovieId")
						.HasName("pk_movie_watch_status");

					b.HasIndex("MovieId")
						.HasDatabaseName("ix_movie_watch_status_movie_id");

					b.ToTable("movie_watch_status", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Season", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<DateTime?>("EndDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("end_date");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<string>("Name")
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Overview")
						.HasColumnType("text")
						.HasColumnName("overview");

					b.Property<int>("SeasonNumber")
						.HasColumnType("integer")
						.HasColumnName("season_number");

					b.Property<Guid>("ShowId")
						.HasColumnType("uuid")
						.HasColumnName("show_id");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.Property<DateTime?>("StartDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("start_date");

					b.HasKey("Id")
						.HasName("pk_seasons");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_seasons_slug");

					b.HasIndex("ShowId", "SeasonNumber")
						.IsUnique()
						.HasDatabaseName("ix_seasons_show_id_season_number");

					b.ToTable("seasons", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Show", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<List<string>>("Aliases")
						.IsRequired()
						.HasColumnType("text[]")
						.HasColumnName("aliases");

					b.Property<DateTime?>("EndAir")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("end_air");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<List<Genre>>("Genres")
						.IsRequired()
						.HasColumnType("genre[]")
						.HasColumnName("genres");

					b.Property<string>("Name")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Overview")
						.HasColumnType("text")
						.HasColumnName("overview");

					b.Property<int>("Rating")
						.HasColumnType("integer")
						.HasColumnName("rating");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.Property<DateTime?>("StartAir")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("start_air");

					b.Property<Status>("Status")
						.HasColumnType("status")
						.HasColumnName("status");

					b.Property<Guid?>("StudioId")
						.HasColumnType("uuid")
						.HasColumnName("studio_id");

					b.Property<string>("Tagline")
						.HasColumnType("text")
						.HasColumnName("tagline");

					b.Property<List<string>>("Tags")
						.IsRequired()
						.HasColumnType("text[]")
						.HasColumnName("tags");

					b.Property<string>("Trailer")
						.HasColumnType("text")
						.HasColumnName("trailer");

					b.HasKey("Id")
						.HasName("pk_shows");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_shows_slug");

					b.HasIndex("StudioId")
						.HasDatabaseName("ix_shows_studio_id");

					b.ToTable("shows", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.ShowWatchStatus", b =>
				{
					b.Property<Guid>("UserId")
						.HasColumnType("uuid")
						.HasColumnName("user_id");

					b.Property<Guid>("ShowId")
						.HasColumnType("uuid")
						.HasColumnName("show_id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<Guid?>("NextEpisodeId")
						.HasColumnType("uuid")
						.HasColumnName("next_episode_id");

					b.Property<DateTime?>("PlayedDate")
						.HasColumnType("timestamp with time zone")
						.HasColumnName("played_date");

					b.Property<WatchStatus>("Status")
						.HasColumnType("watch_status")
						.HasColumnName("status");

					b.Property<int>("UnseenEpisodesCount")
						.HasColumnType("integer")
						.HasColumnName("unseen_episodes_count");

					b.Property<int?>("WatchedPercent")
						.HasColumnType("integer")
						.HasColumnName("watched_percent");

					b.Property<int?>("WatchedTime")
						.HasColumnType("integer")
						.HasColumnName("watched_time");

					b.HasKey("UserId", "ShowId")
						.HasName("pk_show_watch_status");

					b.HasIndex("NextEpisodeId")
						.HasDatabaseName("ix_show_watch_status_next_episode_id");

					b.HasIndex("ShowId")
						.HasDatabaseName("ix_show_watch_status_show_id");

					b.ToTable("show_watch_status", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Studio", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<string>("ExternalId")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("external_id");

					b.Property<string>("Name")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("name");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.HasKey("Id")
						.HasName("pk_studios");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_studios_slug");

					b.ToTable("studios", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.User", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uuid")
						.HasColumnName("id");

					b.Property<DateTime>("AddedDate")
						.ValueGeneratedOnAdd()
						.HasColumnType("timestamp with time zone")
						.HasColumnName("added_date")
						.HasDefaultValueSql("now() at time zone 'utc'");

					b.Property<string>("Email")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("email");

					b.Property<string>("Password")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("password");

					b.Property<string[]>("Permissions")
						.IsRequired()
						.HasColumnType("text[]")
						.HasColumnName("permissions");

					b.Property<string>("Settings")
						.IsRequired()
						.HasColumnType("json")
						.HasColumnName("settings");

					b.Property<string>("Slug")
						.IsRequired()
						.HasMaxLength(256)
						.HasColumnType("character varying(256)")
						.HasColumnName("slug");

					b.Property<string>("Username")
						.IsRequired()
						.HasColumnType("text")
						.HasColumnName("username");

					b.HasKey("Id")
						.HasName("pk_users");

					b.HasIndex("Slug")
						.IsUnique()
						.HasDatabaseName("ix_users_slug");

					b.ToTable("users", (string)null);
				});

			modelBuilder.Entity("link_collection_movie", b =>
				{
					b.Property<Guid>("collection_id")
						.HasColumnType("uuid")
						.HasColumnName("collection_id");

					b.Property<Guid>("movie_id")
						.HasColumnType("uuid")
						.HasColumnName("movie_id");

					b.HasKey("collection_id", "movie_id")
						.HasName("pk_link_collection_movie");

					b.HasIndex("movie_id")
						.HasDatabaseName("ix_link_collection_movie_movie_id");

					b.ToTable("link_collection_movie", (string)null);
				});

			modelBuilder.Entity("link_collection_show", b =>
				{
					b.Property<Guid>("collection_id")
						.HasColumnType("uuid")
						.HasColumnName("collection_id");

					b.Property<Guid>("show_id")
						.HasColumnType("uuid")
						.HasColumnName("show_id");

					b.HasKey("collection_id", "show_id")
						.HasName("pk_link_collection_show");

					b.HasIndex("show_id")
						.HasDatabaseName("ix_link_collection_show_show_id");

					b.ToTable("link_collection_show", (string)null);
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Collection", b =>
				{
					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("CollectionId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("CollectionId");

							b1.ToTable("collections");

							b1.WithOwner()
								.HasForeignKey("CollectionId")
								.HasConstraintName("fk_collections_collections_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Poster", b1 =>
						{
							b1.Property<Guid>("CollectionId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("poster_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("poster_source");

							b1.HasKey("CollectionId");

							b1.ToTable("collections");

							b1.WithOwner()
								.HasForeignKey("CollectionId")
								.HasConstraintName("fk_collections_collections_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Thumbnail", b1 =>
						{
							b1.Property<Guid>("CollectionId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("thumbnail_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("thumbnail_source");

							b1.HasKey("CollectionId");

							b1.ToTable("collections");

							b1.WithOwner()
								.HasForeignKey("CollectionId")
								.HasConstraintName("fk_collections_collections_id");
						});

					b.Navigation("Logo");

					b.Navigation("Poster");

					b.Navigation("Thumbnail");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Episode", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Season", "Season")
						.WithMany("Episodes")
						.HasForeignKey("SeasonId")
						.OnDelete(DeleteBehavior.Cascade)
						.HasConstraintName("fk_episodes_seasons_season_id");

					b.HasOne("Kyoo.Abstractions.Models.Show", "Show")
						.WithMany("Episodes")
						.HasForeignKey("ShowId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_episodes_shows_show_id");

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("EpisodeId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("EpisodeId");

							b1.ToTable("episodes");

							b1.WithOwner()
								.HasForeignKey("EpisodeId")
								.HasConstraintName("fk_episodes_episodes_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Poster", b1 =>
						{
							b1.Property<Guid>("EpisodeId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("poster_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("poster_source");

							b1.HasKey("EpisodeId");

							b1.ToTable("episodes");

							b1.WithOwner()
								.HasForeignKey("EpisodeId")
								.HasConstraintName("fk_episodes_episodes_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Thumbnail", b1 =>
						{
							b1.Property<Guid>("EpisodeId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("thumbnail_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("thumbnail_source");

							b1.HasKey("EpisodeId");

							b1.ToTable("episodes");

							b1.WithOwner()
								.HasForeignKey("EpisodeId")
								.HasConstraintName("fk_episodes_episodes_id");
						});

					b.Navigation("Logo");

					b.Navigation("Poster");

					b.Navigation("Season");

					b.Navigation("Show");

					b.Navigation("Thumbnail");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.EpisodeWatchStatus", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Episode", "Episode")
						.WithMany("Watched")
						.HasForeignKey("EpisodeId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_episode_watch_status_episodes_episode_id");

					b.HasOne("Kyoo.Abstractions.Models.User", "User")
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_episode_watch_status_users_user_id");

					b.Navigation("Episode");

					b.Navigation("User");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Movie", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Studio", "Studio")
						.WithMany("Movies")
						.HasForeignKey("StudioId")
						.OnDelete(DeleteBehavior.SetNull)
						.HasConstraintName("fk_movies_studios_studio_id");

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("MovieId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("MovieId");

							b1.ToTable("movies");

							b1.WithOwner()
								.HasForeignKey("MovieId")
								.HasConstraintName("fk_movies_movies_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Poster", b1 =>
						{
							b1.Property<Guid>("MovieId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("poster_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("poster_source");

							b1.HasKey("MovieId");

							b1.ToTable("movies");

							b1.WithOwner()
								.HasForeignKey("MovieId")
								.HasConstraintName("fk_movies_movies_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Thumbnail", b1 =>
						{
							b1.Property<Guid>("MovieId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("thumbnail_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("thumbnail_source");

							b1.HasKey("MovieId");

							b1.ToTable("movies");

							b1.WithOwner()
								.HasForeignKey("MovieId")
								.HasConstraintName("fk_movies_movies_id");
						});

					b.Navigation("Logo");

					b.Navigation("Poster");

					b.Navigation("Studio");

					b.Navigation("Thumbnail");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.MovieWatchStatus", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Movie", "Movie")
						.WithMany("Watched")
						.HasForeignKey("MovieId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_movie_watch_status_movies_movie_id");

					b.HasOne("Kyoo.Abstractions.Models.User", "User")
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_movie_watch_status_users_user_id");

					b.Navigation("Movie");

					b.Navigation("User");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Season", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Show", "Show")
						.WithMany("Seasons")
						.HasForeignKey("ShowId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_seasons_shows_show_id");

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("SeasonId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("SeasonId");

							b1.ToTable("seasons");

							b1.WithOwner()
								.HasForeignKey("SeasonId")
								.HasConstraintName("fk_seasons_seasons_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Poster", b1 =>
						{
							b1.Property<Guid>("SeasonId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("poster_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("poster_source");

							b1.HasKey("SeasonId");

							b1.ToTable("seasons");

							b1.WithOwner()
								.HasForeignKey("SeasonId")
								.HasConstraintName("fk_seasons_seasons_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Thumbnail", b1 =>
						{
							b1.Property<Guid>("SeasonId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("thumbnail_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("thumbnail_source");

							b1.HasKey("SeasonId");

							b1.ToTable("seasons");

							b1.WithOwner()
								.HasForeignKey("SeasonId")
								.HasConstraintName("fk_seasons_seasons_id");
						});

					b.Navigation("Logo");

					b.Navigation("Poster");

					b.Navigation("Show");

					b.Navigation("Thumbnail");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Show", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Studio", "Studio")
						.WithMany("Shows")
						.HasForeignKey("StudioId")
						.OnDelete(DeleteBehavior.SetNull)
						.HasConstraintName("fk_shows_studios_studio_id");

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("ShowId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("ShowId");

							b1.ToTable("shows");

							b1.WithOwner()
								.HasForeignKey("ShowId")
								.HasConstraintName("fk_shows_shows_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Poster", b1 =>
						{
							b1.Property<Guid>("ShowId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("poster_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("poster_source");

							b1.HasKey("ShowId");

							b1.ToTable("shows");

							b1.WithOwner()
								.HasForeignKey("ShowId")
								.HasConstraintName("fk_shows_shows_id");
						});

					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Thumbnail", b1 =>
						{
							b1.Property<Guid>("ShowId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("thumbnail_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("thumbnail_source");

							b1.HasKey("ShowId");

							b1.ToTable("shows");

							b1.WithOwner()
								.HasForeignKey("ShowId")
								.HasConstraintName("fk_shows_shows_id");
						});

					b.Navigation("Logo");

					b.Navigation("Poster");

					b.Navigation("Studio");

					b.Navigation("Thumbnail");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.ShowWatchStatus", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Episode", "NextEpisode")
						.WithMany()
						.HasForeignKey("NextEpisodeId")
						.HasConstraintName("fk_show_watch_status_episodes_next_episode_id");

					b.HasOne("Kyoo.Abstractions.Models.Show", "Show")
						.WithMany("Watched")
						.HasForeignKey("ShowId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_show_watch_status_shows_show_id");

					b.HasOne("Kyoo.Abstractions.Models.User", "User")
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_show_watch_status_users_user_id");

					b.Navigation("NextEpisode");

					b.Navigation("Show");

					b.Navigation("User");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.User", b =>
				{
					b.OwnsOne("Kyoo.Abstractions.Models.Image", "Logo", b1 =>
						{
							b1.Property<Guid>("UserId")
								.HasColumnType("uuid")
								.HasColumnName("id");

							b1.Property<string>("Blurhash")
								.IsRequired()
								.HasMaxLength(32)
								.HasColumnType("character varying(32)")
								.HasColumnName("logo_blurhash");

							b1.Property<string>("Source")
								.IsRequired()
								.HasColumnType("text")
								.HasColumnName("logo_source");

							b1.HasKey("UserId");

							b1.ToTable("users");

							b1.WithOwner()
								.HasForeignKey("UserId")
								.HasConstraintName("fk_users_users_id");
						});

					b.Navigation("Logo");
				});

			modelBuilder.Entity("link_collection_movie", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Collection", null)
						.WithMany()
						.HasForeignKey("collection_id")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_link_collection_movie_collections_collection_id");

					b.HasOne("Kyoo.Abstractions.Models.Movie", null)
						.WithMany()
						.HasForeignKey("movie_id")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_link_collection_movie_movies_movie_id");
				});

			modelBuilder.Entity("link_collection_show", b =>
				{
					b.HasOne("Kyoo.Abstractions.Models.Collection", null)
						.WithMany()
						.HasForeignKey("collection_id")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_link_collection_show_collections_collection_id");

					b.HasOne("Kyoo.Abstractions.Models.Show", null)
						.WithMany()
						.HasForeignKey("show_id")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired()
						.HasConstraintName("fk_link_collection_show_shows_show_id");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Episode", b =>
				{
					b.Navigation("Watched");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Movie", b =>
				{
					b.Navigation("Watched");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Season", b =>
				{
					b.Navigation("Episodes");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Show", b =>
				{
					b.Navigation("Episodes");

					b.Navigation("Seasons");

					b.Navigation("Watched");
				});

			modelBuilder.Entity("Kyoo.Abstractions.Models.Studio", b =>
				{
					b.Navigation("Movies");

					b.Navigation("Shows");
				});
#pragma warning restore 612, 618
		}
	}
}
