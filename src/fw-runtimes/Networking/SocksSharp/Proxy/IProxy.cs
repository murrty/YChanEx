﻿namespace SocksSharp.Proxy;
using System.Net.Sockets;
/// <summary>
/// Provides an interface for proxy client
/// </summary>
public interface IProxy {
    /// <summary>
    /// Gets or sets proxy settings
    /// </summary>
    IProxySettings Settings { get; set; }

    /// <summary>
    /// Create connection to destination host via proxy server.
    /// </summary>
    /// <param name="destinationHost">Host</param>
    /// <param name="destinationPort">Port</param>
    /// <param name="client">Connection with proxy server.</param>
    /// <returns>Connection to destination host</returns>
    TcpClient CreateConnection(string destinationHost, int destinationPort, TcpClient client);
}
