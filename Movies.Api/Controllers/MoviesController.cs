using Microsoft.AspNetCore.Mvc;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        public MoviesController(IMovieRepository movieRepository) 
        { 
            _movieRepository = movieRepository;
        }

        private readonly IMovieRepository _movieRepository;

        [HttpPost("movies")]
        public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
        {
            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                Genres = request.Genres.ToList(),
            };

            await _movieRepository.CreateAsync(movie);

            return Created($"/api/movies/{movie.Id}", movie);
        }
    }
}
