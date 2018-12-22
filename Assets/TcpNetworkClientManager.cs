using System;
#if UNITY_UWP
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif

public class TcpNetworkClientManager
{
#if UNITY_UWP
    private Stream stream = null;
    private StreamWriter writer = null;
#endif

    public TcpNetworkClientManager(string IP, int port)
    {
#if UNITY_UWP
        Task.Run(async () => {
            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(new HostName(IP),port.ToString());
            stream = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
            try
            {
                string data = await reader.ReadToEndAsync();
            }
            catch (Exception) { }
            writer = null;
        });
#endif
    }

    public void SendMessage(string data)
    {
#if UNITY_UWP
        if (writer != null) Task.Run(async () =>
        {
            await writer.WriteAsync(data);
            await writer.FlushAsync();
        });
#endif
    }

    public void SendImage(byte[] image)
    {
#if UNITY_UWP
        if (stream != null) Task.Run(async () =>
        {
            await stream.WriteAsync(image, 0, image.Length);
            await stream.FlushAsync();
        });
#endif
    }
}