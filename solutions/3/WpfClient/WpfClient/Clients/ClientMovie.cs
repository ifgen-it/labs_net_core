using WpfClient.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace WpfClient.Clients
{
    class ClientMovie
    {
        string baseUrl = @"http://localhost:62591/api/";
        string baseUrlMovie => baseUrl + "movie";

        HttpClient client;

        public ClientMovie()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<Movie> GetMovieAsync(int id)
        {
            string url = baseUrlMovie + "/" + id;
            Movie movie = null;
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                movie = await response.Content.ReadAsAsync<Movie>();

            return movie;
        }
        public async Task<string> GetMoviePhotoAsync(int id)
        {
            string url = baseUrlMovie + "/" + id + "/photo";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var headers = response.Content.Headers;
                var en = headers.GetEnumerator();

                string filename = "filename";
                while (en.MoveNext())
                {
                    var header = en.Current;
                    if (header.Key.Equals("Content-Disposition"))
                    {
                        foreach (var item in header.Value)
                        {
                            var tokens = item.Split(";", StringSplitOptions.RemoveEmptyEntries);
                            foreach (var token in tokens)
                            {
                                if (token.Trim().StartsWith("filename="))
                                {
                                    filename = token
                                        .Split("=", StringSplitOptions.RemoveEmptyEntries)[1]
                                        .Trim('"');
                                }
                            }
                        }
                        break;
                    }
                }

                Directory.CreateDirectory("Downloads\\movies");
                var fileInfo = new FileInfo("Downloads\\movies\\" + filename);
                if (!File.Exists(fileInfo.FullName))
                {
                    await using var ms = await response.Content.ReadAsStreamAsync();
                    await using var fs = File.Create(fileInfo.FullName);
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                    Trace.WriteLine("Movie photo saved as " + fileInfo.FullName);
                    return fileInfo.FullName;
                }
                else
                    return fileInfo.FullName;
            }
            else
                return null;
        }
        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            List<Movie> movies = null;
            HttpResponseMessage response = await client.GetAsync(baseUrlMovie);
            if (response.IsSuccessStatusCode)
                movies = await response.Content.ReadAsAsync<List<Movie>>();
            return movies;
        }
        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            Movie resultMovie = null;
            HttpResponseMessage response = await client.PostAsJsonAsync(baseUrlMovie, movie);
            if (response.IsSuccessStatusCode)
                resultMovie = await response.Content.ReadAsAsync<Movie>();

            return resultMovie;
        }
        public async Task<Movie> UpdateMovieAsync(int id, Movie movie)
        {
            string url = baseUrlMovie + "/" + id;
            Movie resultMovie = null;
            HttpResponseMessage response = await client.PutAsJsonAsync(url, movie);
            if (response.IsSuccessStatusCode)
                resultMovie = await response.Content.ReadAsAsync<Movie>();

            return resultMovie;
        }
        public async Task<Movie> DeleteMovieAsync(int id)
        {
            string url = baseUrlMovie + "/" + id;
            HttpResponseMessage response = await client.DeleteAsync(url);
            Movie movie = await response.Content.ReadAsAsync<Movie>();

            return movie;
        }


    }
}
