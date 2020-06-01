using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class MovieController : ControllerBase
    {
        private readonly IMovieRepository repo;
        private readonly ILogger<ActorController> logger;

        public MovieController(IMovieRepository movieRepo, ILogger<ActorController> logger)
        {
            repo = movieRepo;
            this.logger = logger;
        }

        // GET: api/Movie
        [HttpGet]
        public ActionResult<IEnumerable<Movie>> GetMovies() // пока нет пагинации и сортировок 
        {
            var movies = repo.Movies
                    .Include(m => m.Genre)
                    .OrderBy(m => m.Id)
                    .ToList();
            logger.LogInformation($"{DateTime.Now} GET: api/movie > all movies - not printed");
            return movies;
        }

        // GET: api/Movie/5
        [HttpGet("{id}")]
        public ActionResult<Movie> GetMovie(int id)
        {
            if (id <= 0)
                return BadRequest();

            var movie = repo.Movies
                .Include(m => m.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            logger.LogInformation($"{DateTime.Now} GET: api/movie/{id} > {movie}");
            return movie;
        }

        // GET: api/Movie/5/photo
        [HttpGet("{id}/photo")]
        public IActionResult GetMoviePhoto(int id)
        {
            if (id <= 0)
                return BadRequest();

            var movie = repo.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            var imagePath = $"/images/movies/{movie.Name}.jpg";
            var imagePathStub = $"/images/movies/no_signal.jpg";
            var curDir = Directory.GetCurrentDirectory();

            if (!System.IO.File.Exists(curDir + "/wwwroot" + imagePath))
            {
                imagePath = imagePathStub;
                string absolutePath = curDir + "/wwwroot" + imagePath;
                var fileResult = PhysicalFile(absolutePath, "image/jpeg", "no_signal.jpg");
                logger.LogInformation($"{DateTime.Now} GET: api/movie/{id}/photo > no_signal.jpg");
                return fileResult;
            }
            else
            {
                string absolutePath = curDir + "/wwwroot" + imagePath;
                var fileResult = PhysicalFile(absolutePath, "image/jpeg", $"{movie.Name}.jpg");
                logger.LogInformation($"{DateTime.Now} GET: api/movie/{id}/photo > {movie.Name}.jpg");
                return fileResult;
            }
             
        }

        // PUT: api/Movie/5
        [HttpPut("{id}")]
        public ActionResult<Movie> PutMovie(int id, Movie movie)
        {
            if (id != movie.Id)
                return BadRequest();

            bool movieNameValid = CheckMovie(movie.Name, movie.Id);
            if (!movieNameValid)
                return BadRequest("This movie name already in use");
            else
            {
                movie.Name = movie.Name.Trim();
                movie.GenreId = movie.GenreId == 0 ? null : movie.GenreId;
                repo.Update(movie);
                logger.LogInformation($"{DateTime.Now} PUT: api/movie/{id} > {movie}");
                return movie;
            }
        }

        // POST: api/Movie
        [HttpPost]
        public ActionResult<Movie> PostMovie(Movie movie)
        {
            bool movieNameValid = CheckMovie(movie.Name, movie.Id);
            if (!movieNameValid)
                return BadRequest("This movie name already in use");
            else
            {
                movie.GenreId = movie.GenreId == 0 ? null : movie.GenreId;
                movie.Name = movie.Name.Trim();
                int movieId = repo.Add(movie);
                logger.LogInformation($"{DateTime.Now} POST: api/movie > {movie}");
                return CreatedAtAction("GetMovie", new { id = movieId }, movie);
            }
        }

        // DELETE: api/Movie/5
        [HttpDelete("{id}")]
        public ActionResult<Movie> DeleteMovie(int id)
        {
            if (id <= 0)
                return BadRequest();

            var movie = repo.FindMovie(id);
            if (movie == null)
                return NotFound();

            repo.Remove(movie);
            logger.LogInformation($"{DateTime.Now} DELETE: api/movie/{id} > {movie}");
            return movie;
        }

        private bool MovieExists(int id)
        {
            return repo.Movies.Any(a => a.Id == id);
        }

        private bool CheckMovie(string movieName, int id)
        {
            if (movieName == null || movieName.Equals(""))
                return false;

            var movie = repo.Movies
                            .FirstOrDefault(m => m.Name.Equals(movieName.Trim()));
            if (movie == null || (movie != null && movie.Id == id))
                return true;
            else
                return false;
        }
    }
}
