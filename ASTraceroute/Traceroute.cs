using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace ASTraceroute
{
    public static class Traceroute
    {
        public static IEnumerable<IPAddress> GetTraceroute(IPAddress target, int maximumHops, int timeout)
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
    }
}