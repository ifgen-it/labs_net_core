using ConsoleClient.Clients;
using ConsoleClient.Entities;
using System;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("begin.. Press any key..");
            Console.ReadKey();

            RunAsync().GetAwaiter().GetResult();

            Console.WriteLine("begin");
            Console.ReadKey();
        }

        static async Task RunAsync()
        {
            ClientActor client = new ClientActor();
            Actor a = await client.GetActorAsync(2);
            Console.WriteLine("Getting from WebApi...");
            Console.WriteLine(a);

            a = new Actor{ FirstName = "Evgen", LastName = "Smirnov" };
            Console.WriteLine("Posting object.. Press any key..");
            Console.ReadKey();

            var a2 = await client.CreateActorAsync(a); 
            Console.WriteLine($"Created: {a2}");

            var b1 = new Actor { Id=a2.Id, FirstName = "Евгений", LastName = "Смирнов" };
            Console.WriteLine("Updating object.. Press any key..");
            Console.ReadKey();

            var b2 = await client.UpdateActorAsync(a2.Id, b1); 
            Console.WriteLine($"Updated: {b2}");

            Console.WriteLine("Deleting object.. Press any key..");
            Console.ReadKey();

            var status = await client.DeleteActorAsync(a2.Id); // будет ошибка
            Console.WriteLine($"Getting status.. {status}");
            Console.ReadKey();


        }
    }
}
