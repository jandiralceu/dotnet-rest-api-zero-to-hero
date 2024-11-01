using System.Text.Json;
using Movies.Api.Sdk;
using Refit;

var moviesApi = RestService.For<IMoviesApi>("http://localhost:5001");

var movie = await moviesApi.GetMoviesAsync("die-welle-2008");
Console.WriteLine(JsonSerializer.Serialize(movie));