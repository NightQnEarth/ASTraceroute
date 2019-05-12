using System;
using CommandLine;

namespace ASTraceroute
{
    public class Options
    {
        private int maximumHops;

        [Option("target_name",
            Required = true,
            HelpText = "Domain name or IP-address of target router.")]
        public string TargetName { get; set; }

        [Option('h', "maximum_hops",
            Default = 30,
            HelpText = "Maximum number of hops to search for target.")]
        public int MaximumHops
        {
            get => maximumHops;
            set
            {
                if (value < 1 || value > 255) throw new ArgumentException("Bad value for option -h.");
                maximumHops = value;
            }
        }

        [Option('w', "timeout",
            Default = 4000,
            HelpText = "Wait timeout milliseconds for each reply.")]
        public int Timeout { get; set; }
    }
}