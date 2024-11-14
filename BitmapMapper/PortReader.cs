using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class PortReader
{
    private static SerialPort port = new SerialPort(Program.ARDUINO_PORT);
    private static byte[] buffer = new byte[256];
    private static int runNumber = 0;
    string workingDir;
    private static string FileName;
        public PortReader(string fileName)
        {
            port.BaudRate = 115200;// 1843200;
            FileName = fileName;
            port.ReadBufferSize = 256;
            port.ReadTimeout = 4000;
            
            runNumber = new Random(DateTime.Now.Hour).Next(100000, 9000000);
            workingDir = Path.Combine(Environment.CurrentDirectory, runNumber.ToString());
        }
    public void WriteBinary(bool pad = false)
    {
        try
        {
            if(!port.IsOpen)
                port.Open();
            int retries = 0;

            List<byte> data = new List<byte>(pad ? 64*1024 : 0);
            using (BinaryReader reader = new BinaryReader(new StreamReader(FileName).BaseStream))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    data.Add(reader.ReadByte());
            }
            if(pad) 
            for(int start = data.Count; start < 64*1024; start++)
                data.Add(0);
            //data.Add(0x0); //null terminator for file

            //port.Write(new[] { (byte)'c' },0,1);
            var outStream = port.BaseStream;
            EstablishProgrammingMode(outStream);
            
                            
            string byteCount = data.Count.ToString();
            Array.Copy(Encoding.ASCII.GetBytes(byteCount), buffer, byteCount.Length);
            Console.WriteLine("Passing byte count as {0}", Convert.ToString(data.Count, 10));
            //buffer[0] = (byte)((byte)(data.Count >> 8) & 0xFF);
            //buffer[1] = (byte)(data.Count & 0xFF);
            outStream.Write(buffer, 0, byteCount.Length);
            ReadBytes(1, outStream);
            
            //Thread.Sleep(100);
            //string retVal;
            int lineNum = 0;
            int frameSize = port.ReadBufferSize;
            int numberOfFrames = (int)Math.Ceiling(data.Count / (decimal)frameSize);

            Console.WriteLine("Passing frame size as {0}", frameSize);
            Array.Copy(Encoding.ASCII.GetBytes(frameSize.ToString()), buffer, frameSize.ToString().Length);
            outStream.Write(buffer, 0, frameSize.ToString().Length);
            ReadBytes(3, outStream,7);
            Console.WriteLine("Established frame size.. writting program");

            int mark = data.Count / 100;
            //byte[] dataFrame = new byte[frameSize];
            for (int frameNum = 0; frameNum < numberOfFrames; frameNum++)
            {
                if(retries > 3){
                    Console.WriteLine("Failed to upload data. Too many retries!");
                    return;
                }

                int acutalSize = data.Count - lineNum > frameSize ? frameSize : data.Count - lineNum;
                Array.Copy(data.Skip(frameNum*frameSize).Take(acutalSize).ToArray(), buffer, acutalSize);

                Console.WriteLine($"Uploading frame { frameNum + 1 } of { numberOfFrames}. {acutalSize} bytes");
                for(int frameIdx = 0; frameIdx < acutalSize; frameIdx++){
                    Console.Write(buffer[frameIdx].ToString("X2"));
                    Console.Write(" ");
                }
                Console.WriteLine();
                
                
                
                outStream.Write(buffer, 0, acutalSize);
                
                var result = ReadBytes(0, outStream);                    
                if(result.Any(l => l.Contains("Failed to read from serial."))){
                    Console.WriteLine("Failed to read. retrying");
                    lineNum -= frameSize; //retry;
                    retries++;                        
                }

                ////if (data[lineNum] == 0) continue;
                //dataFrame[lineNum % frameSize] = data[lineNum];
                //if ((lineNum % frameSize == 0 && lineNum > 0) || (lineNum == data.Count - 1 && data.Count  % frameSize!= 0))
                //{
                //    if (lineNum % frameSize != 0)
                //        frameSize = lineNum % frameSize;
                //    //write out frame
                //    outStream.Write(dataFrame, 0, frameSize);
                //    Thread.Sleep(100);
                //    //ReadBytes(frameSize, outStream);
                //    ReadBytes(frameSize, outStream);
                //}
                

                //Console.WriteLine("Writing 0x{0:X} at address 0x{1:X}", Convert.ToInt32(data[lineNum]), Convert.ToInt32(lineNum));
                // buffer[0] = data[lineNum];
                // buffer[1] = 10;
                

                // outStream.Write(buffer,0,1);
                // if(data[lineNum] != 0)
                //     ReadBytes(1, outStream);
                // else
                //     Thread.Sleep(1);

                //dataFrame[lineNum % frameSize] = data[lineNum % frameSize];
                //Array.Copy(Encoding.ASCII.GetBytes(lineNum.ToString().PadLeft(4, '0')), buffer, 4);
                //Array.Copy(Encoding.ASCII.GetBytes(data[lineNum].PadLeft(3, '0')),0, buffer, 4,3);
                //buffer[0] = ((byte)((byte)(lineNum >> 8) & 0xFF));
                //buffer[1] = ((byte)(lineNum & 0xFF));
                //buffer[2] = ((byte)((byte)(Convert.ToInt32(data[lineNum])) & 0xFF));

                //Console.WriteLine("Chars are {0}, {1}, {2}",(int)buffer[0], (int)buffer[1], (int)buffer[2]);
                //Thread.Sleep(10);
                //retVal = ReadBytes(1, outStream)[0];
                //if (!retVal.StartsWith("<<")) lineNum--; // retry
                //Thread.Sleep(15);
                if(lineNum % mark == 0 && lineNum > 0){
                    Console.Write(".");
                }
            }

            Thread.Sleep(50); // give it a sec to flush out
            port.DiscardOutBuffer();
            port.Close();
            Console.WriteLine("Completed upload of binary to SRAM. Data has not been validated!");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            //throw e;

        }
    }

    void EstablishProgrammingMode(Stream outStream)
    {

        //var outStream = port.BaseStream;
        ReadBytes(15, outStream); //get first ten into lines   

        Console.WriteLine("---------------------");
        //Console.Write(">> ");
        Array.Copy(Encoding.ASCII.GetBytes("S"), buffer, 1);
        outStream.Write(buffer, 0, 1);
        ReadBytes(1, outStream);
        Console.WriteLine("Established mode as write.. setting type to programming");

        
        Thread.Sleep(50);
        
        Array.Copy(Encoding.ASCII.GetBytes("START_PROGRAM"), buffer, 13);
        outStream.Write(buffer, 0, 13);
        ReadBytes(1, outStream);            
        Console.WriteLine("Established handshake from remote server.. ready to write program ");            
    }

    private string[] ReadBytes(int linesToRead,Stream outStream, int timeout = 5, bool throwErrorOnEmpty = false)
        {
            //retVal = Read(outStream);
            System.Threading.Thread.Sleep(10);
            int retLineCount = 0;
            List<string> it = new List<string>();
            DateTime starTime = DateTime.Now;
            while (retLineCount < linesToRead && DateTime.Now.Subtract(starTime).TotalSeconds < timeout) // max 2 sec for timeout
            {
                try{
                if (port.BytesToRead > 0)
                {
                    it.Add(port.ReadLine());
                    Console.Write(">> ");
                    Console.WriteLine(it[it.Count - 1]);
                    Thread.Sleep(10);
                    retLineCount++;
                }
                }catch(TimeoutException ex){
                    Console.WriteLine("Read error: timeout. " + ex.Message);
                }
            }
            if (retLineCount == 0 && throwErrorOnEmpty)
                throw new EndOfStreamException("Failed to receive data validation from client! See Inner Exception for line where error occured",new Exception(retLineCount.ToString()));

            //    it.Add("");

            return it.ToArray();
            
        }



    internal void ReadRam()
    {
        if(!port.IsOpen)
            port.Open();
        var outStream = port.BaseStream;

        //var outStream = port.BaseStream;
        ReadBytes(9, outStream); //get first ten into lines  

        int readBytes = 16*64;

                
        Array.Copy(Encoding.ASCII.GetBytes("P"), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        var dumpingRamMessage = ReadBytes(1, outStream);   
        
        // start addr
        Array.Copy(Encoding.ASCII.GetBytes("0"), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        ReadBytes(0, outStream);  
        // end addr
        //Array.Copy(Encoding.ASCII.GetBytes("32768"), buffer, 1);
        Array.Copy(Encoding.ASCII.GetBytes(readBytes.ToString()), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        ReadBytes(0, outStream);  

        Thread.Sleep(50);
        //16 bytes per line, extra space every 16 
        ReadBytes(readBytes/16 + (readBytes)/16/32, outStream, 120);
        
    }

    internal void EraseRam()
    {
        if(!port.IsOpen)
            port.Open();
        var outStream = port.BaseStream;

        //var outStream = port.BaseStream;
        ReadBytes(9, outStream); //get first ten into lines  

        int readBytes = 16*64;

                
        Array.Copy(Encoding.ASCII.GetBytes("P"), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        var dumpingRamMessage = ReadBytes(1, outStream);   
        
        // start addr
        Array.Copy(Encoding.ASCII.GetBytes("0"), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        ReadBytes(0, outStream);  
        // end addr
        //Array.Copy(Encoding.ASCII.GetBytes("32768"), buffer, 1);
        Array.Copy(Encoding.ASCII.GetBytes(readBytes.ToString()), buffer, 1);
        outStream.Write(buffer, 0, 1);     
        ReadBytes(0, outStream);  

        Thread.Sleep(50);
        //16 bytes per line, extra space every 16 
        ReadBytes(readBytes/16 + (readBytes)/16/32, outStream, 120);
        
    }
}
    