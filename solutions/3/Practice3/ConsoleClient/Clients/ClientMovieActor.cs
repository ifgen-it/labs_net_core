using ConsoleClient.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Clients
{
    class ClientMovieActor
    {
        string baseUrl = @"http://localhost:62591/api/";
        string baseUrlMovieActor => baseUrl + "movieactor";

        HttpClient client;

        public ClientMovieActor()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<MovieActor> GetMovieActorAsync(int id)
        {
            string url = baseUrlMovieActor + "/" + id;
            MovieActor movieActor = null;
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                movieActor = await response.Content.ReadAsAsync<MovieActor>();

            return movieActor;
        }
        public async Task<List<MovieActor>> GetAllMovieActorsAsync()
        {
            List<MovieActor> movieActors = null;
            HttpResponseMessage response = await client.GetAsync(baseUrlMovieActor);
            if (response.IsSuccessStatusCode)
                movieActors = await response.Content.ReadAsAsync<List<MovieActor>>();

            return movieActors;
        }
        public async Task<MovieActor> CreateMovieActorAsync(MovieActor movieActor)
        {
            MovieActor resultMovieActor = null;
            HttpResponseMessage response = await client.PostAsJsonAsync(baseUrlMovieActor, movieActor);
            if (response.IsSuccessStatusCode)
                resultMovieActor = await response.Content.ReadAsAsync<MovieActor>();

            return resultMovieActor;
        }
        public async Task<MovieActor> UpdateMovieActorAsync(int id, MovieActor movieActor)
        {
            string url = baseUrlMovieActor + "/" + id;
            MovieActor resultMovieActor = null;
            HttpResponseMessage response = await client.PutAsJsonAsync(url, movieActor);
            if (response.IsSuccessStatusCode)
                resultMovieActor = await response.Content.ReadAsAsync<MovieActor>();

            return resultMovieActor;
        }
        public async Task<MovieActor> DeleteMovieActorAsync(int id)
        {
            string url = baseUrlMovieActor + "/" + id;
            HttpResponseMessage response = await client.DeleteAsync(url);
            MovieActor movieActor = await response.Content.ReadAsAsync<MovieActor>();

            return movieActor;
        }


    }
}
