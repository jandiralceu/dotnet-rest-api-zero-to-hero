using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

var services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async (_, __) => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(x =>
        x.BaseAddress = new Uri("http://localhost:5001"));

var provider = services.BuildServiceProvider();
var moviesApi = provider.GetRequiredService<IMoviesApi>();

// Fetch a movie
Console.WriteLine("Fetching a movie...");
var movie = await moviesApi.GetMovieAsync("die-welle-2008");
Console.WriteLine(JsonSerializer.Serialize(movie));
Console.WriteLine();

// Fetch all movies
var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
};
Console.WriteLine("Fetching all movies...");
var movies = await moviesApi.GetMoviesAsync(request);
foreach (var m in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m));
}
Console.WriteLine();

// Create a movie
var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "The Shawshank Redemption",
    YearOfRelease = 2000,
    Genres = ["action", "drama"]
});
Console.WriteLine("Created movie:");
Console.WriteLine(JsonSerializer.Serialize(newMovie));
Console.WriteLine();

// Update a movie
var updatedMovie = await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
    Title = "The Shawshank Redemption",
    YearOfRelease = 1994,
    Genres = ["drama"]
});
Console.WriteLine("Updated movie:");
Console.WriteLine(JsonSerializer.Serialize(updatedMovie));
Console.WriteLine();

// Delete a movie
Console.WriteLine($"Deleting movie {updatedMovie.Title}");
await moviesApi.DeleteMovieAsync(updatedMovie.Id);
Console.WriteLine("Movie deleted.");