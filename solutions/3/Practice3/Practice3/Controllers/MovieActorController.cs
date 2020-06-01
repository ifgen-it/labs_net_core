using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Practice3.Domain.Abstract;
using Practice3.Domain.Concrete;
using Practice3.Domain.Entities;

namespace Practice3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieActorController : ControllerBase
    {
        private readonly IMovieRepository repo;
        private readonly ILogger<ActorController> logger;

        public MovieActorController(IMovieRepository movieRepo, ILogger<ActorController> logger)
        {
            repo = movieRepo;
            this.logger = logger;
        }

        // GET: api/MovieActor
        [HttpGet]
        public ActionResult<IEnumerable<MovieActor>> GetMovieActors() // пока нет пагинации и сортировок 
        {
            var movieActors = repo.MovieActors
                .OrderBy(ma => ma.MovieId)
                    .ThenBy(ma => ma.ActorId)
                .ToList();
            logger.LogInformation($"{DateTime.Now} GET: api/movieactor > all movieactors - not printed");
            return movieActors;
        }

        // GET: api/MovieActor/5/4
        [HttpGet("{MovieId}/{ActorId}")]
        public ActionResult<MovieActor> GetMovieActor(string MovieId, string ActorId)
        {
            if (MovieId == null || ActorId == null)
                return BadRequest();
            int movieId = 0;
            int actorId = 0;
            if (!int.TryParse(MovieId, out movieId) || !int.TryParse(ActorId, out actorId))
                return BadRequest();
            if (movieId <= 0 || actorId <= 0)
                return BadRequest();

            var movieActor = repo.MovieActors
                .Include(ma => ma.Movie)
                    .ThenInclude(m => m.Genre)
                .Include(ma => ma.Actor)
                .FirstOrDefault(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor == null)
                return NotFound();

            logger.LogInformation($"{DateTime.Now} GET: api/movieactor/{movieId}/{actorId} > {movieActor}");
            return movieActor;
        }
        
        // POST: api/MovieActor
        [HttpPost]
        public ActionResult<MovieActor> PostMovieActor(MovieActor movieActor)
        {
            bool movieActorValid = CheckMovieActor(movieActor.MovieId, movieActor.ActorId);
            if (!movieActorValid)
                return BadRequest("Incorrect parameters or this pair movie-actor already in use");
            else
            {
                repo.AddActorToMovie(movieActor.ActorId, movieActor.MovieId);

                var ma = repo.MovieActors
                    .FirstOrDefault(ma => ma.MovieId == movieActor.MovieId && ma.ActorId == movieActor.ActorId);

                logger.LogInformation($"{DateTime.Now} POST: api/movieactor > {ma}");
                return CreatedAtAction("GetMovieActor", 
                    new { MovieId = movieActor.MovieId.ToString(), ActorId = movieActor.ActorId.ToString() }, ma);
            }
        }

        // DELETE: api/MovieActor/5/4
        [HttpDelete("{MovieId}/{ActorId}")]
        public ActionResult<MovieActor> DeleteMovieActor(string MovieId, string ActorId)
        {
            if (MovieId == null || ActorId == null)
                return BadRequest();
            int movieId = 0;
            int actorId = 0;
            if (!int.TryParse(MovieId, out movieId) || !int.TryParse(ActorId, out actorId))
                return BadRequest();
            if (movieId <= 0 || actorId <= 0)
                return BadRequest();

            if (!repo.MovieActorExists(movieId, actorId))
                return NotFound();

            var ma = repo.MovieActors
                    .FirstOrDefault(ma => ma.MovieId == movieId && ma.ActorId == actorId);
            
            repo.RemoveActorFromMovie(actorId, movieId);
            logger.LogInformation($"{DateTime.Now} DELETE: api/movieactor/{movieId}/{actorId} > {ma}");
            return ma;
        }


        private bool CheckMovieActor(int movieId, int actorId)
        {
            if (movieId <= 0 || actorId <= 0)
                return false;

            bool movieExists = repo.Movies
                            .Any(m => m.Id == movieId);
            bool actorExists = repo.Actors
                            .Any(a => a.Id == actorId);
            if ((movieExists && actorExists) == false)
                return false;

            if (repo.MovieActorExists(movieId, actorId))
                return false;

            return true;
        }
    }
}
