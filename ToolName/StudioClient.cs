using System;
using System.IO;
using System.IO.Pipes;
using Microsoft.Win32;
using System.Diagnostics;

/**
* This file is provided as-is for any use.
* No accountability or support is guaranteed.
**/

namespace ToolName
{
    public class StudioClient
    {
        /** Starts the studio client and returns a client, or null if it was unable. May wait for up to 5s. **/
        public static StudioClient StartClient()
        {
            return new StudioClient(5);
        }

        /** Starts the most recently run ParamStudio if it is not already running. Typically call this before calling start client. Returns true if ParamStudio is running. **/
        public static bool StartParamStudio()
        {
            RegistryKey rkey = Registry.LocalMachine.CreateSubKey($@"Software\DSParamStudio");
                var v = rkey.GetValue("executable");
                if (v != null)
                {
                    string exe = v.ToString();
                    var a = Path.GetFileNameWithoutExtension(exe);
                    Process[] procs = Process.GetProcessesByName(a);
                    if (procs.Length == 0){
                        Process.Start(new ProcessStartInfo(exe));
                        procs = Process.GetProcessesByName(a);
                    }
                    if (procs.Length == 0)
                        return false;
                    return true;
                }
                return false;
        }

        /** Creates a new client, sends a request, and closes the client. **/
        public static bool OpenParamSingleUse(bool newView, string param, string row)
        {
            if (!StartParamStudio())
                return false;
            StudioClient c = StartClient();
            bool success = false;
            if (c != null)
                success = c.OpenParam(newView, param, row);
            c.CloseClient();
            return success;
        }

        /** Opens the named param, and if given, row.**/
        public bool OpenParam(bool newView, string param, string row)
        {
            string view = newView?"new":"-1";
            if (row == null)
                return sendMessage($@"param/view/{view}/{param}");
            else
                return sendMessage($@"param/view/{view}/{param}/{row}");
        }
        
        /** Closes the client's connection, if it is still alive **/
        public void CloseClient()
        {
            try 
            {
                clientWriter.Flush();
                clientStream.Close();
            }
            catch (Exception e)
            {
            }
        }

        internal NamedPipeClientStream clientStream = null;
        internal StreamReader clientReader;
        internal StreamWriter clientWriter;

        internal StudioClient(int timeout)
        {
            clientStream = new NamedPipeClientStream($@"\.\DSParamStudio\pipe\CommandQueue");
            clientStream.Connect(timeout);
            clientReader = new StreamReader(clientStream);
            clientWriter = new StreamWriter(clientStream);
        }

        internal bool sendMessage(string msg)
        {
            try
            {
                clientWriter.WriteLine(msg);
                clientWriter.Flush();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}