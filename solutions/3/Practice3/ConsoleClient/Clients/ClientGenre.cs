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
    class ClientGenre
    {
        string baseUrl = @"http://localhost:62591/api/";
        string baseUrlGenre => baseUrl + "genre";

        HttpClient client;

        public ClientGenre()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<Genre> GetGenreAsync(int id)
        {
            string url = baseUrlGenre + "/" + id;
            Genre genre = null;
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                genre = await response.Content.ReadAsAsync<Genre>();

            return genre;
        }
        public async Task<List<Genre>> GetAllGenresAsync()
        {
            List<Genre> genres = null;
            HttpResponseMessage response = await client.GetAsync(baseUrlGenre);
            if (response.IsSuccessStatusCode)
                genres = await response.Content.ReadAsAsync<List<Genre>>();

            return genres;
        }
        public async Task<Genre> CreateGenreAsync(Genre genre)
        {
            Genre resultGenre = null;
            HttpResponseMessage response = await client.PostAsJsonAsync(baseUrlGenre, genre);
            if (response.IsSuccessStatusCode)
                resultGenre = await response.Content.ReadAsAsync<Genre>();

            return resultGenre;
        }
        public async Task<Genre> UpdateGenreAsync(int id, Genre genre)
        {
            string url = baseUrlGenre + "/" + id;
            Genre resultGenre = null;
            HttpResponseMessage response = await client.PutAsJsonAsync(url, genre);
            if (response.IsSuccessStatusCode)
                resultGenre = await response.Content.ReadAsAsync<Genre>();

            return resultGenre;
        }
        public async Task<Genre> DeleteGenreAsync(int id)
        {
            string url = baseUrlGenre + "/" + id;
            HttpResponseMessage response = await client.DeleteAsync(url);
            Genre genre = await response.Content.ReadAsAsync<Genre>();

            return genre;
        }


    }
}
