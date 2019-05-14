using System;
using System.Linq;
using System.Net;

namespace ASTraceroute
{
    static class Program
    {
        private static readonly DnsEndPoint whoisEndPoint = new DnsEndPoint("whois.cymru.com", 43);

        public static void Main(string[] args)
        {
            var inputOptions = DataParser.GetInputData(args);
            var traceroute = Traceroute.GetTraceroute(DataParser.ConvertToIpAddress(inputOptions.TargetName),
                                                      inputOptions.MaximumHops, inputOptions.Timeout);
            var interfacesInfo = traceroute.Select(
                address => Traceroute.GetWhoisInformationFromRemoteServer(whoisEndPoint, address));

            var asTraceroute = traceroute.Zip(interfacesInfo, DataParser.ParseWhoisServerResponse);

            foreach (var tableRecord in DataParser.ResultTableGenerate(asTraceroute))
                Console.Write(tableRecord);
        }
    }
}