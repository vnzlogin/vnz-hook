using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace windows_console_app
{
    public class Output
    {
        public string Error { get; set; }

        public object Result { get; set; }
    }

    public class ProcessInfo
    {
        public int ProcessId { get; set; }

        public string MainWindowTitle { get; set; }

        public string ProcessName { get; set; }
    }
    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                List<string> list = new List<string>();
                list.Add("--event");
                args = list.ToArray();
            }
            //throw new ArgumentException("Please specify an argument: --processInfo, --focus <pid>, --activewindow");

            var argument = args[0].ToLowerInvariant();

            var output = new Output();

            try
            {
                switch (argument)
                {
                    case "--activewindow":
                        output.Result = GetActiveProcessInfo();
                        Console.WriteLine(JsonConvert.SerializeObject(output));
                        break;
                    case "--processinfo":
                        output.Result = GetProcessInfo();
                        Console.WriteLine(JsonConvert.SerializeObject(output));
                        break;
                    case "--focus":
                        if (argument.Length < 2)
                            throw new ArgumentException("--focus requires a processId");

                        FocusHandler.SwitchToWindow(Int32.Parse(args[1]));
                        output.Result = true;
                        Console.WriteLine(JsonConvert.SerializeObject(output));
                        break;
                    case "--getmodule":
                        using (Process curProcess = Process.GetCurrentProcess())
                        using (ProcessModule curModule = curProcess.MainModule)
                        {
                            output.Result = GetModuleHandle(curModule.ModuleName);
                        }
                        Console.WriteLine(JsonConvert.SerializeObject(output));
                        break;
                    case "--event":
                        _output = output;
                        LowLevelKeyboardProc myCallBack = new LowLevelKeyboardProc(HookCallback);
                        Process _curProcess = Process.GetCurrentProcess();
                        ProcessModule _curModule = _curProcess.MainModule;

                        //using (Process curProcess = Process.GetCurrentProcess())
                        //using (ProcessModule curModule = curProcess.MainModule)
                        //{
                        //    Console.WriteLine("xxx " + curModule.ModuleName);
                        //Debug.WriteLine(GetModuleHandle(curModule.ModuleName));
                        //Debug.WriteLine(curModule.ModuleName);
                        _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, myCallBack,
                            GetModuleHandle(_curModule.ModuleName), 0);
                        //}
                        //SetHook(output);
                        //output.Result = true;
                        Console.Read();
                        //Debug.WriteLine("_hookID " + _hookID);
                        //DoWork();
                        break;
                    default:
                        throw new ArgumentException("Unknonw argument: " + argument);
                }

            }
            catch(Exception ex)
            {
                output.Error = ex.ToString();
                Console.WriteLine(JsonConvert.SerializeObject(output));
            }

            
        }
        private static void DoWork()
        {
            while (!_shouldStop)
            {
                //Console.WriteLine("worker thread: working...");
            }
            //Console.WriteLine("worker thread: terminating gracefully.");
        }
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookID = IntPtr.Zero;
        private static bool _shouldStop = false;
        static Output _output;
        private static void SetHook(Output output)
        {
            _output = output;
            LowLevelKeyboardProc myCallBack = new LowLevelKeyboardProc(HookCallback);
            Process curProcess = Process.GetCurrentProcess();
            ProcessModule curModule = curProcess.MainModule;

            //using (Process curProcess = Process.GetCurrentProcess())
            //using (ProcessModule curModule = curProcess.MainModule)
            //{
            //    Console.WriteLine("xxx " + curModule.ModuleName);
                //Debug.WriteLine(GetModuleHandle(curModule.ModuleName));
                //Debug.WriteLine(curModule.ModuleName);
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, myCallBack,
                    GetModuleHandle(curModule.ModuleName), 0);
            //}
        }
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            /*if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Debug.WriteLine((Keys)vkCode, vkCode + "");
            }*/
            /*var myData = new
            {
                nCode = nCode,
                wParam = wParam,
                lParam = lParam
            };
            
            // Transform it to JSON object
            string jsonData = JsonConvert.SerializeObject(myData);
            Debug.WriteLine(jsonData);
            Console.WriteLine(jsonData);*/
            //Debug.WriteLine("xxxxx " + nCode);
            Console.WriteLine("xxxxx " + nCode);
            _output.Result = nCode + " ";
            Console.WriteLine(JsonConvert.SerializeObject(_output));
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        private static ProcessInfo[] GetProcessInfo()
        {
            var processes = Process.GetProcesses();
            return ConvertToProcessInfo(processes);
        }

        private static ProcessInfo GetActiveProcessInfo()
        {
            var processes = Process.GetProcesses();
            var activeForegroundWindow = NativeMethods.GetForegroundWindow();

            var activeWindows = processes.Where(p => p.MainWindowHandle == activeForegroundWindow);

            if (activeWindows.Count() < 1)
            {
                return null;
            }
            else
                return ConvertToProcessInfo(activeWindows)[0];
        }

        private static ProcessInfo[] ConvertToProcessInfo(IEnumerable<Process> processes)
        {
            return processes
                    .Select(process =>
                    {
                        string title = process.MainWindowTitle;
                        if (title.Contains("{"))
                        {
                            title = "";
                        }
                        return new ProcessInfo() { MainWindowTitle = title, ProcessId = process.Id, ProcessName = process.ProcessName };
                    })
                    .ToArray();
        }
    }
}
