using System;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace ASTraceroute
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Options
    {
        private int maximumHops;

        [Value(0, MetaName = "target_name",
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
                if (value < 1 || value > 255)
                    throw new ArgumentException($"Bad value for option -h. Value \"{value}\" out of range [1, 255].");
                maximumHops = value;
            }
        }

        [Option('w', "timeout",
            Default = 4000,
            HelpText = "Wait timeout milliseconds for each reply.")]
        public int Timeout { get; set; }
    }
}