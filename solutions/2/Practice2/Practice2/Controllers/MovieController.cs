using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Practice2.Domain.Abstract;
using Practice2.Domain.Concrete;
using Practice2.Domain.Entities;

namespace Practice2.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieRepository repo;

        int pageSize = 5;
        public MovieController(IMovieRepository movieRepo)
        {
            repo = movieRepo;
        }

        // GET: Movie
        public IActionResult Index(int? page, string searchText,
                                    bool? nameSort, bool? yearSort,
                                    bool? ratingSort, bool? priceSort,
                                    bool? genreSort)
        {
            int _page = 1;
            if (page != null)
                _page = (int)page;

            // searching
            IQueryable<Movie> needMovies;
            if (searchText == null || searchText.Equals(""))
            {
                needMovies = repo.Movies;
            }
            else
            {
                var token = searchText.Trim().ToLower();
                needMovies = repo.Movies
                        .Where(movie => movie.Name.ToLower().Contains(token));
            }

            int moviesCount = needMovies.Count();
            if (moviesCount == 0)
            {
                ViewBag.CurrentPage = 1;
                ViewBag.PagesCount = 1;
                ViewBag.SearchText = searchText;
                return View(new List<Movie>());
            }
            needMovies = needMovies.Include(m => m.Genre);

            // sorting
            if (nameSort == null && yearSort == null && ratingSort == null && priceSort == null && genreSort == null)
                needMovies = needMovies.OrderBy(m => m.Name);
            else
            {
                if (nameSort != null)
                {
                    if ((bool)nameSort)
                        needMovies = needMovies.OrderBy(m => m.Name);
                    if (!(bool)nameSort)
                        needMovies = needMovies.OrderByDescending(m => m.Name);
                    ViewBag.NameSort = (bool)nameSort;
                }
                else if (yearSort != null)
                {
                    if ((bool)yearSort)
                        needMovies = needMovies.OrderBy(m => m.Year)
                                                .ThenBy(m => m.Name);
                    if (!(bool)yearSort)
                        needMovies = needMovies.OrderByDescending(m => m.Year)
                                                .ThenBy(m => m.Name);
                    ViewBag.YearSort = (bool)yearSort;
                }
                else if (ratingSort != null)
                {
                    if ((bool)ratingSort)
                        needMovies = needMovies.OrderBy(m => m.Rating)
                                                .ThenBy(m => m.Name);
                    if (!(bool)ratingSort)
                        needMovies = needMovies.OrderByDescending(m => m.Rating)
                                                .ThenBy(m => m.Name);
                    ViewBag.RatingSort = (bool)ratingSort;
                }
                else if (priceSort != null)
                {
                    if ((bool)priceSort)
                        needMovies = needMovies.OrderBy(m => m.Price)
                                                .ThenBy(m => m.Name);
                    if (!(bool)priceSort)
                        needMovies = needMovies.OrderByDescending(m => m.Price)
                                                .ThenBy(m => m.Name);
                    ViewBag.PriceSort = (bool)priceSort;
                }
                else if (genreSort != null)
                {
                    if ((bool)genreSort)
                        needMovies = needMovies.OrderBy(m => m.Genre.Name)
                                                .ThenBy(m => m.Name);
                    if (!(bool)genreSort)
                        needMovies = needMovies.OrderByDescending(m => m.Genre.Name)
                                                .ThenBy(m => m.Name);
                    ViewBag.GenreSort = (bool)genreSort;
                }
            }

            // pagination
            int pagesCount = (int)Math.Ceiling((double)moviesCount / pageSize);
            if (_page > pagesCount || _page < 1)
                _page = 1;

            var movies = needMovies
                    .Skip((_page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            ViewBag.CurrentPage = _page;
            ViewBag.PagesCount = pagesCount;
            ViewBag.SearchText = searchText;

            return View(movies);
        }

        // GET: Movie/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = repo.Movies
                .Include(m => m.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // GET: Movie/Create
        public IActionResult Create()
        {
            var genres = repo.Genres.ToList();
            genres.Add(new Genre { Id = 0, Name = "" });
            genres.Sort((x, y) => x.Name.CompareTo(y.Name));
            ViewBag.Genres = new SelectList(genres, "Id", "Name", 0);
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Year,Rating,Price,GenreId")] Movie movie)
        {
            bool baseValid = ModelState.IsValid;
            bool movieNameValid = CheckMovie(movie.Name, movie.Id);
            if (!movieNameValid)
            {
                ModelState.AddModelError("Name", "This name already in use");
            }
            if (baseValid && movieNameValid)
            {
                movie.GenreId = movie.GenreId == 0 ? null : movie.GenreId;
                movie.Name = movie.Name.Trim();
                int movieId = repo.Add(movie);
                return RedirectToAction(nameof(Details), new { id = movieId });
            }
            var genres = repo.Genres.ToList();
            genres.Add(new Genre { Id = 0, Name = "" });
            genres.Sort((x, y) => x.Name.CompareTo(y.Name));
            ViewBag.Genres = new SelectList(genres, "Id", "Name", 0);
            return View();
        }

        // GET: Movie/EditActors/5
        public IActionResult EditActors(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = repo.Movies
                        .Include(m => m.MovieActors)
                            .ThenInclude(ma => ma.Actor)
                        .FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            ViewBag.Movie = movie;
            var actorsFromMovie = repo.ActorsFromMovie((int)id);
            ViewBag.Actors = actorsFromMovie.ToList();
            ViewBag.NewActors = repo.Actors.Except(actorsFromMovie).ToList();

            return View();
        }

        // POST: Movie/EditActors/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditActors(int movieId, int actorId)
        {
            repo.AddActorToMovie(actorId, movieId);
            return RedirectToAction(nameof(EditActors), new { id = movieId });
        }

        // GET: Movie/EditActorRemove
        public IActionResult EditActorRemove(int movieId, int actorId)
        {
            repo.RemoveActorFromMovie(actorId, movieId);
            return RedirectToAction(nameof(EditActors), new { id = movieId });
        }

        // GET: Movie/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = repo.FindMovie((int)id);
            if (movie == null)
                return NotFound();

            var genres = repo.Genres.ToList();
            genres.Add(new Genre { Id = 0, Name = "" });
            genres.Sort((x, y) => x.Name.CompareTo(y.Name));
            ViewBag.Genres = new SelectList(genres, "Id", "Name", 0);
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Year,Rating,Price,GenreId")] Movie movie)
        {
            if (id != movie.Id)
                return NotFound();

            bool baseValid = ModelState.IsValid;
            bool movieNameValid = CheckMovie(movie.Name, movie.Id);
            if (!movieNameValid)
            {
                ModelState.AddModelError("Name", "This name already in use");
            }
            if (baseValid && movieNameValid)
            {
                movie.Name = movie.Name.Trim();

                // update foto
                var oldMovie = repo.FindMovie(movie.Id);
                string oldFileName = oldMovie.Name;
                string newFileName = movie.Name;

                var curDir = Directory.GetCurrentDirectory();
                var oldImagePath = curDir + "/wwwroot" + $"/images/movies/{oldFileName}.jpg";
                var newImagePath = curDir + "/wwwroot" + $"/images/movies/{newFileName}.jpg";

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Move(oldImagePath, newImagePath);
                }

                // update movie
                movie.GenreId = movie.GenreId == 0 ? null : movie.GenreId;
                repo.Update(movie);
                return RedirectToAction(nameof(Details), new { id = id });
            }
            var genres = repo.Genres.ToList();
            ViewBag.Genres = new SelectList(genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // GET: Movie/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = repo.Movies.Include(m => m.Genre)
                .FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var movie = repo.FindMovie(id);
            repo.Remove(movie);
            return RedirectToAction(nameof(Index));
        }

        // GET: Movie/Search/5
        public IActionResult Search(string searchText)
        {
            return RedirectToAction(nameof(Index), new { page = 1, searchText = searchText });
        }

        // POST: Actor/AddFoto
        [HttpPost]
        public IActionResult AddFoto(int id, IFormFile uploadedFile)
        {
            if (uploadedFile == null)
            {
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            var movie = repo.FindMovie(id);
            string fileName = movie.Name;
            var relImagePath = $"/images/movies/{fileName}.jpg";
            var curDir = Directory.GetCurrentDirectory();
            var imagePath = curDir + "/wwwroot" + relImagePath;

            string userFileName = uploadedFile.FileName;
            var tokens = userFileName.Split(new char[] { '.' });
            var extension = tokens[tokens.Length - 1];

            if (extension.Equals("jpg") && uploadedFile.Length <= (1024 * 1024 * 5))
            {
                using (var fs = new FileStream(imagePath, FileMode.Create))
                {
                    uploadedFile.CopyTo(fs);
                }
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Actor/DeleteFoto
        [HttpGet]
        public IActionResult DeleteFoto(int id)
        {
            var movie = repo.FindMovie(id);
            string fileName = movie.Name;
            var relImagePath = $"/images/movies/{fileName}.jpg";
            var curDir = Directory.GetCurrentDirectory();
            var imagePath = curDir + "/wwwroot" + relImagePath;

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public bool CheckMovie(string movieName, int id)
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
