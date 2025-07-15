# VS ATF1504AS Builder file
#  Deployment file created at 7/10/2025, 6:59:27 AM
#  Deployment file created at 7/10/2025, 6:58:25 AM
#  Deployment file created at 7/10/2025, 6:57:08 AM
# Executed on 7/15/2025, 1:55:33 AM
"/usr/bin/openocd" -f "/usr/share/openocd/scripts/interface/ftdi/um232h.cfg"  -c "adapter speed 400" -c "transport select jtag" -c "jtag newtap ATF1504AS tap -irlen 3 -expected-id 0x0150403f" -c init -c "svf /home/vsadmin/Projects/Personal/VGA_PLD/VGA_SINGLE_PLD/atmisp/VGA_SINGLE_PLD.svf"  -c "sleep 200" -c shutdown 