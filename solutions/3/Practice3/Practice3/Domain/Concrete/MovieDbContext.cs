using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Practice3.Domain.Entities;
using Practice3.Domain.Utils;


namespace Practice3.Domain.Concrete
{
    public partial class MovieDbContext : DbContext
    {
        private static int counter;
        public MovieDbContext()
        {
            if (counter == 0)
            {
                Database.EnsureDeleted();
                Database.EnsureCreated();
                this.InsertDataIntoDb();
            }
            ++counter;
        }


        public virtual DbSet<Actor> Actors { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<MovieActor> MovieActors { get; set; }
        public virtual DbSet<Movie> Movies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MovieDb;Trusted_connection=TRUE");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieActor>(entity =>
            {
                entity.HasKey(e => new { e.MovieId, e.ActorId });

                entity.HasIndex(e => e.ActorId);

                entity.HasOne(d => d.Actor)
                    .WithMany(p => p.MovieActors)
                    .HasForeignKey(d => d.ActorId);

                entity.HasOne(d => d.Movie)
                    .WithMany(p => p.MovieActors)
                    .HasForeignKey(d => d.MovieId);
            });

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasIndex(e => e.GenreId);

                entity.HasOne(d => d.Genre)
                    .WithMany(p => p.Movies)
                    .HasForeignKey(d => d.GenreId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
