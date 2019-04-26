using System;
using Grpc.Core;
using MagicOnion.Server;

namespace MagicOnionServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input IP Server: ");
            string ipAddress = Console.ReadLine();
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

            var server = new global::Grpc.Core.Server
            {
                Services = { service },
                Ports = { new ServerPort(ipAddress, 12345, ServerCredentials.Insecure) }
            };

            server.Start();
            Console.WriteLine("Server started");
            Console.ReadLine();
        }
    }
}
