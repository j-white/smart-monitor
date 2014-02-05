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
                if (whitespaceRegex.IsMatch(line)) continue;

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
            if (!output.Contains("START OF OFFLINE IMMEDIATE"))
            {
                throw new SmartNotSupportedException(output, device);
            }
        }

        protected virtual string getScanOutput()
        {
            return getStdOut(SMARTCTL_BIN, "--scan");
        }

        protected virtual string getSmartHealthStatusOutput(string device)
        {
            return getStdOut(SMARTCTL_BIN, "-H " + device);
        }

        protected virtual string getStartShortTestOutput(string device)
        {
            return getStdOut(SMARTCTL_BIN, "-t short " + device);
        }

	    public string getStdOut(String cmd, String args) {
		    Process p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(cmd, args);
		    startInfo.UseShellExecute = false;
		    startInfo.RedirectStandardOutput = true;
		    startInfo.Verb = "runas";
            p.StartInfo = startInfo;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            if (Verbose)
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
