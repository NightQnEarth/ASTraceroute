using System;
using System.Net;

namespace ASTraceroute
{
    static class Program
    {
        private static readonly DnsEndPoint whoisEndPoint = new DnsEndPoint("whois.cymru.com", 43);

        public static void Main(string[] args)
        {
            var inputOptions = DataParser.GetInputData(args);

            var detailedTraceroute = Traceroute.GetDetailedTraceroute(
                DataParser.ConvertToIpAddress(inputOptions.TargetName),
                inputOptions.MaximumHops, inputOptions.Timeout, whoisEndPoint);

            foreach (var tableRecord in DataParser.GenerateResultTable(detailedTraceroute))
                Console.Write(tableRecord);
        }
    }
}