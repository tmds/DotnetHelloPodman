using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Podman;

SocketsHttpHandler httpHandler = new SocketsHttpHandler()
{
    ConnectCallback = delegate
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

// Pull the image.
await podmanClient.CreateImageAsync(fromImage: "registry.redhat.io/ubi8/dotnet-50",
                                    tag: "latest");

// Create container.
ContainerCreateResponse containerCreateResponse = await podmanClient.CreateContainerAsync(/* TODO: where is body? */);
string cid = containerCreateResponse.Id;

// Start container.
await podmanClient.StartContainerAsync(cid);

// Wait for container to exit.
ContainerWaitResponse containerWaitResponse = await podmanClient.WaitContainerAsync(cid);
long? exitCode = containerWaitResponse.StatusCode;

// Read the logs.
using FileResponse logsResponse = await podmanClient.LogsFromContainerAsync(cid, stdout: true, stderr: true);
string logs = await new StreamReader(logsResponse.Stream).ReadToEndAsync();

System.Console.WriteLine(logs);