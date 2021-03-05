using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Net;

namespace DUCOMiner
{
    class Program
    {
        const string SERVER_IP = "51.15.127.80";
        const int PORT_NO = 2811;

        static string lastBlockHash;
        static string result;
        static int difficulty;
        static void Main(string[] args)
        {
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse(SERVER_IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, PORT_NO);
            soc.Connect(remoteEP);

            string serverVer = ReceiveFromServer(soc);

            Console.WriteLine("Server is on version " + serverVer + "\n");

            while (true)
            {

                byte[] byData = Encoding.ASCII.GetBytes("JOB,ewranus41,MEDIUM");
                soc.Send(byData);

                string received = ReceiveFromServer(soc);

                string[] job = received.Split(',');
                lastBlockHash = job[0];
                result = job[1];
                difficulty = (Convert.ToInt32(job[2]));

                for (int i = 0; i < difficulty * 100; i++)
                {
                    string sha1 = SHA1Encrypt(lastBlockHash + i);
                    if (sha1 == result)
                    {
                        byte[] byResult = Encoding.UTF8.GetBytes(i.ToString());
                        soc.Send(byResult);

                        string shareFeed = ReceiveFromServer(soc);

                        if (shareFeed == "GOOD" || shareFeed == "BLOCK")
                        {
                            Console.WriteLine("Accepted share " + i.ToString() + " (Difficulty " + difficulty.ToString() + ")" + "\n");
                        }
                        else if (shareFeed == "INVU")
                        {
                            Console.WriteLine("Invalid Username!" + "\n");
                        }
                        else if (shareFeed == "BAD")
                        {
                            Console.WriteLine("Reject share " + i.ToString() + " (Difficulty " + difficulty.ToString() + ")" + "\n");
                        }
                    }
                }
            }
        }

        public static string ReceiveFromServer(Socket soc)
        {
            byte[] buffer = new byte[1024];
            int iRx = soc.Receive(buffer);
            char[] chars = new char[iRx];

            Decoder d = Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
            String recv = new String(chars);

            return recv;
        }

        static string SHA1Encrypt(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}
