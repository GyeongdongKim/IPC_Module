using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;


namespace IPC_Module
{
    public class Matlab
    {
        private static StreamString ss_in;
        private static StreamString ss_out;
        private static NamedPipeServerStream pipeServer;
        private static NamedPipeClientStream pipeClient;
        private static bool isServerConnected=false, isClientConnected=false;
        public static void init()
        {
            pipeServer = new NamedPipeServerStream("MatlabServer", PipeDirection.Out, 1);
            
            pipeServer.WaitForConnection();

            ss_out = new StreamString(pipeServer);
            isServerConnected = true;
        }
        public static void connectToUnity(int waitTime)
        {

            pipeClient = new NamedPipeClientStream(".", "UnityServer", PipeDirection.In, PipeOptions.None, TokenImpersonationLevel.None);
            pipeClient.Connect(waitTime);
            if (pipeClient.IsConnected)
            {
                ss_in = new StreamString(pipeClient);
                isClientConnected = true;
            }
                
        }
        public static bool isMatlabConnected()
        {
            return isServerConnected;
        }
        public static bool isConnectedWithUnity()
        {
            return isClientConnected;
        }

        public static void disconnectWithUnity()
        {
            pipeClient.Close();
            pipeClient.Dispose();
            isClientConnected = false;
        }

        public static void stop()
        {
            if(pipeServer.IsConnected)
                pipeServer.Disconnect();
            pipeServer.Close();
            pipeServer.Dispose();
            isServerConnected = false;
        }
        public static double[] readData()
        {
            return ss_in.ReadString();
        }
        public static void sendData(double[] data)
        {
            ss_out.WriteString(data);
        }
    }
    public class Unity
    {
        private static StreamString ss_in;
        private static StreamString ss_out;
        private static NamedPipeServerStream pipeServer;
        private static NamedPipeClientStream pipeClient;
        private static bool isServerConnected = false, isClientConnected = false;

        public static void init()
        {
            pipeServer = new NamedPipeServerStream("UnityServer", PipeDirection.Out, 1);

            pipeServer.WaitForConnection();

            ss_out = new StreamString(pipeServer);
            isServerConnected = true;
        }
        public static void connectToMatlab(int waitTime)
        {

            pipeClient = new NamedPipeClientStream(".", "MatlabServer", PipeDirection.In, PipeOptions.None, TokenImpersonationLevel.None);
            pipeClient.Connect(waitTime);
            if (pipeClient.IsConnected)
            {
                ss_in = new StreamString(pipeClient);
                isClientConnected = true;
            }
                
        }
        public static bool isUnityConnected()
        {
            return isServerConnected;
        }

        public static bool isConnectedWithMatlab()
        {
            return isClientConnected;
        }

        public static void disconnectWithMatlab()
        {
            pipeClient.Close();
            pipeClient.Dispose();
            isClientConnected = false;
        }
        public static void stop()
        {
            if (pipeServer.IsConnected)
                pipeServer.Disconnect();
            pipeServer.Close();
            pipeServer.Dispose();
            isServerConnected = false;
        }
        public static double[] readData()
        {
            return ss_in.ReadString();
        }
        public static void sendData(double[] data)
        {
            ss_out.WriteString(data);
        }
    }
    public class StreamString
    {
        private Stream ioStream;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

        public double[] ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return GetDoublesAlt(inBuffer);
        }

        public int WriteString(double[] outData)
        {
            byte[] outBuffer = GetBytesAlt(outData);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
        static byte[] GetBytesAlt(double[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }
        static double[] GetDoublesAlt(byte[] bytes)
        {
            var result = new double[bytes.Length / sizeof(double)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }
    }

}
