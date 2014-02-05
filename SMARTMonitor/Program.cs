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
using System.Linq;
using System.Text;

namespace SMARTMonitor {
    class Program {
        private const int STATE_OK = 0;
        private const int STATE_CRITICAL = 2;

        static void Main(string[] args) {
            bool verbose = false;
            if (args.Length == 1 && "-v".Equals(args[0])) 
            {
                verbose = true;
            }

            SmartCtl smartCtl = new SmartCtl();
            smartCtl.Verbose = verbose;

            string[] devices = smartCtl.scan();

            if (devices.Length < 1) {
	            System.Console.WriteLine("CRITICAL - No devices listed.");
	            Environment.ExitCode = STATE_CRITICAL;
                return;
            }

            int numHealthyDevices = 0;
            string deviceWithError = null;
            foreach (string device in devices) {
	            try {
		            if(!smartCtl.isHealthy(device)) {
			            deviceWithError = device;
		            } else {
			            // Trigger a short scan for next time
			            numHealthyDevices++;
			            smartCtl.startShortTest(device);
		            }
	            } catch (SmartNotSupportedException) {}
            }

            if (deviceWithError != null) {
	            System.Console.WriteLine("CRITICAL - SMART failure detected on {0}.", deviceWithError);
                Environment.ExitCode = STATE_CRITICAL;
            } else if (numHealthyDevices == 0) {
	            System.Console.WriteLine("CRITICAL - None of the available devices ({0} total) support SMART.", devices.Length);
                Environment.ExitCode = STATE_CRITICAL;
            } else {
	            System.Console.WriteLine("OK - {0} device(s) have been verified as healthy.", numHealthyDevices);
                Environment.ExitCode = STATE_OK;
            }
        }
    }
}
