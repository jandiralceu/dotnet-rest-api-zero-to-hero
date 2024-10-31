using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public RatingRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ratings (userid, movieid, rating)
            VALUES(@userID, @movieId, @rating)
            ON CONFLICT (userid, movieid) DO UPDATE 
            SET rating = @rating
            """, new { userId, movieId, rating }, cancellationToken: token));
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            SELECT round(avg(r.rating), 1) FROM ratings r
            WHERE movieid = @movieId
            """, new { movieId }, cancellationToken: token));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync< (float?, int?)>(new CommandDefinition("""
            SELECT round(avg(rating), 1), 
                   (SELECT rating FROM ratings 
                   WHERE movieid = @movieId AND userid = @userId
                   LIMIT 1) 
            FROM ratings
            WHERE movieid = @movieId
            """, new { movieId, userId }, cancellationToken: token));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        var result = await connection.ExecuteAsync(new CommandDefinition($"""
                  DELETE FROM ratings
                  WHERE movieid = @movieId AND userid = @userId
                  """, new { userId, movieId }, cancellationToken: token));
        
        return result > 0;
    }
}