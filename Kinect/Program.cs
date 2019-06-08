using LibUDP;
using System;
using System.Text;

namespace TesteUDP
{
    class Program
    {
        static void Main(string[] args)
        {
            using (UDPSocket socket = new UDPSocket(DataReceived, 42001))
            {
                Console.WriteLine("Tecle Enter para parar");
                Console.ReadLine();
            }
        }

        private static void DataReceived(byte[] buffer, int size, string ip, int port)
        {
            if (size < 26)
            {
                return;
            }
            string[] partes = Encoding.ASCII.GetString(buffer, 18, size - 18).Split(' ');

        }
    }
}
