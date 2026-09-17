using System;
using System.IO;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Core;

namespace revit_mcp_plugin.Utils
{
    /// <summary>
    /// Reads the authenticated endpoint published by the running SocketService.
    /// Keeping this next to the plug-in assembly prevents an in-Revit client from
    /// accidentally connecting to a different Revit session on the same machine.
    /// </summary>
    internal sealed class LocalMcpConnectionInfo
    {
        public int Port { get; private set; }
        public string Token { get; private set; }

        private LocalMcpConnectionInfo(int port, string token)
        {
            Port = port;
            Token = token;
        }

        public static bool TryRead(out LocalMcpConnectionInfo connectionInfo, out string error)
        {
            connectionInfo = null;
            error = null;

            try
            {
                var assemblyPath = typeof(SocketService).Assembly.Location;
                var directory = Path.GetDirectoryName(assemblyPath);
                var portFilePath = Path.Combine(directory ?? string.Empty, "mcp-port.txt");

                if (!File.Exists(portFilePath))
                {
                    error = "The Revit MCP session is not active. Start the MCP server from the Revit panel and try again.";
                    return false;
                }

                var document = JObject.Parse(File.ReadAllText(portFilePath));
                var port = document.Value<int?>("port");
                var token = document.Value<string>("token");

                if (!port.HasValue || port.Value < 1024 || port.Value > 65535 || string.IsNullOrWhiteSpace(token) || token.Length < 32)
                {
                    error = "The active Revit MCP session file is invalid. Restart the MCP server from the Revit panel.";
                    return false;
                }

                connectionInfo = new LocalMcpConnectionInfo(port.Value, token);
                return true;
            }
            catch (Exception)
            {
                error = "The active Revit MCP session could not be read. Restart the MCP server from the Revit panel.";
                return false;
            }
        }
    }
}
