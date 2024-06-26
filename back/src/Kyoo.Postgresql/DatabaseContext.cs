// Kyoo - A portable and vast media library solution.
// Copyright (c) Kyoo.
//
// See AUTHORS.md and LICENSE file in the project root for full license information.
//
// Kyoo is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// Kyoo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Kyoo. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kyoo.Abstractions.Models;
using Kyoo.Abstractions.Models.Exceptions;
using Kyoo.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kyoo.Postgresql;

public abstract class DatabaseContext : DbContext
{
	private readonly IHttpContextAccessor _accessor;

	/// <summary>
	/// Calculate the MD5 of a string, can only be used in database context.
	/// </summary>
	/// <param name="str">The string to hash</param>
	/// <returns>The hash</returns>
	public static string MD5(string str) => throw new NotSupportedException();

	public Guid? CurrentUserId => _accessor.HttpContext?.User.GetId();

	public DbSet<Collection> Collections { get; set; }

	public DbSet<Movie> Movies { get; set; }

	public DbSet<Show> Shows { get; set; }

	public DbSet<Season> Seasons { get; set; }

	public DbSet<Episode> Episodes { get; set; }

	public DbSet<Studio> Studios { get; set; }

	public DbSet<User> Users { get; set; }

	public DbSet<MovieWatchStatus> MovieWatchStatus { get; set; }

	public DbSet<ShowWatchStatus> ShowWatchStatus { get; set; }

	public DbSet<EpisodeWatchStatus> EpisodeWatchStatus { get; set; }

	public DbSet<Issue> Issues { get; set; }

	public DbSet<ServerOption> Options { get; set; }

	/// <summary>
	/// Add a many to many link between two resources.
	/// </summary>
	/// <remarks>Types are order dependant. You can't inverse the order. Please always put the owner first.</remarks>
	/// <param name="first">The ID of the first resource.</param>
	/// <param name="second">The ID of the second resource.</param>
	/// <typeparam name="T1">The first resource type of the relation. It is the owner of the second</typeparam>
	/// <typeparam name="T2">The second resource type of the relation. It is the contained resource.</typeparam>
	public void AddLinks<T1, T2>(Guid first, Guid second)
		where T1 : class, IResource
		where T2 : class, IResource
	{
		Set<Dictionary<string, object>>(LinkName<T1, T2>())
			.Add(
				new Dictionary<string, object>
				{
					[LinkNameFk<T1>()] = first,
					[LinkNameFk<T2>()] = second
				}
			);
	}

	protected DatabaseContext(IHttpContextAccessor accessor)
	{
		_accessor = accessor;
	}

	protected DatabaseContext(DbContextOptions options, IHttpContextAccessor accessor)
		: base(options)
	{
		_accessor = accessor;
	}

	protected abstract string LinkName<T, T2>()
		where T : IResource
		where T2 : IResource;

