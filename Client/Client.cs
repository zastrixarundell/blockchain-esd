using SmartContract;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Net.NetworkInformation;
using System;
using System.Threading.Tasks;

public class Client
{
    static int Main()
    {
        int minerCount = getMinerCount();

        if (minerCount > 0)
            Console.WriteLine("There are " + minerCount + " miners connected.");
        else
            Console.WriteLine("There are no miners connected.");

        string input;
        do
        {
            string id =  IdInput();
            string data = DataInput();

            var user = new User { Id = id, Data = data, Timestamp = DateTime.Now };

            postData(user);

            Console.Write("Do you want to conitnue? Y/N: ");

            input = Console.ReadLine().ToLower();
        } while(input == "y");
        return 0;
    }
    
    public static string DataInput()
    {
        return UserInput(
            7,
            "Plese type in the information which should be sent to the server with at least 7 characters",
            "The data was not formatted correctly. Try again."
        );
    }

    public static string IdInput()
    {
        return UserInput(
            5,
            "Please put in an unique user ID with at least 5 characters",
            "The userID was not formatted correctly. Try again."
        );
    }

    public static string UserInput(int length, string consoleMessage, string consoleError)
    {
        string input;
        
        var error = false;
        do
        {
            if (error)
                Console.WriteLine(consoleError);

            Console.Write(consoleMessage + ": ");

            input = Console.ReadLine();
        } while (
            (input == null || input.Length < length) && (error = true)
        );

        return input;
    }

    public static bool postData(User user)
    {
        HttpClient webClient = new HttpClient();

        try
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var httpResponse = webClient.PostAsync("http://localhost:5067/user", httpContent).Result;

            Console.WriteLine("Successfully added to the queue!");

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return false;
    }

    public static int getMinerCount()
    {
        var response = SendRequest("http://localhost:5067/miners", "get", null).Result;

        Console.WriteLine(response.Content);

        return 0;
    }

    private static async Task<HttpResponseMessage> SendRequest(string url, string type, string? body)
    {
        var client = new HttpClient();

        switch (type.ToLower())
        {
            case "get":
                return await client.GetAsync(url);
            default:
                return await client.PostAsync(url, null);
        }
    }
}