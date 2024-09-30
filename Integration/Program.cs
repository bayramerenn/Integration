using Integration.Common;
using Integration.Service;
using System.Net.Http.Json;

namespace Integration;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        await Task.Run(() =>
        {
            ProcessSingleServerOperation();
        });

        //await ProcessDisributedServerOperation();
    }

    public static void ProcessSingleServerOperation()
    {
        var service = new ItemIntegrationService();

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));

        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));

        Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }

    public static async Task ProcessDisributedServerOperation()
    {
        string apiUrl = "http://localhost:7000/Integrations";

        int requestCount = 20;

        using (HttpClient client = new HttpClient())
        {
            Task[] tasks = new Task[requestCount];

            for (int i = 0; i < requestCount; i++)
            {
                tasks[i] = MakeRequest(client, apiUrl);
            }
            await Task.WhenAll(tasks);

            Console.WriteLine("All request completed.");
        }
    }

    private static async Task MakeRequest(HttpClient client, string apiUrl)
    {
        try
        {
            char randomChar = GetRandomChar();

            HttpResponseMessage response = await client.PostAsJsonAsync(apiUrl, new ItemDto { Content = randomChar.ToString() });

            // Yanıtı işle
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Successful request. Content : {content}");
            }
            else
            {
                Console.WriteLine($"Error status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured: {ex.Message}");
        }
    }

    private static char GetRandomChar()
    {
        string characters = "abcde";
        Random random = new Random();
        int index = random.Next(characters.Length);
        return characters[index];
    }
}