	protected abstract string LinkNameFk<T>()
		where T : IResource;

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	}

	private static void _HasJson<T, TVal>(
		ModelBuilder builder,
		Expression<Func<T, Dictionary<string, TVal>>> property
	)
		where T : class
	{
		// TODO: Waiting for https://github.com/dotnet/efcore/issues/29825
		// modelBuilder.Entity<T>()
		// 	.OwnsOne(x => x.ExternalId, x =>
		// 	{
		// 		x.ToJson();
		// 	});
		builder
			.Entity<T>()
			.Property(property)
			.HasConversion(
				v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
				v =>
					JsonSerializer.Deserialize<Dictionary<string, TVal>>(
						v,
						(JsonSerializerOptions?)null
					)!
			)
			.HasColumnType("json");
		builder
			.Entity<T>()
			.Property(property)
			.Metadata.SetValueComparer(
				new ValueComparer<Dictionary<string, TVal>>(
					(c1, c2) => c1!.SequenceEqual(c2!),
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()))
				)
			);
	}

	private static void _HasMetadata<T>(ModelBuilder modelBuilder)
		where T : class, IMetadata
	{
		_HasJson<T, MetadataId>(modelBuilder, x => x.ExternalId);
	}

	private static void _HasImages<T>(ModelBuilder modelBuilder)
		where T : class, IThumbnails
	{
		modelBuilder.Entity<T>().OwnsOne(x => x.Poster, x => x.ToJson());
		modelBuilder.Entity<T>().OwnsOne(x => x.Thumbnail, x => x.ToJson());
		modelBuilder.Entity<T>().OwnsOne(x => x.Logo, x => x.ToJson());
	}

	private static void _HasAddedDate<T>(ModelBuilder modelBuilder)
		where T : class, IAddedDate
	{
		modelBuilder
			.Entity<T>()
			.Property(x => x.AddedDate)
			.HasDefaultValueSql("now() at time zone 'utc'")
			.ValueGeneratedOnAdd();
	}

	private static void _HasRefreshDate<T>(ModelBuilder builder)
		where T : class, IRefreshable
	{
		// schedule a refresh soon since metadata can change frequently for recently added items ond online databases
		builder
			.Entity<T>()
			.Property(x => x.NextMetadataRefresh)
			.HasDefaultValueSql("now() at time zone 'utc' + interval '2 hours'")
			.ValueGeneratedOnAdd();
	}

	private void _HasManyToMany<T, T2>(
		ModelBuilder modelBuilder,
		Expression<Func<T, IEnumerable<T2>?>> firstNavigation,
		Expression<Func<T2, IEnumerable<T>?>> secondNavigation
	)
		where T : class, IResource
		where T2 : class, IResource
	{
		modelBuilder
			.Entity<T2>()
			.HasMany(secondNavigation)
			.WithMany(firstNavigation)
			.UsingEntity<Dictionary<string, object>>(
				LinkName<T, T2>(),
				x =>
					x.HasOne<T>()
						.WithMany()
						.HasForeignKey(LinkNameFk<T>())
						.OnDelete(DeleteBehavior.Cascade),
				x =>
					x.HasOne<T2>()
						.WithMany()
						.HasForeignKey(LinkNameFk<T2>())
						.OnDelete(DeleteBehavior.Cascade)
			);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Show>().Ignore(x => x.FirstEpisode).Ignore(x => x.AirDate);
		modelBuilder.Entity<Episode>().Ignore(x => x.PreviousEpisode).Ignore(x => x.NextEpisode);

		modelBuilder
			.Entity<Show>()
			.HasMany(x => x.Seasons)
			.WithOne(x => x.Show)
			.OnDelete(DeleteBehavior.Cascade);
		modelBuilder
			.Entity<Show>()
			.HasMany(x => x.Episodes)
			.WithOne(x => x.Show)
			.OnDelete(DeleteBehavior.Cascade);
		modelBuilder
			.Entity<Season>()
			.HasMany(x => x.Episodes)
			.WithOne(x => x.Season)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder
			.Entity<Movie>()
			.HasOne(x => x.Studio)
			.WithMany(x => x.Movies)
			.OnDelete(DeleteBehavior.SetNull);
		modelBuilder
			.Entity<Show>()
			.HasOne(x => x.Studio)
			.WithMany(x => x.Shows)
			.OnDelete(DeleteBehavior.SetNull);

		_HasManyToMany<Collection, Movie>(modelBuilder, x => x.Movies, x => x.Collections);
		_HasManyToMany<Collection, Show>(modelBuilder, x => x.Shows, x => x.Collections);

		_HasMetadata<Collection>(modelBuilder);
		_HasMetadata<Movie>(modelBuilder);
		_HasMetadata<Show>(modelBuilder);
		_HasMetadata<Season>(modelBuilder);
		_HasMetadata<Studio>(modelBuilder);
		_HasJson<Episode, EpisodeId>(modelBuilder, x => x.ExternalId);

		_HasImages<Collection>(modelBuilder);
		_HasImages<Movie>(modelBuilder);
		_HasImages<Show>(modelBuilder);
		_HasImages<Season>(modelBuilder);
		_HasImages<Episode>(modelBuilder);

		_HasAddedDate<Collection>(modelBuilder);
		_HasAddedDate<Movie>(modelBuilder);
		_HasAddedDate<Show>(modelBuilder);
		_HasAddedDate<Season>(modelBuilder);
		_HasAddedDate<Episode>(modelBuilder);
		_HasAddedDate<User>(modelBuilder);
		_HasAddedDate<Issue>(modelBuilder);

		_HasRefreshDate<Collection>(modelBuilder);
		_HasRefreshDate<Movie>(modelBuilder);
		_HasRefreshDate<Show>(modelBuilder);
		_HasRefreshDate<Season>(modelBuilder);
		_HasRefreshDate<Episode>(modelBuilder);

		modelBuilder
			.Entity<MovieWatchStatus>()
			.HasKey(x => new { User = x.UserId, Movie = x.MovieId });
		modelBuilder
			.Entity<ShowWatchStatus>()
			.HasKey(x => new { User = x.UserId, Show = x.ShowId });
		modelBuilder
			.Entity<EpisodeWatchStatus>()
			.HasKey(x => new { User = x.UserId, Episode = x.EpisodeId });

		modelBuilder
			.Entity<MovieWatchStatus>()
			.HasOne(x => x.Movie)
			.WithMany(x => x.Watched)
			.HasForeignKey(x => x.MovieId)
			.OnDelete(DeleteBehavior.Cascade);
		modelBuilder
			.Entity<ShowWatchStatus>()
			.HasOne(x => x.Show)
			.WithMany(x => x.Watched)
			.HasForeignKey(x => x.ShowId)
			.OnDelete(DeleteBehavior.Cascade);
		modelBuilder
			.Entity<ShowWatchStatus>()
			.HasOne(x => x.NextEpisode)
			.WithMany()
			.HasForeignKey(x => x.NextEpisodeId)
			.OnDelete(DeleteBehavior.SetNull);
		modelBuilder
			.Entity<EpisodeWatchStatus>()
			.HasOne(x => x.Episode)
			.WithMany(x => x.Watched)
			.HasForeignKey(x => x.EpisodeId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<MovieWatchStatus>().HasQueryFilter(x => x.UserId == CurrentUserId);
		modelBuilder.Entity<ShowWatchStatus>().HasQueryFilter(x => x.UserId == CurrentUserId);
		modelBuilder.Entity<EpisodeWatchStatus>().HasQueryFilter(x => x.UserId == CurrentUserId);

		modelBuilder.Entity<ShowWatchStatus>().Navigation(x => x.NextEpisode).AutoInclude();

		_HasAddedDate<MovieWatchStatus>(modelBuilder);
		_HasAddedDate<ShowWatchStatus>(modelBuilder);
		_HasAddedDate<EpisodeWatchStatus>(modelBuilder);

		modelBuilder.Entity<Movie>().Ignore(x => x.WatchStatus);
		modelBuilder.Entity<Show>().Ignore(x => x.WatchStatus);
		modelBuilder.Entity<Episode>().Ignore(x => x.WatchStatus);

		modelBuilder.Entity<Collection>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder.Entity<Movie>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder.Entity<Show>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder.Entity<Studio>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder
			.Entity<Season>()
			.HasIndex(x => new { ShowID = x.ShowId, x.SeasonNumber })
			.IsUnique();
		modelBuilder.Entity<Season>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder
			.Entity<Episode>()
			.HasIndex(x => new
			{
				ShowID = x.ShowId,
				x.SeasonNumber,
				x.EpisodeNumber,
				x.AbsoluteNumber
			})
			.IsUnique();
		modelBuilder.Entity<Episode>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder.Entity<User>().HasIndex(x => x.Slug).IsUnique();
		modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();

		modelBuilder.Entity<Movie>().Ignore(x => x.Links);

		modelBuilder.Entity<Issue>().HasKey(x => new { x.Domain, x.Cause });

		_HasJson<User, string>(modelBuilder, x => x.Settings);
		_HasJson<User, ExternalToken>(modelBuilder, x => x.ExternalId);
		_HasJson<Issue, object>(modelBuilder, x => x.Extra);

		modelBuilder.Entity<ServerOption>().HasKey(x => x.Key);
	}

	public override int SaveChanges()
	{
		try
		{
			return base.SaveChanges();
		}
		catch (DbUpdateException ex)
		{
			DiscardChanges();
			if (IsDuplicateException(ex))
				throw new DuplicatedItemException();
			throw;
		}
	}

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		try
		{
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}
		catch (DbUpdateException ex)
		{
			DiscardChanges();
			if (IsDuplicateException(ex))
				throw new DuplicatedItemException();
			throw;
		}
	}

	public override async Task<int> SaveChangesAsync(
		bool acceptAllChangesOnSuccess,
		CancellationToken cancellationToken = default
	)
	{
		try
		{
			return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
		catch (DbUpdateException ex)
		{
			DiscardChanges();
			if (IsDuplicateException(ex))
				throw new DuplicatedItemException();
			throw;
		}
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await base.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException ex)
		{
			DiscardChanges();
			if (IsDuplicateException(ex))
				throw new DuplicatedItemException();
			throw;
		}
	}

	public async Task<int> SaveChangesAsync<T>(
		Func<Task<T>> getExisting,
		CancellationToken cancellationToken = default
	)
	{
		try
		{
			return await SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException ex)
		{
			DiscardChanges();
			if (IsDuplicateException(ex))
				throw new DuplicatedItemException(await getExisting());
			throw;
		}
		catch (DuplicatedItemException)
		{
			throw new DuplicatedItemException(await getExisting());
		}
	}

	public async Task<int> SaveIfNoDuplicates(CancellationToken cancellationToken = default)
	{
		try
		{
			return await SaveChangesAsync(cancellationToken);
		}
		catch (DuplicatedItemException)
		{
			return -1;
		}
	}

	public T? LocalEntity<T>(string slug)
		where T : class, IResource
	{
		return ChangeTracker.Entries<T>().FirstOrDefault(x => x.Entity.Slug == slug)?.Entity;
	}

	protected abstract bool IsDuplicateException(Exception ex);

	public void DiscardChanges()
	{
		foreach (
			EntityEntry entry in ChangeTracker.Entries().Where(x => x.State != EntityState.Detached)
		)
		{
			entry.State = EntityState.Detached;
		}
	}
}
