/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SMARTMonitor {
    public class SmartCtl {
        public const string SMARTCTL_BIN = "smartctl.exe";

        public Boolean Verbose { get; set; }

        public SmartCtl()  {
            Verbose = false;
        }

        public string[] scan() {
            string output = getScanOutput();
            List<String> devices = new List<String>();

            Regex whitespaceRegex = new Regex("^\\s*$");
            foreach (string line in output.Split('\n')) {
                // Skip lines containing nothing but whitespace
                if (whitespaceRegex.IsMatch(line)) continue;

                // Grab the piece of text before the first space
                string[] tokens = line.Split(' ');
                if (tokens.Length > 0) {
                    devices.Add(tokens[0]);
                }
            }
            return devices.ToArray();
        }

	    public bool isSmartSupported(string device) {
            try
            {
                isHealthy(device);
                return true;
            }
            catch (SmartNotSupportedException)
            {
                return false;
            }
        }

        public bool isHealthy(string device) {
            string output = getSmartHealthStatusOutput(device);
            Regex regex = new Regex(".*test result: (?<result>.*?)\\s", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            Match match = regex.Match(output);
            if (!match.Success)
            {
                throw new SmartNotSupportedException(output, device);
            }

            String result = match.Groups["result"].Value;
            if ("PASSED".Equals(result, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void startShortTest(string device) {
            string output = getStartShortTestOutput(device);
            // This line will be output if the test was succesfully initiated or if
            // another self-test is currently running
            if (!output.Contains("START OF OFFLINE IMMEDIATE"))
            {
                throw new SmartNotSupportedException(output, device);
            }
        }

        protected virtual string getScanOutput()
        {
            return Utils.getStdOut(SMARTCTL_BIN, "--scan", Verbose);
        }

        protected virtual string getSmartHealthStatusOutput(string device)
        {
            return Utils.getStdOut(SMARTCTL_BIN, "-H " + device, Verbose);
        }

        protected virtual string getStartShortTestOutput(string device)
        {
            return Utils.getStdOut(SMARTCTL_BIN, "-t short " + device, Verbose);
        }


    }

    public class Utils
    {
        public static string getStdOut(String cmd, String args, bool verbose=false)
        {
            Process p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(cmd, args);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            // This should allow the executed process to inherit the administrative rights
            startInfo.Verb = "runas";

            p.StartInfo = startInfo;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            if (verbose)
            {
                System.Console.WriteLine("Output of '{0} {1}':\n{2}\n", cmd, args, output);
            }

            return output;
        }
    }

    [Serializable]
    public class SmartNotSupportedException : Exception {
        public SmartNotSupportedException(string message, string device)
            : base(message) {
            Device = device;
        }

        public string Device { get; private set; }
    }
}
