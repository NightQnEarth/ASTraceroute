using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ASTraceroute
{
    public static class Traceroute
    {
        private static readonly Regex asNumberRegex = new Regex(@"^(\d+)\s+\|");
        private static readonly Regex asNameRegex = new Regex(@"\| ((?:\w|\-|,| |\t)+)\s*$");
        private static readonly Regex countryRegex = new Regex(@"\| ([A-Z]{2}) \|");

        private static IEnumerable<IPAddress> GetTraceroute(IPAddress target, int maximumHops, int timeout)
        {
            Ping pingSender = new Ping();
            PingOptions pingOptions = new PingOptions(1, true);
            var buffer = new byte[32];

            while (pingOptions.Ttl <= maximumHops)
            {
                PingReply pingReply = pingSender.Send(target, timeout, buffer, pingOptions);

                // ReSharper disable once PossibleNullReferenceException
                yield return pingReply.Address;

                if (pingReply.Status != IPStatus.TtlExpired && pingReply.Status != IPStatus.TimedOut)
                    yield break;

                pingOptions.Ttl++;
            }
        }

        private static string GetWhoisInformationFromRemoteServer(DnsEndPoint whoisServer, IPAddress targetIP)
        {
            if (targetIP is null) return null;

            using (var whoisTcpClient = new TcpClient(whoisServer.Host, whoisServer.Port))
                using (NetworkStream networkStream = whoisTcpClient.GetStream())
                    using (var streamWriter = new StreamWriter(networkStream))
                    {
                        streamWriter.WriteLine($"-c -f {targetIP}");
                        streamWriter.Flush();

                        using (var streamReader = new StreamReader(networkStream, Encoding.UTF8, true, 3000, true))
                        {
                            var resultBuilder = new StringBuilder();

                            while (!streamReader.EndOfStream)
                                resultBuilder.AppendLine(streamReader.ReadLine());

                            return resultBuilder.ToString();
                        }
                    }
        }

        private static NetworkInterfaceInfo ParseWhoisServerResponseToInterfaceInfo(
            IPAddress ipAddress, string serverResponse) =>
            new NetworkInterfaceInfo
            {
                InterfaceAddress = ipAddress?.ToString() ?? "*",
                ASNumber = serverResponse is null ? "*" : asNumberRegex.Match(serverResponse).Groups[1].Value,
                ASName = serverResponse is null ? "*" : asNameRegex.Match(serverResponse).Groups[1].Value,
                Country = serverResponse is null ? "*" : countryRegex.Match(serverResponse).Groups[1].Value
            };

        public static IEnumerable<NetworkInterfaceInfo> DetailedTraceroute(
            IPAddress target, int maximumHops, int timeout, DnsEndPoint whoisServer) =>
            GetTraceroute(target, maximumHops, timeout)
                .Select(address => ParseWhoisServerResponseToInterfaceInfo(
                            address, GetWhoisInformationFromRemoteServer(whoisServer, address)));
    }
}