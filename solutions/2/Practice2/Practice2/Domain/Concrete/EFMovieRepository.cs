using Practice2.Domain.Abstract;
using Practice2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Practice2.Domain.Utils;

namespace Practice2.Domain.Concrete
{
    public class EFMovieRepository : IMovieRepository
    {
        private readonly MovieDbContext db;
        public EFMovieRepository(MovieDbContext dbContext)
        {
            db = dbContext;
        }

        public IQueryable<Movie> Movies => db.Movies;
        public IQueryable<Actor> Actors => db.Actors;
        public IQueryable<Genre> Genres => db.Genres;

        public int Add(object entity)
        {
            switch (entity)
            {
                case Movie movie:
                    db.Add(movie);
                    db.SaveChanges();
                    return movie.Id;
                case Actor actor:
                    db.Add(actor);
                    db.SaveChanges();
                    return actor.Id;
                case Genre genre:
                    db.Add(genre);
                    db.SaveChanges();
                    return genre.Id;
                default:
                    break;
            }
            return 0;
        }
        public void Remove(object entity)
        {
            switch (entity)
            {
                case Movie movie:
                    var foundMovie = db.Movies.FirstOrDefault(m => m.Id == movie.Id);
                    db.Remove(foundMovie);
                    break;
                case Actor actor:
                    var foundActor = db.Actors.FirstOrDefault(a => a.Id == actor.Id);
                    db.Remove(foundActor);
                    break;
                case Genre genre:
                    var foundGenre = db.Genres.FirstOrDefault(g => g.Id == genre.Id);
                    db.Remove(foundGenre);
                    break;
                default:
                    break;
            }
            db.SaveChanges();
        }
        public void Update(object entity)
        {
            switch (entity)
            {
                case Movie movie:
                    var foundMovie = db.Movies.FirstOrDefault(m => m.Id == movie.Id);
                    db.Entry(foundMovie).CurrentValues.SetValues(movie);
                    break;
                case Actor actor:
                    var foundActor = db.Actors.FirstOrDefault(a => a.Id == actor.Id);
                    db.Entry(foundActor).CurrentValues.SetValues(actor);
                    break;
                case Genre genre:
                    var foundGenre = db.Genres.FirstOrDefault(g => g.Id == genre.Id);
                    db.Entry(foundGenre).CurrentValues.SetValues(genre);
                    break;
                default:
                    break;
            }
            db.SaveChanges();
        }
        public Actor FindActor(int id)
        {
            return db.Actors.Find(id);
        }
        public Genre FindGenre(int id)
        {
            return db.Genres.Find(id);
        }
        public Movie FindMovie(int id)
        {
            return db.Movies.Find(id);
        }

        public bool AddActorToMovie(int actorId, int movieId)
        {
            var actor = db.Actors.Find(actorId);
            var movie = db.Movies.Find(movieId);
            if (actor == null || movie == null)
                return false;
            
            movie.MovieActors.Add(new MovieActor { Movie = movie, Actor = actor });
            db.SaveChanges();
            return true;
        }

        public bool RemoveActorFromMovie(int actorId, int movieId)
        {
            var actor = db.Actors.Find(actorId);
            var movie = db.Movies.Find(movieId);
            if (actor == null || movie == null)
                return false;

            var ma = movie.MovieActors.FirstOrDefault(ma => ma.ActorId == actorId);
            movie.MovieActors.Remove(ma);
            db.SaveChanges();
            return true;
        }

        public IQueryable<Actor> ActorsFromMovie(int movieId)
        {
            var actorsId = db.MovieActors
                .Where(ma => ma.MovieId == movieId)
                .Select(ma => ma.ActorId).ToList();
            var actors = db.Actors.Where(a => actorsId.Contains(a.Id))
                            .OrderBy(a => a.LastName);

            return actors;
        }
    }
}
