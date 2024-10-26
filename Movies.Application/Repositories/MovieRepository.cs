using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;   

        public MovieRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateAsync(Movie movie)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO movies (id, slug, title, yearofrelease)
                VALUES (@Id, @Slug, @Title, @YearOfRelease)
                """, movie));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT INTO genres (movieId, name)
                        VALUES (@MovieId, @Name)
                        """, new { MovieId = movie.Id, Name = genre }));
                }
            }
            transaction.Commit();

            return result > 0;
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.QueryAsync(new CommandDefinition("""
                SELECT m.*, string_agg(g.name, ',') as genres
                FROM movies m LEFT JOIN genres g on m.id = g.movieid
                GROUP BY id
                """));

            return result.Select(x => new Movie
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Genres = Enumerable.ToList(x.genres.Split(','))
            });
        }

        public async Task<Movie?> GetByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
                new CommandDefinition("""
                    SELECT * FROM movies WHERE id = @id
                    """, new { id }));

            if (movie is null) return null;

            var genres = await connection.QueryAsync<string>(
                new CommandDefinition("""
                    SELECT name FROM genres WHERE movieid = @id
                    """, new { id }));

            foreach(var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<Movie?> GetBySlugAsync(string slug)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
                new CommandDefinition("""
                    SELECT * FROM movies WHERE slug = @slug
                    """, new { slug }));

            if (movie is null) return null;

            var genres = await connection.QueryAsync<string>(
                new CommandDefinition("""
                    SELECT name FROM genres WHERE movieid = @id
                    """, new { id = movie.Id }));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieid = @id
                """, new { movie.Id }));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO genres (movieId, name)
                    VALUES (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE movies
                SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                WHERE id = @Id
                """, movie));
            
            transaction.Commit();

            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM genres WHERE movieid = @id
                """, new { id }));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM movies WHERE id = @id
                """, new { id }));

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> CheckIfExistsByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT count(1) FROM movies WHERE id = @id
                """, new { id }));
        }
    }
}
