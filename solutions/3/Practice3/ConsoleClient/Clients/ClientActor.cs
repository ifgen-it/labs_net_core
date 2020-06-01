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
    class ClientActor
    {
        string baseUrl = @"http://localhost:62591/api/";
        string baseUrlActor => baseUrl + "actor";

        HttpClient client;

        public ClientActor()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<Actor> GetActorAsync(int id)
        {
            string url = baseUrlActor + "/" + id;
            Actor actor = null;
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                actor = await response.Content.ReadAsAsync<Actor>();

            return actor;
        }
        public async Task<List<Actor>> GetAllActorsAsync()
        {
            List<Actor> actors = null;
            HttpResponseMessage response = await client.GetAsync(baseUrlActor);
            if (response.IsSuccessStatusCode)
                actors = await response.Content.ReadAsAsync<List<Actor>>();

            return actors;
        }
        public async Task<Actor> CreateActorAsync(Actor actor)
        {
            Actor resultActor = null;
            HttpResponseMessage response = await client.PostAsJsonAsync(baseUrlActor, actor);
            if (response.IsSuccessStatusCode)
                resultActor = await response.Content.ReadAsAsync<Actor>();

            return resultActor;
        }
        public async Task<Actor> UpdateActorAsync(int id, Actor actor)
        {
            string url = baseUrlActor + "/" + id;
            Actor resultActor = null;
            HttpResponseMessage response = await client.PutAsJsonAsync(url, actor);
            if (response.IsSuccessStatusCode)
                resultActor = await response.Content.ReadAsAsync<Actor>();

            return resultActor;
        }
        public async Task<Actor> DeleteActorAsync(int id)
        {
            string url = baseUrlActor + "/" + id;
            HttpResponseMessage response = await client.DeleteAsync(url);
            Actor actor = await response.Content.ReadAsAsync<Actor>();

            return actor;
        }


    }
}
