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
    public class ActorController : Controller
    {
        private readonly IMovieRepository repo;

        int pageSize = 5;

        public ActorController(IMovieRepository movieRepo)
        {
            repo = movieRepo;
        }

        // GET: Actor
        public IActionResult Index(int? page, string searchText, bool? firstNameSort, bool? lastNameSort)
        {
            int _page = 1;
            if (page != null)
                _page = (int)page;

            // searching
            IQueryable<Actor> needActors;
            if (searchText == null || searchText.Equals(""))
            {
                needActors = repo.Actors;
            }
            else
            {
                var token = searchText.Trim().ToLower();

                needActors = repo.Actors.Where(actor =>
                actor.FirstName.ToLower().Contains(token)
               || actor.LastName.ToLower().Contains(token)
               || (actor.FirstName + " " + actor.LastName).ToLower().Contains(token)
               || (actor.LastName + " " + actor.FirstName).ToLower().Contains(token));
            }

            int actorsCount = needActors.Count();
            if (actorsCount == 0)
            {
                ViewBag.CurrentPage = 1;
                ViewBag.PagesCount = 1;
                ViewBag.SearchText = searchText;
                return View(new List<Actor>());
            }

            // sorting
            if (firstNameSort == null && lastNameSort == null)
                needActors = needActors.OrderBy(a => a.LastName);
            else
            {
                if (firstNameSort != null)
                {
                    if ((bool)firstNameSort)
                        needActors = needActors.OrderBy(a => a.FirstName);
                    if (!(bool)firstNameSort)
                        needActors = needActors.OrderByDescending(a => a.FirstName);
                    ViewBag.FirstNameSort = (bool)firstNameSort;
                }
                else if (lastNameSort != null)
                {
                    if ((bool)lastNameSort)
                        needActors = needActors.OrderBy(a => a.LastName);
                    if (!(bool)lastNameSort)
                        needActors = needActors.OrderByDescending(a => a.LastName);
                    ViewBag.LastNameSort = (bool)lastNameSort;
                }
            }

            // pagination
            int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
            if (_page > pagesCount || _page < 1)
                _page = 1;

            var actors = needActors
                    .Skip((_page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            ViewBag.CurrentPage = _page;
            ViewBag.PagesCount = pagesCount;
            ViewBag.SearchText = searchText;

            return View(actors);
        }

        // GET: Actor/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = repo.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,FirstName,LastName")] Actor actor)
        {
            bool baseValid = ModelState.IsValid;
            bool actorNameValid = CheckActor(actor.FirstName, actor.LastName, actor.Id);
            if (!actorNameValid)
            {
                ModelState.AddModelError("", "This actor name already in use");
            }
            if (baseValid && actorNameValid)
            {
                actor.FirstName = actor.FirstName.Trim();
                actor.LastName = actor.LastName.Trim();
                int actorId = repo.Add(actor);
                return RedirectToAction(nameof(Details), new { id = actorId });
            }
            return View(actor);
        }

        // GET: Actor/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = repo.FindActor((int)id);
            if (actor == null)
                return NotFound();

            return View(actor);
        }

        // POST: Actor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,FirstName,LastName")] Actor actor)
        {
            if (id != actor.Id)
                return NotFound();

            bool baseValid = ModelState.IsValid;
            bool actorNameValid = CheckActor(actor.FirstName, actor.LastName, actor.Id);
            if (!actorNameValid)
            {
                ModelState.AddModelError("", "This actor name already in use");
            }
            if (baseValid && actorNameValid)
            {
                actor.FirstName = actor.FirstName.Trim();
                actor.LastName = actor.LastName.Trim();

                // update foto
                var oldActor = repo.FindActor(actor.Id);
                string oldFileName = oldActor.FirstName + " " + oldActor.LastName;
                string newFileName = actor.FirstName + " " + actor.LastName;

                var curDir = Directory.GetCurrentDirectory();
                var oldImagePath = curDir + "/wwwroot" + $"/images/actors/{oldFileName}.jpg";
                var newImagePath = curDir + "/wwwroot" + $"/images/actors/{newFileName}.jpg";

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Move(oldImagePath, newImagePath);
                }

                // update actor
                repo.Update(actor);
                return RedirectToAction(nameof(Details), new { id = id });
            }
            return View(actor);
        }

        // GET: Actor/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = repo.FindActor((int)id);
            if (actor == null)
                return NotFound();

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var actor = repo.FindActor(id);
            repo.Remove(actor);
            return RedirectToAction(nameof(Index));
        }

        // GET: Actor/Search/5
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

            var actor = repo.FindActor(id);
            string fileName = actor.FirstName + " " + actor.LastName;
            var relImagePath = $"/images/actors/{fileName}.jpg";
            var curDir = Directory.GetCurrentDirectory();
            var imagePath = curDir + "/wwwroot" + relImagePath;

            string userFileName = uploadedFile.FileName;
            var tokens = userFileName.Split(new char[] { '.' });
            var extension = tokens[tokens.Length - 1];
            
            if (extension.Equals("jpg") && uploadedFile.Length <= (1024*1024*2))
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
            var actor = repo.FindActor(id);
            string fileName = actor.FirstName + " " + actor.LastName;
            var relImagePath = $"/images/actors/{fileName}.jpg";
            var curDir = Directory.GetCurrentDirectory();
            var imagePath = curDir + "/wwwroot" + relImagePath;

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public bool CheckActor(string firstName, string lastName, int id)
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
