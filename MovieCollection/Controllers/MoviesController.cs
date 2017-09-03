using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieCollection.Data;
using MovieCollection.Models;
using Microsoft.AspNetCore.Authorization;
using MovieCollection.Helpers;

namespace MovieCollection.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Movies
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["GenreSortParm"] = sortOrder == "Genre" ? "genre_desc" : "Genre";
            
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var _movies = _context.Movies.Include(m => m.Category).Include(g => g.Genre)
                           .Include(s => s.Serie).OrderByDescending(d => d.DateCreated);

            if (!String.IsNullOrEmpty(searchString))
            {
                _movies = _movies.Where(s => s.MovieName.Contains(searchString))
                    .OrderByDescending(d => d.DateCreated);
            }

            switch (sortOrder)
            {
                case "title_desc":
                    _movies = _movies.OrderByDescending(s => s.MovieName);
                    break;
                case "Genre":
                    _movies = _movies.OrderBy(s => s.Genre);
                    break;
                case "genre_desc":
                    _movies = _movies.OrderByDescending(s => s.Genre);
                    break;
                default:
                    _movies = _movies.OrderBy(s => s.MovieName);
                    break;
            }

            int pageSize = 7;

            var test = await _movies.AsNoTracking().ToListAsync();
            var test2 = await PaginatedList<Movie>.CreateAsync(_movies.AsNoTracking(), page ?? 1, pageSize);

            return View(await PaginatedList<Movie>.CreateAsync(_movies.AsNoTracking(), page ?? 1, pageSize));
        }



        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(g => g.Genre)
                .Include(s => s.Serie)
                .SingleOrDefaultAsync(m => m.IdMovie == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["IdCategory"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["IdGenre"] = new SelectList(_context.Genres, "IdGenre", "GenreName");
            ViewData["IdSerie"] = new SelectList(_context.Series, "IdSerie", "SerieName");

            Movie _movie = new Movie();

            return View(_movie);
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                movie.UserCreated = User.Identity.Name;
                movie.UserModified = User.Identity.Name;

                movie.OuoLink = string.Format("http://ouo.io/s/mcTIrdpj?s={0}", System.Net.WebUtility.UrlEncode(movie.MegaLink));

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["IdCategory"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", movie.IdCategory);
            ViewData["IdGenre"] = new SelectList(_context.Genres, "IdGenre", "GenreName", movie.IdGenre);
            ViewData["IdSerie"] = new SelectList(_context.Series, "IdSerie", "SerieName", movie.IdSerie);

            return View(movie);
        }

        // GET: Movies/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.SingleOrDefaultAsync(m => m.IdMovie == id);

            if (movie == null)
            {
                return NotFound();
            }

            ViewData["IdCategory"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", movie.IdCategory);
            ViewData["IdGenre"] = new SelectList(_context.Genres, "IdGenre", "GenreName", movie.IdGenre);
            ViewData["IdSerie"] = new SelectList(_context.Series, "IdSerie", "SerieName", movie.IdSerie);

            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.IdMovie)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    movie.DateModified = DateTime.Today;
                    movie.UserModified = User.Identity.Name;

                    movie.OuoLink = string.Format("http://ouo.io/s/mcTIrdpj?s={0}", movie.MegaLink);

                    _context.Update(movie);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.IdMovie))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }

            ViewData["IdCategory"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", movie.IdCategory);
            ViewData["IdGenre"] = new SelectList(_context.Genres, "IdGenre", "GenreName", movie.IdGenre);
            ViewData["IdSerie"] = new SelectList(_context.Series, "IdSerie", "SerieName", movie.IdSerie);

            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Category)
                .SingleOrDefaultAsync(m => m.IdMovie == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.SingleOrDefaultAsync(m => m.IdMovie == id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.IdMovie == id);
        }
    }
}
