﻿using Practice3.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice3.Domain.Abstract
{
    public interface IMovieRepository
    {
        IQueryable<Movie> Movies { get; }
        IQueryable<Actor> Actors { get; }
        IQueryable<Genre> Genres { get; }
        IQueryable<MovieActor> MovieActors { get; }
        int Add(object entity);
        void Remove(object entity);
        void Update(object entity);
        Movie FindMovie(int id);
        Actor FindActor(int id);
        Genre FindGenre(int id);
        bool AddActorToMovie(int actorId, int movieId);
        bool RemoveActorFromMovie(int actorId, int movieId);
        IQueryable<Actor> ActorsFromMovie(int movieId);
        bool MovieActorExists(int movieId, int actorId);
        
    }
}
