using WpfClient.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace WpfClient.Clients
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
        public async Task<string> GetActorPhotoAsync(int id)
        {
            string url = baseUrlActor + "/" + id + "/photo";
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

                Directory.CreateDirectory("Downloads\\actors");
                var fileInfo = new FileInfo("Downloads\\actors\\" + filename);
                if (!File.Exists(fileInfo.FullName))
                {
                    await using var ms = await response.Content.ReadAsStreamAsync();
                    await using var fs = File.Create(fileInfo.FullName);
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                    Trace.WriteLine("Actor photo saved as " + fileInfo.FullName);
                    return fileInfo.FullName;
                }
                else
                    return fileInfo.FullName;
            }
            else
                return null;
        }
        public async Task<Actor> GetTopActorAsync()
        {
            string url = baseUrlActor + "/top";
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
