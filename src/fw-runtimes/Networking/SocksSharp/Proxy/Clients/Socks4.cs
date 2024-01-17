﻿/*
Copyright © 2012-2015 Ruslan Khuduev <x-rus@list.ru>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
namespace SocksSharp.Proxy;
using System.IO;
using System.Text;
using System.Net.Sockets;
using SocksSharp.Helpers;
using SocksSharp.Core.Helpers;
using static SocksSharp.Proxy.Socks4Constants;
public class Socks4 : IProxy {
    internal protected const int DefaultPort = 1080;

    public IProxySettings Settings { get; set; }

    /// <summary>
    /// Create connection to destination host via proxy server.
    /// </summary>
    /// <param name="destinationHost">Host</param>
    /// <param name="destinationPort">Port</param>
    /// <param name="client">Connection with proxy server.</param>
    /// <returns>Connection to destination host</returns>
    /// <exception cref="ArgumentException">Value of <paramref name="destinationHost"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Value of <paramref name="destinationPort"/> less than 1 or greater than 65535.</exception>
    /// <exception cref="ProxyException">Error while working with proxy.</exception>
    public TcpClient CreateConnection(string destinationHost, int destinationPort, TcpClient client) {
        if (string.IsNullOrEmpty(destinationHost)) {
            throw new ArgumentException(nameof(destinationHost));
        }

        if (!ExceptionHelper.ValidateTcpPort(destinationPort)) {
            throw new ArgumentOutOfRangeException(nameof(destinationPort));
        }

        if (client?.Connected != true) {
            throw new SocketException();
        }

        try {
            SendCommand(client.GetStream(), CommandConnect, destinationHost, destinationPort);
        }
        catch (Exception ex) {
            client.Close();
            if (ex is IOException || ex is SocketException) {
                throw new ProxyException("Error while working with proxy", ex);
            }
            throw;
        }

        return client;
    }

    #region Methods (protected)
    internal protected virtual void SendCommand(NetworkStream nStream, byte command, string destinationHost, int destinationPort) {
        var dstIp = HostHelper.GetIPAddressBytes(destinationHost);
        var dstPort = HostHelper.GetPortBytes(destinationPort);

        byte[] userId = [];
        if (Settings.Credentials != null) {
            if (!string.IsNullOrEmpty(Settings.Credentials.UserName)) {
                userId = Encoding.ASCII.GetBytes(Settings.Credentials.UserName);
            }
        }

        // +----+----+----+----+----+----+----+----+----+----+....+----+
        // | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL|
        // +----+----+----+----+----+----+----+----+----+----+....+----+
        //    1    1      2              4           variable       1
        byte[] request = new byte[9 + userId.Length];

        request[0] = VersionNumber;
        request[1] = command;
        dstPort.CopyTo(request, 2);
        dstIp.CopyTo(request, 4);
        userId.CopyTo(request, 8);
        request[8 + userId.Length] = 0x00;

        nStream.Write(request, 0, request.Length);

        // +----+----+----+----+----+----+----+----+
        // | VN | CD | DSTPORT |      DSTIP        |
        // +----+----+----+----+----+----+----+----+
        //   1    1       2              4
        byte[] response = new byte[8];

        nStream.Read(response, 0, response.Length);

        byte reply = response[1];

        if (reply != CommandReplyRequestGranted) {
            HandleCommandError(reply);
        }
    }

    internal protected static void HandleCommandError(byte command) {
        string errorMessage = command switch {
            CommandReplyRequestRejectedOrFailed => "Request rejected or failed",
            CommandReplyRequestRejectedCannotConnectToIdentd => "Request rejected: cannot connect to identd",
            CommandReplyRequestRejectedDifferentIdentd => "Request rejected: different identd",
            _ => "Unknown socks error",
        };
        throw new ProxyException(errorMessage);
    }
    #endregion
}

public static class Socks4Constants {
    public const byte VersionNumber = 4;
    public const byte CommandConnect = 0x01;
    public const byte CommandBind = 0x02;
    public const byte CommandReplyRequestGranted = 0x5a;
    public const byte CommandReplyRequestRejectedOrFailed = 0x5b;
    public const byte CommandReplyRequestRejectedCannotConnectToIdentd = 0x5c;
    public const byte CommandReplyRequestRejectedDifferentIdentd = 0x5d;
}
