using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly List<Movie> _movies = new();
        public Task<bool> CreateAsync(Movie movie)
        {
            _movies.Add(movie);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteByIdAsync(Guid id)
        {
            var removedCount = _movies.RemoveAll(x => x.Id == id);
            return Task.FromResult(removedCount > 0);
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            return Task.FromResult(_movies.AsEnumerable());
        }

        public Task<Movie?> GetByIdAsync(Guid id)
        {
            var movie = _movies.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(movie);
        }

        public Task<Movie?> GetBySlugAsync(string slug)
        {
            var movie = _movies.SingleOrDefault(x => x.Slug == slug);
            return Task.FromResult(movie);
        }

        public Task<bool> UpdateAsync(Movie movie)
        {
            var index = _movies.FindIndex(x => x.Id == movie.Id);
            if (index == -1) return Task.FromResult(false);

            _movies[index] = movie;
            return Task.FromResult(true);
        }
    }
}
