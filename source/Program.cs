//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Utility.CommandLine;

namespace nanoFramework.Tools
{
    class Program
    {
        [Argument('h', "hexfile")]
        private static string HexFile { get; set; }

        [Argument('b', "binfile")]
        private static List<string> BinFiles { get; set; }

        [Argument('a', "address")]
        private static List<string> Addresses { get; set; }


        [Argument('o', "outputdfu")]
        private static string OutputDfuFile { get; set; }

        [Argument('v', "vid")]
        private static string Vid { get; set; }

        [Argument('p', "pid")]
        private static string Pid { get; set; }

        [Argument('f', "fwversion")]
        private static string FirmwareVersion { get; set; }

        static int Main(string[] args)
        {
            Arguments.Populate();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"nanoFramework HEX2DFU converter v{Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine($"Copyright (c) 2017 nanoFramework project contributors");
            Console.WriteLine();


            // output usage help if no arguments are specified
            if (args.Count() == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine(" adding a single HEX file: hex2dfu -h=hex_file_name -o=output_DFU_image_file_name");
                Console.WriteLine(" adding one or more BIN files: hex2dfu -b=bin_file_name -a=address_to_flash [-b=bin_file_name_N -a=address_to_flash_N] -o=output_DFU_image_file_name");
                Console.WriteLine();
                Console.WriteLine("  options:");
                Console.WriteLine();
                Console.WriteLine(@"     [-v=""0000""] (VID of target USB device (hexadecimal format), leave empty to use STM default)");
                Console.WriteLine(@"     [-p=""0000""] (PID of target USB device (hexadecimal format), leave empty to use STM default)");
                Console.WriteLine(@"     [-f=""0000""] (Firmware version of the target USB device (hexadecimal format), leave empty to use default)");
                Console.WriteLine();

                return 0;
            }

            // args check

            // output DFU file name is mandatory
            if (OutputDfuFile == null)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Output DFU target file name is required.");
                Console.WriteLine();
                Console.WriteLine(@"Use -o=output_DFU_image_file_name");
                Console.WriteLine();
                Console.WriteLine();

                return 1;
            }

            // need, at least, one hex file
            if (HexFile == null && BinFiles == null)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Need at least one HEX or BIN file to create DFU target image.");
                Console.WriteLine();
                Console.WriteLine(@"Use -h=path-to-hex-file for each HEX file to add to the DFU target.");
                Console.WriteLine(@"Use -b=bin_file_name -a=address_to_flash [-b=bin_file_name_N -a=address_to_flash_N] for each BIN file to add to the DFU target.");
                Console.WriteLine();
                Console.WriteLine();

                return 2;
            }

            if (BinFiles != null)
            {
                // need the addresses too
                if (Addresses == null)
                {
                    Console.WriteLine();
                    Console.WriteLine("ERROR: For BIN files the addresses to flash are mandatory.");
                    Console.WriteLine();
                    Console.WriteLine(@"Use -b=bin_file_name -a=address_to_flash [-b=bin_file_name_N -a=address_to_flash_N] for each BIN file to add to the DFU target.");
                    Console.WriteLine();
                    Console.WriteLine();

                    return 3;
                }
            }

            var dfuFile = new FileInfo(OutputDfuFile);

            var vendorId = ushort.TryParse(Vid, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var vid) ? vid : (ushort) 0x0483;
            var productId = ushort.TryParse(Pid, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var pid) ?  pid : (ushort) 0xDF11;
            var firmwareVersion = ushort.TryParse(FirmwareVersion, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var version) ? version : (ushort) 0x2200;
            
            if (HexFile != null)
            {
                var hexFile = new FileInfo(HexFile);
                Hex2Dfu.CreateDfuFile(hexFile, dfuFile, vendorId, productId, firmwareVersion);
            }
            else
            {
                // combine BIN files and addresses
                List<BinaryFileInfo> binFiles = new List<BinaryFileInfo>();

                var addressEnum = Addresses.GetEnumerator();

                foreach (string file in BinFiles)
                {
                    addressEnum.MoveNext();
                    binFiles.Add(new BinaryFileInfo(new FileInfo(file), uint.Parse(addressEnum.Current, System.Globalization.NumberStyles.HexNumber)));
                }

                Hex2Dfu.CreateDfuFile(binFiles, dfuFile, vendorId, productId, firmwareVersion);
            }

            return 0;
        }
    }
}
