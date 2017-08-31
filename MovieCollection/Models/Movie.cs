using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MovieCollection.Models
{
    public class Movie
    {
        public Movie()
        {
            MovieSequel = 1;
            Click = 0;
            Published = false;
            DateCreated = DateTime.Today;
            DateModified = DateTime.Today;
            Season = 1;
            Episode = 1;
            SpokenLanguage = Language.Ingles;
            SubTitledLanguage = SubTitle.Español;
            QualityReference = Quality.HD720;
            Resolution = "1280x720";
            Published = true;
        }

        [Key]
        public int IdMovie { get; set; }

        [Required]
        [Display(Name = "Título")]
        public string MovieName { get; set; }

        [Display(Name = "Alias")]
        public string MovieAlias { get; set; }

        [Display(Name = "Secuela")]
        public int MovieSequel { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Imagen")]
        public string ImgURL { get; set; }

        public string MegaLink { get; set; }
        public string OuoLink { get; set; }

        [Display(Name = "Idioma")]
        public Language SpokenLanguage { get; set; }

        [Display(Name = "Sub-Títulos")]
        public SubTitle SubTitledLanguage { get; set; }

        [Display(Name = "Calidad")]
        public Quality QualityReference { get; set; }

        [Display(Name = "Resolución")]
        public string Resolution { get; set; }

        [Display(Name = "Categoría")]
        public int IdCategory { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Género")]
        public int IdGenre { get; set; }
        public Genre Genre { get; set; }

        [Display(Name = "Serie")]
        public int? IdSerie { get; set; }
        public Serie Serie { get; set; }

        [Display(Name = "Temporada")]
        public int? Season { get; set; }

        [Display(Name = "Episodio")]
        public int? Episode { get; set; }
       
        public int Click { get; set; }

        [Display(Name = "Publicar?")]
        public bool Published { get; set; }

        [Display(Name = "Fecha Estreno")]
        public DateTime? DatePremiere { get; set; }

        [Display(Name = "Fecha Creación")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Fecha Modificación")]
        public DateTime DateModified { get; set; }

        [Display(Name = "Usuario Creación")]
        public string UserCreated { get; set; }

        [Display(Name = "Fecha Modificación")]
        public string UserModified { get; set; }

    }

    public class Category
    {
        [Key]
        public int IdCategory { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public string CategoryName { get; set; }

        public IEnumerable<Movie> Movies { get; set; }
    }

    public class Genre
    {
        [Key]
        public int IdGenre { get; set; }

        [Required]
        [Display(Name = "Género")]
        public string GenreName { get; set; }

        public IEnumerable<Movie> Movies { get; set; }
    }

    public class Serie
    {
        [Key]
        public int IdSerie { get; set; }

        [Required]
        [Display(Name = "Serie")]
        public string SerieName { get; set; }

        public string OriginalName { get; set; }

        public IEnumerable<Movie> Movies { get; set; }
    }

    public enum Language
    {
        Español,
        Latino,
        Ingles,
        Dual
    }

    public enum SubTitle
    {
        Español,
        Ingles,
        Dual,
        Ninguno
    }

    public enum Quality
    {
        BlueRay,
        HD720,
        Web
    }
}
