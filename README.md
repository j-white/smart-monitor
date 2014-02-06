smart-monitor
=============
----------
What is smart-monitor?
-------------------
smart-monitor is a wrapper for [smartmontools](http://smartmontools.sourceforge.net "smartmontools") that allows administrators to verify the SMART status on Windows systems from Kaseya.

How does it work?
-------------------
smartmon interfaces with smartctl via the command-line interface, and when ran does the following:

- Lists of all of the available devices: 'smartctl --scan'
- For every device that supports SMART:
    * Checks the health status: 'smartctl -H $device$'
    * Initiates a new self-test: 'smartctl -t short $device$'

If SMART is supported on at least one device and no devices are failing, the script will exit with a return code of 0 and output:

<pre>
OK - 1 device(s) have been verified as healthy.
</pre>

If none of the devices support SMART, the script will exit with a return code of 2 and output:
<pre>
CRITICAL - None of the available devices (1 total) support SMART.
</pre>

If one or more of the devices that support SMART are failing the self-tests, the script will output:

<pre>
CRITICAL - SMART failure detected on /dev/sda.
</pre>

Debugging
-------------------
Start smartmon with <code>smartmon -v</code> to view the commands executed and their output.

License
-------------------
GPLv3
