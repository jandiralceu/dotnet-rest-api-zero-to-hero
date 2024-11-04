using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    public const string Name = "GetAllMovies";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async ([AsParameters] GetAllMoviesRequest request, IMovieService movieService, HttpContext context , CancellationToken cancellationToken) =>
        {
            var userId = context.GetUserId();
            var options = request.MapToOptions().WithUser(userId);
            var movies = await movieService.GetAllAsync(options, cancellationToken);
            var moviesCount = await movieService.GetCountSync(request.Title, request.Year, cancellationToken);

            var moviesResponse = movies.MapToResponse(
                request.Page.GetValueOrDefault(PaginationRequest.DefaultPage),
                request.PageSize.GetValueOrDefault(PaginationRequest.DefaultPageSize), 
                moviesCount);

            return TypedResults.Ok(moviesResponse);
        }).WithName(Name);

        return app;
    }
}