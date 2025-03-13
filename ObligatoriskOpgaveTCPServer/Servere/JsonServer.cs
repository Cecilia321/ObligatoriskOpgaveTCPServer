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
                //Jeg deserializer - en JSON-streng (jsonInput) bliver deserialiseret (konverteret) til et mit objekt (RequestData) ved hjælp af JsonSerializer.Deserialize<T>().
                RequestData request = JsonSerializer.Deserialize<RequestData>(jsonInput);
                request.Method = request.Method.ToUpper();

                if (request == null || string.IsNullOrEmpty(request.Method))
                return JsonSerializer.Serialize(new {error = "Invalid JSON format. Missing required fields." });

                //dictionary med metoder 
                var operations = new Dictionary<string, Func<int, int, string>>
                     {
                      {"RAN", (numb1, numb2) => $"Resultatet er: {new Random().Next(numb1, numb2 + 1)}" }, //Tager et random tal mellem de to tal man vælger
                      {"ADD", (numb1, numb2) => $"Resultatet er: {numb1 + numb2}" }, //Pludser 2 tal
                      {"SUB", (numb1, numb2) => $"Resultatet er: {numb1 - numb2}" } //munisser 2 tal
                     };


                // Beregn resultatet
                string result = operations[request.Method](request.Numb1, request.Numb2);
                //Serialize konvertrere resultatet til jsonformat
                return JsonSerializer.Serialize(new { result }); 
            }   
            catch (JsonException)
            {
                return JsonSerializer.Serialize(new { error = "Invalid JSON format." });
            }

        }
        }
    }


// skriv fx {"Method":"Add","Numb1":5,"Numb2":10} i socket testen 



