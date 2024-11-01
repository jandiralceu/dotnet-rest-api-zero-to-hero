using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MovieRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO movies (id, slug, title, yearofrelease)
            VALUES (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: token));

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

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);

        var orderClause = options.SortField is not null ? 
            $", m.{options.SortField} ORDER BY m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}" : 
            string.Empty;
        
        var result = await connection.QueryAsync(new CommandDefinition($"""
            SELECT m.*, string_agg(distinct g.name, ',') as genres,
                   round(avg(r.rating), 1) as rating,
                   myr.rating as userrating
            FROM movies m 
            LEFT JOIN genres g on m.id = g.movieid
            LEFT JOIN ratings r ON m.id = r.movieid
            LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
            WHERE (@title IS NULL OR m.title LIKE ('%' || @title || '%')) 
            AND (@yearOfRelease IS NULL OR m.yearofrelease = @yearOfRelease)
            GROUP BY id, userrating {orderClause}
            """, new
            {
                userId = options.UserId,
                title = options.Title,
                yearOfRelease = options.YearOfRelease,
            }, cancellationToken: token));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating 
                FROM movies m
                LEFT JOIN ratings r ON m.id = r.movieid
                LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
                WHERE id = @id
                GROUP BY id, userrating
                """, new { id, userId }, cancellationToken: token));

        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("SELECT name FROM genres WHERE movieid = @id", new { id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating 
                FROM movies m
                LEFT JOIN ratings r ON m.id = r.movieid
                LEFT JOIN ratings myr ON m.id = myr.movieid AND myr.userid = @userId
                WHERE slug = @slug
                GROUP BY id, userrating
                """, new { slug, userId }, cancellationToken: token));

        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("SELECT name FROM genres WHERE movieid = @id", new { id = movie.Id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("DELETE FROM genres WHERE movieid = @id", new { movie.Id }, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO genres (movieId, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE movies
            SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
            WHERE id = @Id
            """, movie, cancellationToken: token));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("DELETE FROM genres WHERE movieid = @id", new { id }, cancellationToken: token));

        var result = await connection.ExecuteAsync(new CommandDefinition("DELETE FROM movies WHERE id = @id", new { id }, cancellationToken: token));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> CheckIfExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("SELECT count(1) FROM movies WHERE id = @id", new { id }, cancellationToken: token));
    }
}
    
    
