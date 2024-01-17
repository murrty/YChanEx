/*
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
using System.Net;
using System.Text;
using System.Net.Sockets;
using SocksSharp.Helpers;
using SocksSharp.Core.Helpers;
using static SocksSharp.Proxy.Socks5Constants;
public class Socks5 : IProxy {
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
            NetworkStream nStream = client.GetStream();
            InitialNegotiation(nStream);
            SendCommand(nStream, CommandConnect, destinationHost, destinationPort);
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

    #region Methods (private)
    private void InitialNegotiation(NetworkStream nStream) {
        byte authMethod;

        if (Settings.Credentials != null) {
            authMethod = AuthMethodUsernamePassword;
        }
        else {
            authMethod = AuthMethodNoAuthenticationRequired;
        }

        // +----+----------+----------+
        // |VER | NMETHODS | METHODS  |
        // +----+----------+----------+
        // | 1  |    1     | 1 to 255 |
        // +----+----------+----------+
        byte[] request = [
            VersionNumber,
            1,
            authMethod
        ];

        nStream.Write(request, 0, request.Length);

        // +----+--------+
        // |VER | METHOD |
        // +----+--------+
        // | 1  |   1    |
        // +----+--------+
        byte[] response = new byte[2];

        nStream.Read(response, 0, response.Length);

        byte reply = response[1];

        if (authMethod == AuthMethodUsernamePassword && reply == AuthMethodUsernamePassword) {
            SendUsernameAndPassword(nStream);
        }
        else if (reply != CommandReplySucceeded) {
            HandleCommandError(reply);
        }
    }

    private void SendUsernameAndPassword(NetworkStream nStream) {
        byte[] uname = string.IsNullOrEmpty(Settings.Credentials.UserName)
            ? [] : Encoding.ASCII.GetBytes(Settings.Credentials.UserName);

        byte[] passwd = string.IsNullOrEmpty(Settings.Credentials.Password)
            ? [] : Encoding.ASCII.GetBytes(Settings.Credentials.Password);

        // +----+------+----------+------+----------+
        // |VER | ULEN |  UNAME   | PLEN |  PASSWD  |
        // +----+------+----------+------+----------+
        // | 1  |  1   | 1 to 255 |  1   | 1 to 255 |
        // +----+------+----------+------+----------+
        byte[] request = new byte[uname.Length + passwd.Length + 3];

        request[0] = 1;
        request[1] = (byte)uname.Length;
        uname.CopyTo(request, 2);
        request[2 + uname.Length] = (byte)passwd.Length;
        passwd.CopyTo(request, 3 + uname.Length);

        nStream.Write(request, 0, request.Length);

        // +----+--------+
        // |VER | STATUS |
        // +----+--------+
        // | 1  |   1    |
        // +----+--------+
        byte[] response = new byte[2];

        nStream.Read(response, 0, response.Length);

        byte reply = response[1];

        if (reply != CommandReplySucceeded) {
            throw new ProxyException("Unable to authenticate proxy-server");
        }
    }

    private void SendCommand(NetworkStream nStream, byte command, string destinationHost, int destinationPort) {
        byte aTyp = GetAddressType(destinationHost);
        byte[] dstAddr = HostHelper.GetHostAddressBytes(aTyp, destinationHost);
        byte[] dstPort = HostHelper.GetPortBytes(destinationPort);

        // +----+-----+-------+------+----------+----------+
        // |VER | CMD |  RSV  | ATYP | DST.ADDR | DST.PORT |
        // +----+-----+-------+------+----------+----------+
        // | 1  |  1  | X'00' |  1   | Variable |    2     |
        // +----+-----+-------+------+----------+----------+
        byte[] request = new byte[4 + dstAddr.Length + 2];

        request[0] = VersionNumber;
        request[1] = command;
        request[2] = Reserved;
        request[3] = aTyp;
        dstAddr.CopyTo(request, 4);
        dstPort.CopyTo(request, 4 + dstAddr.Length);

        nStream.Write(request, 0, request.Length);

        // +----+-----+-------+------+----------+----------+
        // |VER | REP |  RSV  | ATYP | BND.ADDR | BND.PORT |
        // +----+-----+-------+------+----------+----------+
        // | 1  |  1  | X'00' |  1   | Variable |    2     |
        // +----+-----+-------+------+----------+----------+
        byte[] response = new byte[255];

        nStream.Read(response, 0, response.Length);

        byte reply = response[1];
        if (reply != CommandReplySucceeded) {
            HandleCommandError(reply);
        }
    }

    private static byte GetAddressType(string host) {
        if (!IPAddress.TryParse(host, out IPAddress ipAddr)) {
            return AddressTypeDomainName;
        }

        switch (ipAddr.AddressFamily) {
            case AddressFamily.InterNetwork:
                return AddressTypeIPV4;

            case AddressFamily.InterNetworkV6:
                return AddressTypeIPV6;

            default:
                return 0;
                throw new ProxyException(string.Format("Not supported address type {0}", host));
        }
    }

    private static void HandleCommandError(byte command) {
        string errorMessage = command switch {
            AuthMethodReplyNoAcceptableMethods => "Auth failed: not acceptable method",
            CommandReplyGeneralSocksServerFailure => "General socks server failure",
            CommandReplyConnectionNotAllowedByRuleset => "Connection not allowed by ruleset",
            CommandReplyNetworkUnreachable => "Network unreachable",
            CommandReplyHostUnreachable => "Host unreachable",
            CommandReplyConnectionRefused => "Connection refused",
            CommandReplyTTLExpired => "TTL Expired",
            CommandReplyCommandNotSupported => "Command not supported",
            CommandReplyAddressTypeNotSupported => "Address type not supported",
            _ => "Unknown socks error",
        };
        throw new ProxyException(errorMessage);
    }
    #endregion
}

public static class Socks5Constants {
    public const byte VersionNumber = 5;
    public const byte Reserved = 0x00;
    public const byte AuthMethodNoAuthenticationRequired = 0x00;
    public const byte AuthMethodGssapi = 0x01;
    public const byte AuthMethodUsernamePassword = 0x02;
    public const byte AuthMethodIanaAssignedRangeBegin = 0x03;
    public const byte AuthMethodIanaAssignedRangeEnd = 0x7f;
    public const byte AuthMethodReservedRangeBegin = 0x80;
    public const byte AuthMethodReservedRangeEnd = 0xfe;
    public const byte AuthMethodReplyNoAcceptableMethods = 0xff;
    public const byte CommandConnect = 0x01;
    public const byte CommandBind = 0x02;
    public const byte CommandUdpAssociate = 0x03;
    public const byte CommandReplySucceeded = 0x00;
    public const byte CommandReplyGeneralSocksServerFailure = 0x01;
    public const byte CommandReplyConnectionNotAllowedByRuleset = 0x02;
    public const byte CommandReplyNetworkUnreachable = 0x03;
    public const byte CommandReplyHostUnreachable = 0x04;
    public const byte CommandReplyConnectionRefused = 0x05;
    public const byte CommandReplyTTLExpired = 0x06;
    public const byte CommandReplyCommandNotSupported = 0x07;
    public const byte CommandReplyAddressTypeNotSupported = 0x08;
    public const byte AddressTypeIPV4 = 0x01;
    public const byte AddressTypeDomainName = 0x03;
    public const byte AddressTypeIPV6 = 0x04;
    public const int DefaultPort = 1080;
}
