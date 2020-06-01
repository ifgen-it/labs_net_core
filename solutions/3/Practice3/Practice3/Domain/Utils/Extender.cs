using Practice3.Domain.Concrete;
using Practice3.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice3.Domain.Utils
{
    public static class Extender
    {
        public static void Copy(this Movie movie, Movie other)
        {
            movie.Id = other.Id;
            movie.Name = other.Name;
            movie.Year = other.Year;
            movie.Rating = other.Rating;
            movie.Price = other.Price;
            movie.GenreId = other.GenreId;
            movie.Genre = other.Genre;
            movie.MovieActors = other.MovieActors;
        }
        public static void Copy(this Actor actor, Actor other)
        {
            actor.Id = other.Id;
            actor.FirstName = other.FirstName;
            actor.LastName = other.LastName;
            actor.MovieActors = other.MovieActors;
        }
        public static void Copy(this Genre genre, Genre other)
        {
            genre.Id = other.Id;
            genre.Name = other.Name;
            genre.Movies = other.Movies;
        }
        public static void InsertDataIntoDb(this MovieDbContext db)
        {
            // Genres
            Genre g1 = new Genre { Name = "drama" };
            Genre g2 = new Genre { Name = "fantasy" };
            Genre g3 = new Genre { Name = "crime" };
            Genre g4 = new Genre { Name = "comedy" };
            Genre g5 = new Genre { Name = "thriller" };
            db.Genres.AddRange(g1, g2, g3, g4, g5);

            // Movies
            Movie m1 = new Movie { Name = "The Shawshank Redemption", Genre = g1, Year = 1994, Rating = 9.1f, Price = 300 };
            Movie m2 = new Movie { Name = "The Matrix", Genre = g2, Year = 1999, Rating = 8.5f, Price = 250 };
            Movie m3 = new Movie { Name = "Inception", Genre = g2, Year = 2010, Rating = 8.66f, Price = 220 };
            Movie m4 = new Movie { Name = "The Thirteenth Floor", Genre = g2, Year = 1999, Rating = 7.575f, Price = 180 };
            Movie m5 = new Movie { Name = "Green Book", Genre = g1, Year = 2018, Rating = 8.348f, Price = 320 };
            Movie m6 = new Movie { Name = "Training Day", Genre = g3, Year = 2001, Rating = 7.831f, Price = 260 };
            Movie m7 = new Movie { Name = "Scarface", Genre = g3, Year = 1983, Rating = 8.181f, Price = 400 };
            Movie m8 = new Movie { Name = "The Mask", Genre = g4, Year = 1994, Rating = 7.974f, Price = 330 };
            Movie m9 = new Movie { Name = "The Departed", Genre = g3, Year = 2006, Rating = 8.455f, Price = 290 };
            Movie m10 = new Movie { Name = "The Recruit", Genre = g5, Year = 2003, Rating = 7.364f, Price = 350 };
            db.Movies.AddRange(m1, m2, m3, m4, m5, m6, m7, m8, m9, m10);

            // Actors
            Actor a1 = new Actor { FirstName = "Tim", LastName = "Robbins" };
            Actor a2 = new Actor { FirstName = "Morgan", LastName = "Freeman" };
            Actor a3 = new Actor { FirstName = "Keanu", LastName = "Reeves" };
            Actor a4 = new Actor { FirstName = "Laurence", LastName = "Fishburne" };
            Actor a5 = new Actor { FirstName = "Carrie-Anne", LastName = "Moss" };
            Actor a6 = new Actor { FirstName = "Hugo", LastName = "Weaving" };
            Actor a7 = new Actor { FirstName = "Leonardo", LastName = "DiCaprio" };
            Actor a8 = new Actor { FirstName = "Joseph", LastName = "Gordon-Levitt" };
            Actor a9 = new Actor { FirstName = "Ellen", LastName = "Page" };
            Actor a10 = new Actor { FirstName = "Tom", LastName = "Hardy" };
            Actor a11 = new Actor { FirstName = "Craig", LastName = "Bierko" };
            Actor a12 = new Actor { FirstName = "Gretchen", LastName = "Mol" };
            Actor a13 = new Actor { FirstName = "Armin", LastName = "Mueller-Stahl" };
            Actor a14 = new Actor { FirstName = "Viggo", LastName = "Mortensen" };
            Actor a15 = new Actor { FirstName = "Denzel", LastName = "Washington" };
            Actor a16 = new Actor { FirstName = "Ethan", LastName = "Hawke" };
            Actor a17 = new Actor { FirstName = "Al", LastName = "Pacino" };
            Actor a18 = new Actor { FirstName = "Steven", LastName = "Bauer" };
            Actor a19 = new Actor { FirstName = "Jim", LastName = "Carrey" };
            Actor a20 = new Actor { FirstName = "Cameron", LastName = "Diaz" };
            Actor a21 = new Actor { FirstName = "Matt", LastName = "Damon" };
            Actor a22 = new Actor { FirstName = "Jack", LastName = "Nicholson" };
            Actor a23 = new Actor { FirstName = "Mark", LastName = "Wahlberg" };
            Actor a24 = new Actor { FirstName = "Colin", LastName = "Farrell" };
            db.Actors.AddRange(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12,
                a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24);

            // MovieActor
            new List<MovieActor> {
                    new MovieActor { Movie = m1, Actor = a1 },
                    new MovieActor { Movie = m1, Actor = a2 }}
                        .ForEach(ma => m1.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m2, Actor = a3 },
                    new MovieActor { Movie = m2, Actor = a4 },
                    new MovieActor { Movie = m2, Actor = a5 },
                    new MovieActor { Movie = m2, Actor = a6 }}
                        .ForEach(ma => m2.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m3, Actor = a7 },
                    new MovieActor { Movie = m3, Actor = a8 },
                    new MovieActor { Movie = m3, Actor = a9 },
                    new MovieActor { Movie = m3, Actor = a10 }}
                        .ForEach(ma => m3.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m4, Actor = a11 },
                    new MovieActor { Movie = m4, Actor = a12 },
                    new MovieActor { Movie = m4, Actor = a13 }}
                        .ForEach(ma => m4.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m5, Actor = a14 }}
                        .ForEach(ma => m5.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m6, Actor = a15 },
                    new MovieActor { Movie = m6, Actor = a16 }}
                        .ForEach(ma => m6.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m7, Actor = a17 },
                    new MovieActor { Movie = m7, Actor = a18 }}
                        .ForEach(ma => m7.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m8, Actor = a19 },
                    new MovieActor { Movie = m8, Actor = a20 }}
                        .ForEach(ma => m8.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m9, Actor = a7 },
                    new MovieActor { Movie = m9, Actor = a21 },
                    new MovieActor { Movie = m9, Actor = a22 },
                    new MovieActor { Movie = m9, Actor = a23 }}
                        .ForEach(ma => m9.MovieActors.Add(ma));
            new List<MovieActor> {
                    new MovieActor { Movie = m10, Actor = a17 },
                    new MovieActor { Movie = m10, Actor = a24 }}
                        .ForEach(ma => m10.MovieActors.Add(ma));
            db.SaveChanges();
        }

    }
}
