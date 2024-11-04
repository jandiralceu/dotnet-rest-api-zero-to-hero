using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";

    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
            Guid id, IMovieService movieService, IOutputCacheStore outputCacheStore, CancellationToken cancellationToken) =>
        {
            var deleted = await movieService.DeleteByIdAsync(id, cancellationToken);
            if (!deleted) return Results.NotFound();
        
            await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

            return TypedResults.Ok();
        }).WithName(Name);
        
        return app;
    }
}