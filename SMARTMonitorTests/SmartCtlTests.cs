using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMARTMonitor;

namespace SMARTMonitorTests
{
    [TestClass]
    public class SmartCtlTests
    {
        private SmartCtl smartCtl = new MockSmartCtl();

        [TestMethod]
        public void scan()
        {
            string[] devices = smartCtl.scan();
            Assert.AreEqual(2, devices.Length);
            Assert.AreEqual("/dev/sda", devices[0]);
            Assert.AreEqual("/dev/sdb", devices[1]);
        }

        [TestMethod]
        public void isSmartSupported()
        {
            Assert.AreEqual(true, smartCtl.isSmartSupported("/dev/sda"));
            Assert.AreEqual(false, smartCtl.isSmartSupported("/dev/sdb"));
        }

        [TestMethod]
        public void isHealthy()
        {
            Assert.AreEqual(true, smartCtl.isHealthy("/dev/sda"));
            Assert.AreEqual(false, smartCtl.isHealthy("/dev/sdc"));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartNotSupportedException))]
        public void isHealthy_Throws()
        {
            Assert.AreEqual(false, smartCtl.isHealthy("/dev/sdb"));
        }

        [TestMethod]
        public void startShortTest()
        {
            smartCtl.startShortTest("/dev/sda");
            smartCtl.startShortTest("/dev/sda_started");
        }

        [TestMethod]
        [ExpectedException(typeof(SmartNotSupportedException))]
        public void startShortTest_Throws()
        {
            smartCtl.startShortTest("/dev/sdb");
        }

        [TestMethod]
        public void canCaptureOutput()
        {
            Assert.IsTrue(smartCtl.getStdOut(SmartCtl.SMARTCTL_BIN, "-h").Contains("IOCTL_SCSI"));
        }
    }

    public class MockSmartCtl : SmartCtl {
        protected override string getScanOutput() {
            return @"/dev/sda -d ata # /dev/sda, ATA device
/dev/sdb -d scsi # /dev/sdb, SCSI device
   
    
";
        }

        protected override string getSmartHealthStatusOutput(string device)
        {
            if ("/dev/sda".Equals(device))
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

Warning: Limited functionality due to missing admin rights
Read SMART Thresholds failed: Function not implemented

=== START OF READ SMART DATA SECTION ===
SMART overall-health self-assessment test result: PASSED
";
            }
            else if ("/dev/sdb".Equals(device))
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

Standard Inquiry (36 bytes) failed [Input/output error]
Retrying with a 64 byte Standard Inquiry
Standard Inquiry (64 bytes) failed [Input/output error]
A mandatory SMART command failed: exiting. To continue, add one or more '-T permissive' options.
";
            }
            else
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

Warning: Limited functionality due to missing admin rights
Read SMART Thresholds failed: Function not implemented

=== START OF READ SMART DATA SECTION ===
SMART overall-health self-assessment test result: FAILED!
";
            }
        }

        protected override string getStartShortTestOutput(string device)
        {
            if ("/dev/sda".Equals(device))
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

=== START OF OFFLINE IMMEDIATE AND SELF-TEST SECTION ===
Sending command: ""Execute SMART Short self-test routine immediately in off-line mode"".
Drive command ""Execute SMART Short self-test routine immediately in off-line mode"" successful.
Testing has begun.
Please wait 2 minutes for test to complete.
Test will complete after Tue Feb 04 22:35:01 2014

Use smartctl -X to abort test.";
            }
            else if ("/dev/sda_started".Equals(device))
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

=== START OF OFFLINE IMMEDIATE AND SELF-TEST SECTION ===
Can't start self-test without aborting current test (90% remaining),
add '-t force' option to override, or run 'smartctl -X' to abort test.";
            }
            else
            {
                return @"smartctl 6.2 2013-07-26 r3841 [i686-w64-mingw32-win7(64)-sp1] (sf-6.2-1)
Copyright (C) 2002-13, Bruce Allen, Christian Franke, www.smartmontools.org

Standard Inquiry (36 bytes) failed [Input/output error]
Retrying with a 64 byte Standard Inquiry
Standard Inquiry (64 bytes) failed [Input/output error]
A mandatory SMART command failed: exiting. To continue, add one or more '-T permissive' options.
";
            }
        }
    }
}
