using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace ASTraceroute
{
    public static class DataParser
    {
        private static readonly Regex asNumberRegex = new Regex(@"^(\d+)\s+\|");
        private static readonly Regex asNameRegex = new Regex(@"\| ((?:\w|\-|,|\s)+)$");
        private static readonly Regex countryRegex = new Regex(@"\| ([A-Z]{2}) \|");
        
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Options GetInputData(string[] args)
        {
            Options options = new Options();

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

        public static NetworkInterfaceInfo ParseWhoisServerResponse(IPAddress ipAddress, string serverResponse) =>
            new NetworkInterfaceInfo
            {
                InterfaceAddress = ipAddress?.ToString() ?? "*",
                ASNumber = serverResponse is null ? "*" : asNumberRegex.Match(serverResponse).Groups[1].Value,
                ASName = serverResponse is null ? "*" : asNameRegex.Match(serverResponse).Groups[1].Value,
                Country = serverResponse is null ? "*" : countryRegex.Match(serverResponse).Groups[1].Value
            };

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<string> ResultTableGenerate(IEnumerable<NetworkInterfaceInfo> interfacesInfo)
        {
            const string asNameColumnHeader = "ASName";
            const string asNumberColumnHeader = "ASNumber";
            const string countryColumnHeader = "Country";
            const string interfaceAddressColumnHeader = "InterfaceAddress";
            const string numberColumnHeader = "Number";
            
            const int interfaceAddressColumnWidth = 32 + 7;
            int asNumberColumnWidth = asNumberColumnHeader.Length;
            const int asNameColumnWidth = 30;
            int countryColumnWidth = countryColumnHeader.Length;
            int interfacesNumberColumnWidth = interfaceAddressColumnHeader.Length;
            
            int tableWidth = interfaceAddressColumnWidth + asNumberColumnWidth + asNameColumnWidth + 
                             countryColumnWidth + interfacesNumberColumnWidth + 5 * 2 + 6;

//            foreach (var interfaceInfo in interfacesInfo)
//            { 
//                maxInterfaceAddressLength = Math.Max(maxInterfaceAddressLength, interfaceInfo.InterfaceAddress.Length);
//                maxASNumberLength = Math.Max(maxASNumberLength, interfaceInfo.ASNumber.Length);
//                maxASNameLength = Math.Max(maxASNameLength, interfaceInfo.ASName.Length);
//                maxCountryLength = Math.Max(maxCountryLength, interfaceInfo.Country.Length);
//                maxInterfaceAddressLength++;
//            }
//
//            maxInterfacesNumberLength = maxInterfacesNumberLength.ToString().Length;

            var stringBuilder = new StringBuilder();
            var rowNumber = 0;

            AppendBorder();

            AppendInterfaceRecord(new NetworkInterfaceInfo
            {
                ASName = asNameColumnHeader,
                ASNumber = asNumberColumnHeader,
                Country = countryColumnHeader,
                InterfaceAddress = interfaceAddressColumnHeader
            }, 
                                  numberColumnHeader);
            
            yield return stringBuilder.ToString();
            stringBuilder.Clear();

            foreach (var info in interfacesInfo)
            {
                AppendInterfaceRecord(info, (++rowNumber).ToString());
                yield return stringBuilder.ToString();
            }
            
            stringBuilder.Clear();
            AppendBorder();
            yield return stringBuilder.ToString();

            void AppendBorder()
            {
                stringBuilder.Append('+');
                stringBuilder.Append('-', tableWidth - 2);
                stringBuilder.Append('+');
                stringBuilder.Append(Environment.NewLine);
            }

            void AppendInterfaceRecord(NetworkInterfaceInfo interfaceInfo, string recordNumber)
            {
                stringBuilder.Clear();
                
                AppendColumn(interfacesNumberColumnWidth, recordNumber);
                AppendColumn(interfaceAddressColumnWidth, interfaceInfo.InterfaceAddress);
                AppendColumn(countryColumnWidth, interfaceInfo.Country);
                AppendColumn(asNumberColumnWidth, interfaceInfo.ASNumber);
                AppendColumn(asNameColumnWidth, interfaceInfo.ASName);

                stringBuilder.Append('|');
                
            }

            void AppendColumn(int columnWidth, string content)
            {
                stringBuilder.Append('|');
                var stabLength = columnWidth - content.Length / 2;
                stringBuilder.Append(new string(' ', stabLength));
                stringBuilder.Append(content);
                stringBuilder.Append(new string(' ', stabLength));
                if (stabLength * 2 + content.Length < columnWidth + 2)
                    stringBuilder.Append(' ');
            }
        }
    }
}