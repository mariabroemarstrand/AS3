using Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Linq;


Console.WriteLine("Hello, World!");

var server = new TcpListener(IPAddress.Loopback, 5000);
server.Start();
Console.WriteLine("Server started...");

var data = new Dictionary<int, string>()
{
    {1, "Beverages" },
    {2, "Condiments"},
    {3, "Confections"}
};

Console.WriteLine(data[1]);


while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected...");

    try
    {
        HandleClient(client, data);

    }
    catch (Exception e)
    {
       // Console.WriteLine(e.ToString());    
        Console.WriteLine("Unable to communicate with client...");
    }

}

static void HandleClient(TcpClient client, Dictionary<int, string>? data)
{
    var stream = client.GetStream();
    var buffer = new byte[1024];
    var rcnt = stream.Read(buffer);
    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    Console.WriteLine(requestText);
    var request = JsonSerializer.Deserialize<Request>(requestText);
    var listOfStrings = new List<string>();


       

        if (request.Method == "echo" && !string.IsNullOrEmpty(request.Body))
        {
            Response r = CreateReponse(request.Body);
            SendResponse(stream, r);
        }

        if (request.Date.ToString() == "0")
        {

            listOfStrings.Add("missing date");

        }

        if (string.IsNullOrEmpty(request.Method))
        {

            listOfStrings.Add("4 Missing method");

        }

        else if (request.Method.Equals("create") || request.Method.Equals("update") || request.Method.Equals("delete") || request.Method.Equals("read") || request.Method.Equals("echo") && string.IsNullOrEmpty(request?.Body))
        {

            listOfStrings.Add("missing resource");

                if (request.Method.Equals("echo") || request.Method.Equals("create") || request.Method.Equals("update"))
                {
                    listOfStrings.Add("missing body");
                }

        }

        else
        {

            listOfStrings.Add("illegal method");
        }

        if (request.Date.GetType() != typeof(DateTime))
        {
            listOfStrings.Add("illegal date");
        }

        if (request.Body?.GetType() != typeof(JsonSerializer))
        {
            listOfStrings.Add("illegal body");
        }

    //if (!request.Path.Contains("1") || !request.Path.Contains("2") || !request.Path.Contains("3"))
    //{
    //    Console.WriteLine("YESSSSSS");
    //    listOfStrings.Clear();
    //    listOfStrings.Add("4 Bad Request");
    //    listOfStrings.Add(request.Body);
    //}

    Response response = CreateReponse(String.Join("\n", listOfStrings));
    SendResponse(stream, response);
    stream.Close();
}



static void SendResponse(NetworkStream stream, Response response)
{
    var responseText = JsonSerializer.Serialize<Response>(response);
    var responseBuffer = Encoding.UTF8.GetBytes(responseText);
    stream.Write(responseBuffer);
}

static Response CreateReponse(string status, string body="")
{
    return new Response
    {
        Status = status,
        Body = body
    };
}
