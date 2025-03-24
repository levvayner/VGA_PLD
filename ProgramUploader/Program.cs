using System.Runtime.InteropServices;

internal static class Program
{
    public const string ARDUINO_PORT = "/dev/ttyACM0";

    public static int BitmapWidth = 320;
    //public static int BitmapPadWidth = 51;
    public static int BitmapHeight = 300;
    public static int BitmapPadHeight = 6;
    public static bool FillRAM = false;
    private static string InputFile = "";
    
    private static void Main(string[] args)
    {
        bool ReadCommand = false;

        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("ProgramUploader -i file.bin");
            InputFile = "firmware.bin";
            //return;            
        }
        for (int i = 0; args.Length > i; i++)
        {
            if (args[i] == "-i")
            {
                InputFile = args[++i];
            }
            // if (args[i] == "-o")
            // {
            //     OutputFile = args[++i];
            // }
            // if (args[i] == "-p" || args[i] == "--program")
            // {
            //     UseArduinoProxy = true;
            // }

            if (args[i] == "-r" || args[i] == "--read")
            {
                ReadCommand = true;
            }
        }   

        if (ReadCommand)
        {
            ReadRAM();
            return;
        }
        var writer = new DuePort(InputFile);        
        writer.WriteBinary(FillRAM);
        

    }


    private static void ReadRAM(){
         var reader = new DuePort(InputFile);
            Console.WriteLine("Reading...");
            reader.ReadRam();
    }
}