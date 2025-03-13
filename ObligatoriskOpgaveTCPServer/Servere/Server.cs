using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObligatoriskOpgaveTCPServer.Servere
{
    class Server
    {
        private const int PORTNUMMER = 7777;

        public void Start()
        {
            TcpListener server = new TcpListener(System.Net.IPAddress.Any, PORTNUMMER);
            server.Start();

            //Vi gør at kunden kan håndtere mere end en forbindelse 
            while (true)
            {
                TcpClient socket = server.AcceptTcpClient();
                Task.Run(() =>
                {
                    TcpClient TempSocket = socket;
                    DoOneClient(socket);
                });
            }
        }

        private static void DoOneClient(TcpClient socket)
        {
            StreamReader reader = new StreamReader(socket.GetStream());

            StreamWriter writer = new StreamWriter(socket.GetStream());
            try
            {


                writer.AutoFlush = true;
                while (true)
                {
                    
                    //Jeg laver en dictionary, som har en nøgleværdi og benytter func, som er en funktion, der tager to int-værdier og returnere en string 
                    //hver operation er defineret som en lambda-funktion, (numb1, numb2) =>..., som udfører en beregning. 
                    var operations = new Dictionary<string, Func<int, int, string>>
                     {
                      {"RAN", (numb1, numb2) => $"Resultatet er: {new Random().Next(numb1, numb2 + 1)}" }, //Tager et random tal mellem de to tal man vælger
                      {"ADD", (numb1, numb2) => $"Resultatet er: {numb1 + numb2}" }, //Pludser 2 tal
                      {"SUB", (numb1, numb2) => $"Resultatet er: {numb1 - numb2}" } //munisser 2 tal
                     };

                    

                    //læser jeg klientens valg
                    string operation = reader.ReadLine().Trim().ToUpper();
                    if (operation == null || !operations.ContainsKey(operation))
                    {
                        writer.WriteLine("Ugyldig komando - tast sub, add, ran");
                        return;
                    }

                    //Serveren svarer
                    writer.WriteLine("Tast input 2 tal som er sepereret af mellemrum");

                    //læser klientens svar
                    string numberInputs = reader.ReadLine();
                    string[] numbers = numberInputs.Split(' ');
                    if (numberInputs == null || numbers.Length != 2 || !int.TryParse(numbers[0], out int numb1) || !int.TryParse(numbers[1], out int numb2))
                    {
                        writer.WriteLine("Ugyldigt input");
                        return;
                    }

                    //nu beregner serveren og sender svaret 
                    string result = operations[operation](numb1, numb2);
                    writer.WriteLine(result);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl: {ex.Message}");
            }
            finally
            {
                socket.Close();
            }
        }
        }
    }

