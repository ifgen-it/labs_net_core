using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Practice2.Domain.Abstract;
using Practice2.Models;

namespace Practice2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IMovieRepository repo;

        public HomeController(ILogger<HomeController> logger, IMovieRepository movieRepo)
        {
            this.logger = logger;
            repo = movieRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Statistics()
        {
            ViewBag.MoviesCount = repo.Movies.Count();
            ViewBag.ActorsCount = repo.Actors.Count();

            var moviesActorsCount = repo.Movies
                            .Include(m => m.MovieActors)
                            .Select(m => m.MovieActors.Count)
                            .ToList();
            var avgActors = moviesActorsCount.Average();
            ViewBag.AvgActors = string.Format($"{avgActors:f2}");

            var topMovie = repo.Movies
                            .OrderByDescending(m => m.Rating)
                            .FirstOrDefault();
            ViewBag.TopMovie = topMovie;

            var topActor = repo.Actors
                            .Include(a => a.MovieActors)
                            .OrderByDescending(a => a.MovieActors.Count)
                            .FirstOrDefault();
            ViewBag.TopActor = topActor;

            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
