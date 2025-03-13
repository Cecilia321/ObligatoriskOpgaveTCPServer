using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ObligatoriskOpgaveTCPServer.Servere
{
    class JsonServer
    {
        private const int PORTNUMMER = 8888;

        public void Start()
        {
            TcpListener server = new TcpListener(PORTNUMMER);
            server.Start();

            //gør at kan håndtere mere end en forbindelse 
            while (true)
            {
                TcpClient socket = server.AcceptTcpClient();
                Task.Run(() =>
                {
                    TcpClient TempSocket = socket;
                    DoOneClient(socket);
                });

            };
        
        }

        public static void DoOneClient(TcpClient socket)
        {
            StreamReader reader = new StreamReader(socket.GetStream());

            StreamWriter writer = new StreamWriter(socket.GetStream());
            writer.AutoFlush = true; //den flusher automatisk. 

            try
            {
                while (true)
                {
                    //Læs Json fra klienten 
                    string jsonInput = reader.ReadLine();
                    if (string.IsNullOrEmpty(jsonInput)) break;

                    Console.WriteLine($"modtaget JSON {jsonInput}");

                    //behandler json og sender svar 
                    string responseJson = RequestHandler(jsonInput);
                    writer.WriteLine(responseJson);
                    Console.WriteLine($"Sendt svar: {responseJson}");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"fejl: {ex.Message}");
            }
            finally
            {
                socket.Close();
            }
        }
        public static string RequestHandler(string jsonInput)
        {
            try
            {
                //
                var request = JsonSerializer.Deserialize<RequestData>(jsonInput);

                if (request == null || string.IsNullOrEmpty(request.Method))
                return JsonSerializer.Serialize(new {error = "Invalid JSON format. Missing required fields." });

                //dictionary med metoder 
                var operations = new Dictionary<string, Func<int, int, string>>
                     {
                      {"RAN", (numb1, numb2) => $"Resultatet er: {new Random().Next(numb1, numb2 + 1)}" }, //Tager et random tal mellem de to tal man vælger
                      {"ADD", (numb1, numb2) => $"Resultatet er: {numb1 + numb2}" }, //Pludser 2 tal
                      {"SUB", (numb1, numb2) => $"Resultatet er: {numb1 - numb2}" } //munisser 2 tal
                     };

                //Tjek om metoden er kendt
                if (!operations.TryGetValue(request.Method.ToUpper(), out var operation))
                    return JsonSerializer.Serialize(new { error = "Unknown method. Use 'RAN', 'ADD' or 'SUB'." });

                // Konverter Numb1 og Numb2 fra string til int
                if (!int.TryParse(request.Numb1, out int numb1))
                    return JsonSerializer.Serialize(new { error = $"Invalid value for Numb1: {request.Numb1}. It must be an integer." });

                if (!int.TryParse(request.Numb2, out int numb2))
                    return JsonSerializer.Serialize(new { error = $"Invalid value for Numb2: {request.Numb2}. It must be an integer." });

                // Beregn resultatet
                string result = operation(numb1, numb2);
                return JsonSerializer.Serialize(new { result });
            }   
            catch (JsonException)
            {
                return JsonSerializer.Serialize(new { error = "Invalid JSON format." });
            }

        }
        }
    }


// skriv fx {"Method":"ADD","Numb1":"5","Numb2":"10"} i socket testen 


