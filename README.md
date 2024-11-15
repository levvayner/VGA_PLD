# VGA Video Card
This project uses TTL components and 2 GALs to create a video card.
There are several parts to the entire development system to aid in development and troubleshooting. Another repository for code that is used on the Arduino Due can be found at https://github.com/levvayner/SRAM-Console


## Parts
 - Hardware Design files in KiCad
 - 2 projects for progamming GAL22V10C PLDs
 - Application used to refine and upload images to the Arduino Due
 - Timing and other documentation
 - Firmware for the Arduino Due to host the video card (https://github.com/levvayner/SRAM-Console)

 ## Getting Started

 ### Prerequisites
 #### Schematics
KiCad software - available for free @ https://www.kicad.org/
 #### Programmable Logic Devices
This can be done with the official Atmel software (https://www.microchip.com/en-us/products/fpgas-and-plds/spld-cplds/pld-design-resources) on windows and using their programmer. 
An alternative approach is to use a cheap progammer like the **TL866+** and using Microsoft Code with the **VS Cupl** extension.
 -http://www.xgecu.com/en/TL866_main.html
-https://marketplace.visualstudio.com/items?itemName=VaynerSystems.VS-Cupl

If choosing the former, follow Atmel's instructions using the included .pld files. If chosing the latter, you can open the workspace and you will see both projects. You can build and deploy the projects right from Code!

You will need to download the Atmel Wincupl progam either way. If you chose to use VS code, you will also need to download and install minipro. More instructions about requirements can be found in the VS Cupl repo https://github.com/levvayner/ATF15xx-cupl


#### Microcontroller
The project uses an Arduino Due uC. The device is low cost, offers a great deal of pins, and has a low price point.

Another device like the Arduino MEGA may suffice. 
Install Microsoft's Code and install the Platform IO extension.
The project can be found here: https://github.com/levvayner/SRAM-Console

## Building the hardware

You can use a breadboard and follow the schematic to build the circuit. First the clock and counters are wired. Then the PLDs are wired to execute combinatory logic to determine timing signals like V-SYNC, H-SYNC, and others like when the screen is not displaying, marking it safe to write to RAM.

The components used in the schematic are available in the KiCad project's output folder.
Currently they are

| Component                 | Source                                                                                 |
| ------------------------- | -------------------------------------------------------------------------------------- |
| Arduino Due               | https://docs.arduino.cc/hardware/due                                                   |
| Capactitor (0805) 560 pf  | ebay, mouser, digikey                                                                  |
| Capacitor Tantalum        | ebay, mouser, digikey                                                                  |
| 2.54mm pins               | ebay, mouser, digikey                                                                  |
| DSUB 15 pin VGA connector | ebay, mouser, digikey                                                                  |
| USB type A connector      | ebay, mouser, digikey                                                                  |
| Resistors (0805)          | ebay, mouser, digikey                                                                  |
| K6X8008T2B-TF55           | https://www.alldatasheet.com/datasheet-pdf/download/85484/SAMSUNG/K6X8008C2B-TF55.html |
| 74HC245D                  | ebay, mouser, digikey                                                                  |
| 74HC161D                  | ebay, mouser, digikey                                                                  |
| 74HC32D                   | ebay, mouser, digikey                                                                  |
| 74HC08D                   | ebay, mouser, digikey                                                                  |
| 74HC00D                   | ebay, mouser, digikey                                                                  |
| 74HC14D                   | ebay, mouser, digikey                                                                  |
| GAL22V10                  | ebay, mouser, digikey                                                                  |
| 16MHz Full can oscillator | ebay, mouser, digikey                                                                  |

### GALS
The programmable logic devices are not absolutely required. They can be replaced by inveters and a few logic gates to determine when a counter should trigger a signal (e.g. h-sync at count 603) and use the counter inputs to determine if that number is reached. Several flip flops can be used for storing the state e.g. store v-sync bit at frame 610, clear at 620.
However you may want to change timings or the clock speed to accomplish a different resolution or pixel density. It is much easier to reprogram the GALS to change timing than to re-wire these logic gates. If you have finalized your design, chaning to logic gates and flip flops is recommended. Even better you can use a magnitude comperator instad of collection logic gates.

To progam the gals, you may need to erase the chip. Some chips come locked, and the programmer software from XGECU has an option to clear it. If your changes don't seem to take affect after programming, use their software to make sure the chip is not locked.

There are two projects included. One is used for the horizontal timing, the other for vertical.

*There is **VGA_SINGLE_PLD** project that can use an **ATF1504AS** instead of these two, but it has not been verified.*

### Arduino 
The project [VGA Controller](https://github.com/levvayner/SRAM-Console) can be used to interface with and develop the video card further. It has functionality to connect to the controller from its serial port, or connect a PS/2 keyboard (experimental) 



## Roadmap
- Add PS/2 mouse support
- Add second SRAM chip to expand data bus to 16 bit
- Add second SRAM chip to round-robin 2 chips between controller and video output, allowing for controller to read/write to SRAM without interfering with the VGA rendering process.
