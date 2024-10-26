using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public Task<bool> CreateAsync(Movie movie)
        {
            return _movieRepository.CreateAsync(movie);
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            return _movieRepository.GetAllAsync();
        }

        public Task<Movie?> GetByIdAsync(Guid id)
        {
            return _movieRepository.GetByIdAsync(id);
        }

        public Task<Movie?> GetBySlugAsync(string slug)
        {
            return _movieRepository.GetBySlugAsync(slug);
        }

        public async Task<Movie?> UpdateAsync(Movie movie)
        {
            var exists = await _movieRepository.CheckIfExistsByIdAsync(movie.Id);

            if (!exists) return null;

            await _movieRepository.UpdateAsync(movie);
            return movie;
        }

        public Task<bool> DeleteByIdAsync(Guid id)
        {
            return _movieRepository.DeleteByIdAsync(id);
        }
    }
}