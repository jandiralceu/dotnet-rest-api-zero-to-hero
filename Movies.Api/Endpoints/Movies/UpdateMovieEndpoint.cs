using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
            Guid id, UpdateMovieRequest request, IMovieService movieService, HttpContext context, IOutputCacheStore outputCacheStore, CancellationToken cancellationToken) =>
        {
            var movie = request.MapToMovie(id);
            var userId = context.GetUserId();
            var updatedMovie = await movieService.UpdateAsync(movie, userId, cancellationToken);

            if (updatedMovie is null) return Results.NotFound();
        
            await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

            return TypedResults.Ok(movie.MapToResponse());
        }).WithName(Name);
        
        return app;
    }
}