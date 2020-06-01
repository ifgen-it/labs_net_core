using System;
using System.Collections.Generic;
using System.IO;
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
    public class ActorController : ControllerBase
    {
        private readonly IMovieRepository repo;
        private readonly ILogger<ActorController> logger;

        public ActorController(IMovieRepository movieRepo, ILogger<ActorController> logger)
        {
            repo = movieRepo;
            this.logger = logger;
        }

        // GET: api/Actor
        [HttpGet]
        public ActionResult<IEnumerable<Actor>> GetActors() // пока нет пагинации и сортировок 
        {
            var actors = repo.Actors
                    .OrderBy(a => a.Id)
                    .ToList();
            logger.LogInformation($"{DateTime.Now} GET: api/actor > all actors - not printed");
            return actors;
        }

        // GET: api/Actor/5
        [HttpGet("{id}")]
        public ActionResult<Actor> GetActor(int id)
        {
            if (id <= 0)
                return BadRequest();

            var actor = repo.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                        .ThenInclude(m => m.Genre)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null)
                return NotFound();
            logger.LogInformation($"{DateTime.Now} GET: api/actor/{id} > {actor}");
            return actor;
        }
        // GET: api/Actor/5/photo
        [HttpGet("{id}/photo")]
        public IActionResult GetActorPhoto(int id)
        {
            if (id <= 0)
                return BadRequest();

            var actor = repo.Actors.FirstOrDefault(a => a.Id == id);
            if (actor == null)
                return NotFound();

            var imagePath = $"/images/actors/{actor.FirstName} {actor.LastName}.jpg";
            var imagePathStub = $"/images/actors/no_avatar.png";
            var curDir = Directory.GetCurrentDirectory();

            if (!System.IO.File.Exists(curDir + "/wwwroot" + imagePath))
            {
                imagePath = imagePathStub;
                string absolutePath = curDir + "/wwwroot" + imagePath;
                var fileResult = PhysicalFile(absolutePath, "image/png", "no_avatar.png");
                logger.LogInformation($"{DateTime.Now} GET: api/movie/{id}/photo > no_avatar.png");
                return fileResult;
            }
            else
            {
                string absolutePath = curDir + "/wwwroot" + imagePath;
                var fileResult = PhysicalFile(absolutePath, "image/jpeg", $"{actor.FirstName} {actor.LastName}.jpg");
                logger.LogInformation($"{DateTime.Now} GET: api/movie/{id}/photo > {actor.FirstName} {actor.LastName}.jpg");
                return fileResult;
            }   
        }
        // GET: api/Actor/top
        [HttpGet("top")]
        public ActionResult<Actor> GetTopActor()
        {
            var topActor = repo.Actors
            .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
                    .ThenInclude(m => m.Genre)
            .OrderBy(a => -a.MovieActors.Count)
            .FirstOrDefault();

            if (topActor == null)
                return NotFound();
            logger.LogInformation($"{DateTime.Now} GET: api/actor/top > top actor: {topActor}");
            return topActor;
        }

        // PUT: api/Actor/5
        [HttpPut("{id}")]
        public ActionResult<Actor> PutActor(int id, Actor actor)
        {
            if (id != actor.Id)
                return BadRequest();

            bool actorNameValid = CheckActor(actor.FirstName, actor.LastName, actor.Id);
            if (!actorNameValid)
                return BadRequest("This actor name already in use");
            else
            {
                actor.FirstName = actor.FirstName.Trim();
                actor.LastName = actor.LastName.Trim();
                repo.Update(actor);
                logger.LogInformation($"{DateTime.Now} PUT: api/actor/{id} > {actor}");
                return actor;
            }
        }

        // POST: api/Actor
        [HttpPost]
        public ActionResult<Actor> PostActor(Actor actor)
        {
            bool actorNameValid = CheckActor(actor.FirstName, actor.LastName, actor.Id);
            if (!actorNameValid)
                return BadRequest("This actor name already in use");
            else
            {
                actor.FirstName = actor.FirstName.Trim();
                actor.LastName = actor.LastName.Trim();
                int actorId = repo.Add(actor);
                logger.LogInformation($"{DateTime.Now} POST: api/actor > {actor}");
                return CreatedAtAction("GetActor", new { id = actorId }, actor);
            }
        }

        // DELETE: api/Actor/5
        [HttpDelete("{id}")]
        public ActionResult<Actor> DeleteActor(int id)
        {
            if (id <= 0)
                return BadRequest();

            var actor = repo.FindActor(id);
            if (actor == null)
                return NotFound();

            repo.Remove(actor);
            logger.LogInformation($"{DateTime.Now} DELETE: api/actor/{id} > {actor}");
            return actor;
        }

        private bool ActorExists(int id)
        {
            return repo.Actors.Any(a => a.Id == id);
        }

        private bool CheckActor(string firstName, string lastName, int id)
        {
            if (firstName == null || lastName == null
                || firstName.Equals("") || lastName.Equals(""))
                return false;
            var actor = repo.Actors
                            .FirstOrDefault(a => a.FirstName.Equals(firstName.Trim())
                            && a.LastName.Equals(lastName.Trim()));
            if (actor == null || (actor != null && actor.Id == id))
                return true;
            else
                return false;
        }
    }
}
