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
    public class GenreController : ControllerBase
    {
        private readonly IMovieRepository repo;
        private readonly ILogger<ActorController> logger;

        public GenreController(IMovieRepository movieRepo, ILogger<ActorController> logger)
        {
            repo = movieRepo;
            this.logger = logger;
        }

        // GET: api/Genre
        [HttpGet]
        public ActionResult<IEnumerable<Genre>> GetGenres() // пока нет пагинации и сортировок 
        {
            var genres = repo.Genres
                .OrderBy(g => g.Id)
                .ToList();
            logger.LogInformation($"{DateTime.Now} GET: api/genre > all genres - not printed");
            return genres;
        }

        // GET: api/Genre/5
        [HttpGet("{id}")]
        public ActionResult<Genre> GetGenre(int id)
        {
            if (id <= 0)
                return BadRequest();

            var genre = repo.Genres
                .Include(g => g.Movies)
                .FirstOrDefault(a => a.Id == id);
            if (genre == null)
                return NotFound();

            logger.LogInformation($"{DateTime.Now} GET: api/genre/{id} > {genre}");
            return genre;
        }

        // PUT: api/Genre/5
        [HttpPut("{id}")]
        public ActionResult<Genre> PutGenre(int id, Genre genre)
        {
            if (id != genre.Id)
                return BadRequest();

            bool genreNameValid = CheckGenre(genre.Name, genre.Id);
            if (!genreNameValid)
                return BadRequest("This genre name already in use");
            else
            {
                genre.Name = genre.Name.Trim();
                repo.Update(genre);
                logger.LogInformation($"{DateTime.Now} PUT: api/genre/{id} > {genre}");
                return genre;
            }
        }

        // POST: api/Genre
        [HttpPost]
        public ActionResult<Genre> PostGenre(Genre genre)
        {
            bool genreNameValid = CheckGenre(genre.Name, genre.Id);
            if (!genreNameValid)
                return BadRequest("This genre name already in use");
            else
            {
                genre.Name = genre.Name.Trim();
                int genreId = repo.Add(genre);
                logger.LogInformation($"{DateTime.Now} POST: api/genre > {genre}");
                return CreatedAtAction("GetGenre", new { id = genreId }, genre);
            }
        }

        // DELETE: api/Genre/5
        [HttpDelete("{id}")]
        public ActionResult<Genre> DeleteGenre(int id)
        {
            if (id <= 0)
                return BadRequest();

            var genre = repo.FindGenre(id);
            if (genre == null)
                return NotFound();

            repo.Remove(genre);
            logger.LogInformation($"{DateTime.Now} DELETE: api/genre/{id} > {genre}");
            return genre;
        }

        private bool GenreExists(int id)
        {
            return repo.Genres.Any(g => g.Id == id);
        }

        private bool CheckGenre(string genreName, int id)
        {
            if (genreName == null || genreName.Equals(""))
                return false;

            var genre = repo.Genres
                            .FirstOrDefault(g => g.Name.Equals(genreName.Trim()));
            if (genre == null || (genre != null && genre.Id == id))
                return true;
            else
                return false;
        }
    }
}
