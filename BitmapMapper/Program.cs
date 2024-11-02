using System.Runtime.InteropServices;
using Aspose.Drawing;
using Aspose.Drawing.Imaging;
internal static class Program
{
    private static string InputFile;
    private static string OutputFile;
    private static void Main(string[] args)
    {
        
        if(args.Length < 2){
            Console.WriteLine("Usage:");
            Console.WriteLine("BitmapMapper -i inputfile.bmp [-o outputfile.bin]");
            WriteStripes();
            return;        
        }
        for(int i = 0;args.Length > i;i++){
            if(args[i] == "-i"){
                InputFile = args[++i];
            }
            if(args[i] == "-o"){
                OutputFile = args[++i];
            }
        }
        if(string.IsNullOrEmpty(OutputFile)){
            OutputFile = Path.GetFileNameWithoutExtension(InputFile) + ".bin";
        }
        Console.WriteLine($"Creating {OutputFile} from {InputFile}");

        try{
            Bitmap bmp = new Bitmap(InputFile);
            Bitmap resized = new Bitmap(bmp, 86, 64);
            var length = resized.Height * resized.Width;
            byte[] bytes = new byte[length]; // 1 byte per pixel
            for(int y = 0; y < resized.Height ;y++) {
                for(int x = 0; x < resized.Width;x++) {
                    var color = resized.GetPixel(x,y);
                    bytes[y * resized.Width + x] = (byte)(color.R/32 | color.G/32 << 3 | color.B/64 << 6  );
                }
            }
            
            // BitmapData data = resized.LockBits(new Rectangle(0,0,resized.Width, resized.Height),ImageLockMode.ReadOnly,PixelFormat.Format8bppIndexed);
            // var length = data.Stride * data.Height;

            // byte[] bytes = new byte[length];

            // // Copy bitmap to byte[]
            // Marshal.Copy(data.Scan0, bytes, 0, length);
            // resized.UnlockBits(data);
            // resized.Dispose();
            // bmp.Dispose();

            Console.WriteLine($"Read {length} bytes from {InputFile}. Creating binary...");
            using(BinaryWriter writer = new BinaryWriter(new FileStream(OutputFile, FileMode.OpenOrCreate))){
                writer.Write(bytes);
                writer.Close();
            }

            Console.WriteLine($"Completed writing data to {OutputFile}");
        }
        catch(Exception ex){
            Console.WriteLine("Error occured generating data: " + ex.Message);
        }

    }

    private static void WriteStripes()
    {
        // counter will count until 160 for visible, 211 total horizontally
        // counter will count until 600 for visible, 628 total vertically
        // shifting horizontal one bit, we get 80 and 106 respectivly
        // shifting vertical three bits, we get 75 and 78 respectivly
        // draw horizontal for 80, blank until 106
        // draw vertical for 75, blank until 78

        // var fullWidth = 106;
        // var drawWidth = 80;
        // var fullHeight = 78;
        // var drawHeight = 75;
        
        // var length = fullHeight * fullWidth;
        // byte[] bytes = new byte[length]; // 1 byte per pixel
        // for(int y = 0; y < fullHeight ;y++) {
        //     Console.Write($"{string.Format( "0x{0:X4}", y*fullWidth)}");
        //     for(int x = 0; x < fullWidth;x++) {
        //         var red = 1;
        //         var green = 0;// x > drawWidth/2 && y < drawHeight / 2 ? 1 : 0;
        //         var blue = 0;//x > drawWidth/2 && y > drawHeight / 2 ? 1 : 0;
                
        //         if(x > 5 && x < drawWidth - 10 && y < (drawHeight /2 + 1) && y > (drawHeight /2 - 5))
        //             bytes[y*fullWidth + x] = (byte)(red | green << 3 | blue << 6);
        //         else
        //             bytes[y*fullWidth + x] = 0x0;
                
        //         Console.Write($"{ string.Format( "{0:X2}", bytes[y*fullWidth + x])} ");                    
        //     }
        //     Console.WriteLine();
        // }

        
        var drawWidth = 80;
        var drawHeight = 120;
        
        //var length = drawHeight * drawWidth;
        var length = 32*1024;
        byte[] bytes = new byte[length]; // 1 byte per pixel
        for(int idx = 0; idx < length;) {
            bytes[idx++] = (byte)0;
        }
        
        for(int y = 0; y < drawHeight ;y++) {
            Console.Write($"{string.Format( "0x{0:X4}", y*drawWidth)}");
            for(int x = 0; x < drawWidth;x++) {
                var blockX = x / 5;
                var blockY = y/5;
                var red = blockX;
                var green = blockX / 2;// x > drawWidth/2 && y < drawHeight / 2 ? 1 : 0;
                var blue = blockY;//x > drawWidth/2 && y > drawHeight / 2 ? 1 : 0;
                
                //if(x < drawWidth && y < drawHeight /2 && x == drawWidth / 2 && x > 5 && y > 5)
                //draw border 5 pixels from edges
                var topBorder = x >= 5 && x <= drawWidth - 10 && y == 5;
                var bottomBorder = x > 5 && x <= drawWidth - 10 && y == drawHeight - 10;
                var leftBorder = x == 5 && y >= 5 && y <= drawHeight - 10;
                var rightBorder = x == drawWidth - 10 && y >= 5 && y <= drawHeight - 10;

                if(leftBorder || rightBorder)
                    bytes[y*drawWidth + x] = 1;
                else if (topBorder || bottomBorder)
                    bytes[y*drawWidth + x] = 1 << 3;
                else if(x > 5 && x < drawWidth - 10 && y > 5 && y < drawHeight - 10)
                    bytes[y*drawWidth + x] =(byte)(red | green << 3 | blue << 6);
                
                Console.Write($" { string.Format( "{0:X2}", bytes[y*drawWidth + x])}");                    
            }
            Console.WriteLine();
        }

        using(BinaryWriter writer = new BinaryWriter(new FileStream(string.IsNullOrEmpty(OutputFile) ? "colors.bin" : OutputFile, FileMode.Create))){
            writer.Write(bytes);
            writer.Close();
        }
    }
}