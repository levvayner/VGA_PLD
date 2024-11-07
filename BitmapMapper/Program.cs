using System.Runtime.InteropServices;
using Aspose.Drawing;
using Aspose.Drawing.Imaging;
internal static class Program
{
    public const string ARDUINO_PORT = "/dev/ttyACM0";

    public static int BitmapWidth = 160;
    public static int BitmapPadWidth = 51;
    public static int BitmapHeight = 255;
    public static int BitmapPadHeight = 6;
    public static bool FillRAM = true;
    private static string InputFile;
    private static string OutputFile;
    private static bool UseArduinoProxy = false;
    
    private static void Main(string[] args)
    {
        bool ReadCommand = false;

        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("BitmapMapper -i inputfile.bmp [-o outputfile.bin]");
            //CreateImageBin(800,600);
            InputFile = "puppy.png";
            UseArduinoProxy = true;
            WriteStripes();
            return;
            //ReadRAM();
        }
        for (int i = 0; args.Length > i; i++)
        {
            if (args[i] == "-i")
            {
                InputFile = args[++i];
            }
            if (args[i] == "-o")
            {
                OutputFile = args[++i];
            }
            if (args[i] == "-p" || args[i] == "--program")
            {
                UseArduinoProxy = true;
            }

            if (args[i] == "-r" || args[i] == "--read")
            {
                ReadCommand = true;
            }
        }
        if (string.IsNullOrEmpty(InputFile))
        {
            InputFile = "colors.bin";
        }
        if (string.IsNullOrEmpty(OutputFile))
        {
            OutputFile = Path.GetFileNameWithoutExtension(InputFile) + ".bin";
        }

        if (ReadCommand)
        {
            ReadRAM();
            return;
        }
        Console.WriteLine($"Creating {OutputFile} from {InputFile}");
        var created = CreateImageBin(BitmapWidth, BitmapHeight);

        if (UseArduinoProxy && created)
        {
            //progam using arudino device

            var reader = new PortReader(OutputFile);
            reader.WriteBinary(FillRAM);
        }

    }

    private static bool CreateImageBin(int width = 160, int height = 120)
    {
        try
        {
            Bitmap bmp = new Bitmap(InputFile);
            Bitmap resized = new Bitmap(bmp, width, height);
            var length = resized.Height  * 256 ;
            byte[] bytes = new byte[length]; // 1 byte per pixel
            for (int y = 0; y < resized.Height; y++)
            {
                Console.WriteLine("Resizing row " + y);
                for (int x = 0; x < resized.Width; x++)
                {
                    var color = resized.GetPixel(x, y);
                    bytes[(y << 7) + x] = (byte)(((color.R/ 32) & 0x3) | ((color.G / 32) & 0x3) << 3 | (color.B / 64) << 6);
                }
            }
            Console.WriteLine($"Resized bitmap to {width}x{height}");

            // BitmapData data = resized.LockBits(new Rectangle(0,0,resized.Width, resized.Height),ImageLockMode.ReadOnly,PixelFormat.Format8bppIndexed);
            // var length = data.Stride * data.Height;

            // byte[] bytes = new byte[length];

            // // Copy bitmap to byte[]
            // Marshal.Copy(data.Scan0, bytes, 0, length);
            // resized.UnlockBits(data);
            // resized.Dispose();
            // bmp.Dispose();

            Console.WriteLine($"Read {length} bytes from {InputFile}. Creating binary...");
            using (BinaryWriter writer = new BinaryWriter(new FileStream(OutputFile, FileMode.Create)))
            {
                writer.Write(bytes);
                writer.Close();
            }

            Console.WriteLine($"Completed writing data to {OutputFile}");
            resized.Save(Path.GetFileNameWithoutExtension(OutputFile) + "Resized.bmp");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occured generating data: " + ex.Message + "\n" + ex.StackTrace);
            return false;
        }
    }

    private static void WriteStripes()
    {
        
        //var length = drawHeight * drawWidth;
        var length = 64*1024;
        byte[] bytes = new byte[length]; // 1 byte per pixel
        for(int idx = 0; idx < length;) {
            bytes[idx++] = (byte)0;
        }
        
        for(int y = 0; y < BitmapHeight ;y++) {
            //Console.Write($"{string.Format( "0x{0:X4}", y*drawWidth)}");
            for(int x = 0; x < BitmapWidth;x++) {
                // var blockX = x / 5;
                // var blockY = y/5;
                var red = y < (BitmapHeight / 2) ? 1 : 0;
                var green = 0;// x > drawWidth/2 && y < drawHeight / 2 ? 1 : 0;
                var blue = x > BitmapWidth/2 && y > BitmapHeight / 2 ? 1 : 0;
                
                //if(x < drawWidth && y < drawHeight /2 && x == drawWidth / 2 && x > 5 && y > 5)
                //draw border 5 pixels from edges
                var topBorder = x >= 5 && x <= BitmapWidth - 10 && y == 5;
                var bottomBorder = x > 5 && x <= BitmapWidth - 10 && y == BitmapHeight - 10;
                var leftBorder = x == 5 && y >= 5 && y <= BitmapHeight - 10;
                var rightBorder = x == BitmapWidth - 10 && y >= 5 && y <= BitmapHeight - 10;

                if(leftBorder || rightBorder)
                    bytes[y*256 + x] = 1;
                else if (topBorder || bottomBorder)
                    bytes[y*256 + x] = 1 << 3;
                else if(x < BitmapWidth && y < BitmapHeight)
                    bytes[y*256 + x] =(byte)(red | green << 3 | blue << 6);
                
                //Console.Write($" { string.Format( "{0:X2}", bytes[y*drawWidth + x])}");                    
            }
            //Console.WriteLine();
        }
        OutputFile = string.IsNullOrEmpty(OutputFile) ? "colors.bin" : OutputFile;
        using(BinaryWriter writer = new BinaryWriter(new FileStream(OutputFile, FileMode.Create))){
            writer.Write(bytes);
            writer.Close();
        }

        if(UseArduinoProxy){
            //progam using arudino device

            var reader = new PortReader(OutputFile);
            reader.WriteBinary(FillRAM);

            Console.WriteLine("Reading...");

            reader.ReadRam();
            
        }
    }

    private static void ReadRAM(){
         var reader = new PortReader(OutputFile);
            Console.WriteLine("Reading...");
            reader.ReadRam();
    }
}