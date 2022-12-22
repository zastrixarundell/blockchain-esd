using SmartContract;
using System.Threading.Channels;

class Client
{
    static int Main()
    {
        string input;
        do
        {
            string id =  IdInput();
            string data = DataInput();

            // var user = new User { Id = id, Data = data, Timestamp = DateTime.Now };

            Console.Write("Do you want to conitnue? Y/N: ");

            input = Console.ReadLine().ToLower();
        } while(input == "y");
        return 0;
    }

    

    private static string DataInput()
    {
        return UserInput(
            7,
            "Plese type in the information which should be sent to the server with at least 7 characters",
            "The data was not formatted correctly. Try again."
        );
    }

    private static string IdInput()
    {
        return UserInput(
            5,
            "Please put in an unique user ID with at least 5 characters",
            "The userID was not formatted correctly. Try again."
        );
    }

    private static string UserInput(int length, string consoleMessage, string consoleError)
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
}