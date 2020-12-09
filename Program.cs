using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Podman;

SocketsHttpHandler httpHandler = new SocketsHttpHandler()
{
    ConnectCallback = static delegate
    {
        var endpoint = new UnixDomainSocketEndPoint($"/run/user/{getuid()}/podman/podman.sock");
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(endpoint);
        return new ValueTask<Stream>(new NetworkStream(socket, ownsSocket: true));

        [DllImport("libc")]
        static extern uint getuid();
    }
};

HttpClient client = new HttpClient(httpHandler);
PodmanClient podmanClient = new PodmanClient(client);

Console.WriteLine("Pull image");
await podmanClient.CreateImageAsync(fromImage: "registry.access.redhat.com/ubi8/dotnet-50",
                                    tag: "latest");

Console.WriteLine("Create container");
ContainerCreateResponse containerCreateResponse = await podmanClient.CreateContainerAsync(/* TODO: where is body? */);
string cid = containerCreateResponse.Id;

Console.WriteLine("Start container");
await podmanClient.StartContainerAsync(cid);

Console.WriteLine("Wait for exit");
ContainerWaitResponse containerWaitResponse = await podmanClient.WaitContainerAsync(cid);
long? exitCode = containerWaitResponse.StatusCode;

Console.WriteLine("Get logs");
using FileResponse logsResponse = await podmanClient.LogsFromContainerAsync(cid, stdout: true, stderr: true);
string logs = await new StreamReader(logsResponse.Stream).ReadToEndAsync();

Console.WriteLine("Container logs: ");
Console.WriteLine(logs);