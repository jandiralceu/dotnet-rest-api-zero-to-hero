using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IMovieRepository
    {
        Task<bool> CreateAsync(Movie movie, CancellationToken token = default);
        Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default);
        Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default);
        Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default, Guid? userId = default);
        Task<bool> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<bool> CheckIfExistsByIdAsync(Guid id, CancellationToken token = default);
    }
}
