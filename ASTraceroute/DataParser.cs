using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using CommandLine;

namespace ASTraceroute
{
    public static class DataParser
    {
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Options GetInputData(string[] args)
        {
            var options = new Options();

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(inputOptions =>
                  {
                      options.TargetName = inputOptions.TargetName;
                      options.MaximumHops = inputOptions.MaximumHops;
                      options.Timeout = inputOptions.Timeout;
                  })
                  .WithNotParsed(errors => Environment.Exit(0));

            return options;
        }

        public static IPAddress ConvertToIpAddress(string targetName)
        {
            if (!IPAddress.TryParse(targetName, out IPAddress targetIp))
                targetIp = Dns.GetHostAddresses(targetName)[0];

            return targetIp ?? throw new InvalidCastException(
                       $"Cannot convert getting target_name=\"{targetName}\" to IPAddress type.");
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<string> GenerateResultTable(IEnumerable<NetworkInterfaceInfo> interfacesInfo)
        {
            const string numberLabel = "Number";
            const string interfaceAddressLabel = "Interface address";
            const string asNumberLabel = "ASNumber";
            const string asNameLabel = "ASName";
            const string countryLabel = "Country";

            var columnWidthMap = new Dictionary<string, int>
            {
                { numberLabel, numberLabel.Length + 2 },
                { interfaceAddressLabel, 32 + 7 + 2 },
                { asNumberLabel, asNumberLabel.Length + 2 },
                { asNameLabel, 60 },
                { countryLabel, countryLabel.Length + 2 }
            };

            var stringBuilder = new StringBuilder();

            AppendBorder();
            AppendInterfaceRecord(numberLabel,
                                  new NetworkInterfaceInfo
                                  {
                                      InterfaceAddress = interfaceAddressLabel,
                                      ASNumber = asNumberLabel,
                                      ASName = asNameLabel,
                                      Country = countryLabel
                                  });
            AppendBorder();
            yield return Flush();

            var recordNumber = 0;
            foreach (var info in interfacesInfo)
            {
                AppendInterfaceRecord((++recordNumber).ToString(), info);
                yield return Flush();
            }

            AppendBorder();
            yield return Flush();

            string Flush()
            {
                var builtString = stringBuilder.ToString();
                stringBuilder.Clear();

                return builtString;
            }

            void AppendBorder() =>
                stringBuilder.Append('+').Append('-', columnWidthMap.Values.Sum() + 6 - 2).Append('+')
                             .Append(Environment.NewLine);

            void AppendInterfaceRecord(string number, NetworkInterfaceInfo interfaceInfo)
            {
                AppendColumn(columnWidthMap[numberLabel], number);
                AppendColumn(columnWidthMap[interfaceAddressLabel], interfaceInfo.InterfaceAddress);
                AppendColumn(columnWidthMap[asNumberLabel], interfaceInfo.ASNumber);
                AppendColumn(columnWidthMap[asNameLabel], interfaceInfo.ASName);
                AppendColumn(columnWidthMap[countryLabel], interfaceInfo.Country);

                stringBuilder.Append('|').Append(Environment.NewLine);
            }

            void AppendColumn(int columnWidth, string content)
            {
                if (content.Length > columnWidth - 2 && content.Length > 5)
                    content = content.Substring(0, columnWidth - 5) + "...";

                var stabLength = (columnWidth - content.Length) / 2;

                stringBuilder.Append('|').Append(' ', stabLength).Append(content).Append(' ', stabLength);

                if (stabLength * 2 + content.Length < columnWidth) stringBuilder.Append(' ');
            }
        }
    }
}