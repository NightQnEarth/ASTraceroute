using System;
using System.Net;
using CommandLine;

namespace ASTraceroute
{
    static class Program
    {
        public static void Main(string[] args)
        {
            string targetName = null;
            int maximumHops = 0;
            int timeout = 0;

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(options =>
                  {
                      targetName = options.TargetName;
                      maximumHops = options.MaximumHops;
                      timeout = options.Timeout;
                  })
                  .WithNotParsed(errors => Environment.Exit(0));

            if (!IPAddress.TryParse(targetName, out IPAddress targetIp))
                targetIp = Dns.GetHostAddresses(targetName)[0];

            foreach (var ipAddress in Traceroute.GetTraceroute(targetIp, maximumHops, timeout))
                Console.WriteLine(ipAddress);
        }
    }
}