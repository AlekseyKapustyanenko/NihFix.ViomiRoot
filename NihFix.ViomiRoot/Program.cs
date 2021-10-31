using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SharpAdbClient;

namespace NihFix.ViomiRoot
{
    class Program
    {
        static void Main(string[] args)
        {
            AdbServer server = new AdbServer();
            var result = server.StartServer(@"F:\adb\platform-tools\adb.exe", restartServerIfNewer: true);
            var adbClient = new AdbClient();
            using var adbShellData = File.OpenRead(@"./adb_shell");
            Console.WriteLine("Root has been started");
            while (true)
            {
                try
                {
                    Root(adbClient, adbShellData);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                }
            }
            Console.ReadLine();
        }


        private static DeviceData GetDevice(AdbClient client)
        {
            while (true)
            {
                var device = client.GetDevices().FirstOrDefault();
                if (device != null)
                {
                    return device;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }
        }

        private static void Root(AdbClient client, Stream adbShellData)
        {
            var device = GetDevice(client);
            //PushAdbShell(device,adbShellData);
            PersistAdbShell(device, client);
        }

        private static void PushAdbShell(DeviceData device, Stream adbShellData)
        {
            
                

                Console.WriteLine(
                    $"Find device. Name: {device.Name} Model: {device.Model} DeviceId: {device.TransportId}");
                using SyncService service =
                    new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device);

                service.Push(adbShellData, "/bin/adb_shell", 777, DateTime.Now, null, CancellationToken.None);


                Console.WriteLine("adb_shell is pushed");
        }

        private static void PersistAdbShell(DeviceData device, AdbClient client)
        {
            var receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand("rm /etc/rc.d/S90robotManager", device,receiver);
            Console.WriteLine($"PersistHaveDone: {receiver.ToString()}");
        }
    }
}