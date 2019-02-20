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
using System.IO;
using System.Net;
using SixLabors.ImageSharp;
using System.Net.Http;
using System.Diagnostics;

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
            ViewData["DatePremiereSortParm"] = sortOrder == "DatePremiere" ? "datepremiere_desc" : "DatePremiere";
            ViewData["DateCreatedSortParm"] = sortOrder == "DateCreated" ? "datecreated_desc" : "DateCreated";

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
                _movies = _movies.Where(s => s.MovieName.Contains(searchString) || s.MovieAlias.Contains(searchString) || s.Serie.SerieName.Contains(searchString) || s.Serie.OriginalName.Contains(searchString))
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
                case "DatePremiere":
                    _movies = _movies.OrderBy(s => s.DatePremiere);
                    break;

                case "datepremiere_desc":
                    _movies = _movies.OrderByDescending(s => s.DatePremiere);
                    break;

                case "DateCreated":
                    _movies = _movies.OrderBy(s => s.DateCreated);
                    break;

                case "datecreated_desc":
                    _movies = _movies.OrderByDescending(s => s.DateCreated);
                    break;
                default:
                    _movies = _movies.OrderBy(s => s.MovieName).OrderByDescending(s => s.DateCreated);
                    break;
            }

            int pageSize = 7;

            //var test = await _movies.AsNoTracking().ToListAsync();
            //var test2 = await PaginatedList<Movie>.CreateAsync(_movies.AsNoTracking(), page ?? 1, pageSize);

            return View(await PaginatedList<Movie>.CreateAsync(_movies.AsNoTracking(), page ?? 1, pageSize));
        }


        public IActionResult Series()
        {

            return View();
        }

        [HttpGet]
        public JsonResult GetMovies()
        {
            var model = _context.Movies.OrderByDescending(d => d.DateCreated); ;


            //if (!string.IsNullOrEmpty(categoryName))
            //{
            //    var genreId = _context.Genres.Where(x => x.GenreName == categoryName).Select(x=>x.IdGenre).FirstOrDefault();

            //    model.Where(x => x.IdGenre == genreId);
            //}

            return Json(model.AsNoTracking().ToList());
        }

        [HttpGet]
        public JsonResult GetGenreId(string categoryName)
        {

            var genreId = _context.Genres.Where(x => x.GenreName == categoryName).FirstOrDefault().IdGenre.ToString();

            return Json(genreId);

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
            try
            {
                ViewData["IdCategory"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", movie.IdCategory);
                ViewData["IdGenre"] = new SelectList(_context.Genres, "IdGenre", "GenreName", movie.IdGenre);
                ViewData["IdSerie"] = new SelectList(_context.Series, "IdSerie", "SerieName", movie.IdSerie);

                if (ModelState.IsValid)
                {

                    if (movie.IdCategory == 2 && movie.IdSerie == null)
                    {
                        ViewData["ErrorURL"] = string.Format("Para la Categoría SERIES debe seleccionar una de la lista de SERIES o CREAR una nueva.");
                        return View(movie);
                    }

                    if (!IsValidUri(movie.ImgURL))
                    {
                        ViewData["ErrorURL"] = string.Format("Imagen: {0} es una URL inválida", movie.ImgURL);
                        return View(movie);
                    }

                    if (!await IsImageResolutionOK(movie.ImgURL))
                    {
                        ViewData["ErrorURL"] = string.Format("La calidad de la imágen deben ser mínimo de 600x400 o no es una extensión válida.");
                        return View(movie);
                    }


                    if (!IsValidUri(movie.MegaLink))
                    {
                        ViewData["ErrorURL"] += string.Format("Mega: {0} es una URL inválida", movie.MegaLink);

                        return View(movie);
                    }

                    movie.UserCreated = User.Identity.Name;
                    movie.UserModified = User.Identity.Name;

                    movie.OuoLink = string.Format("http://ouo.io/s/mcTIrdpj?s={0}", WebUtility.UrlEncode(movie.MegaLink));

                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ViewData["ErrorURL"] += string.Format("Error: {0}.", ex.Message);
            }


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
                    if (!IsValidUri(movie.ImgURL))
                    {
                        ViewData["ErrorURL"] = String.Format("Imagen: {0} es una URL inválida", movie.ImgURL);
                        return View(movie);
                    }

                    if (!IsValidUri(movie.MegaLink))
                    {
                        ViewData["ErrorURL"] += String.Format("Mega: {0} es una URL inválida", movie.MegaLink);

                        return View(movie);
                    }

                    movie.DateModified = DateTime.Today;
                    movie.UserModified = User.Identity.Name;



                    movie.OuoLink = string.Format("http://ouo.io/s/mcTIrdpj?s={0}", System.Net.WebUtility.UrlEncode(movie.MegaLink));

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

        public bool IsValidUri(string uri)
        {
            Uri validatedUri;
            return Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out validatedUri);
        }

        public async Task<bool> IsImageResolutionOK(string imgUrl)
        {
            try
            {
                using (HttpClient c = new HttpClient())
                {
                    using (Stream s = await c.GetStreamAsync(imgUrl))
                    {
                        using (Image<Rgba32> image = Image.Load(s))
                        {
                            if (image.Width >= 600 && image.Height >= 400)
                            {
                                return true;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;

        }
    }
}
