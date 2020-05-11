using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Practice2.Domain.Abstract;
using Practice2.Domain.Concrete;
using Practice2.Domain.Entities;

namespace Practice2.Controllers
{
    public class GenreController : Controller
    {
        private readonly IMovieRepository repo;

        public GenreController(IMovieRepository movieRepo)
        {
            repo = movieRepo;
        }

        // GET: Genre
        public IActionResult Index()
        {
            var genres = repo.Genres
                .OrderBy(g => g.Name)
                .ToList();
            return View(genres);
        }

        // GET: Genre/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Genre/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name")] Genre genre)
        {
            bool baseValid = ModelState.IsValid;
            bool genreNameValid = CheckGenre(genre.Name, genre.Id);
            if (!genreNameValid)
            {
                ModelState.AddModelError("Name", "This name already in use");
            }
            if (baseValid && genreNameValid)
            {
                genre.Name = genre.Name.Trim();
                repo.Add(genre);
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // GET: Genre/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var genre = repo.FindGenre((int)id);
            if (genre == null)
                return NotFound();

            return View(genre);
        }

        // POST: Genre/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name")] Genre genre)
        {
            if (id != genre.Id)
                return NotFound();

            bool baseValid = ModelState.IsValid;
            bool genreNameValid = CheckGenre(genre.Name, genre.Id);
            if (!genreNameValid)
            {
                ModelState.AddModelError("Name", "This name already in use");
            }
            if (baseValid && genreNameValid)
            {
                genre.Name = genre.Name.Trim();
                repo.Update(genre);
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // GET: Genre/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var genre = repo.FindGenre((int)id);
            if (genre == null)
                return NotFound();

            return View(genre);
        }

        // POST: Genre/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var genre = repo.FindGenre(id);
            repo.Remove(genre);
            return RedirectToAction(nameof(Index));
        }

        public bool CheckGenre(string genreName, int id)
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
