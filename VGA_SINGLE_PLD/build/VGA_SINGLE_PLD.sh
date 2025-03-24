# VS ATF1504AS Builder file
#  Deployment file created at 3/21/2025, 1:51:31 PM
# Executed on 3/23/2025, 8:18:58 PM
"/usr/bin/openocd" -f "/usr/share/openocd/scripts/interface/ftdi/um232h.cfg"  -c "adapter speed 400" -c "transport select jtag" -c "jtag newtap ATF1504AS tap -irlen 3 -expected-id 0x0150403f" -c init -c "svf /home/vsadmin/Projects/Personal/VGA_PLD/VGA_SINGLE_PLD/atmisp/VGA_SINGLE_PLD.svf"  -c "sleep 200" -c shutdown 
