using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsApp1
{
    internal static class Program
    {
        private const int WH_MOUSE_LL = 0x000E;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MBUTTONDBLCLK = 0x0209;
        private const int WM_CLICK = 0xF5;
        private const int WM_SETTEXT = 0x000C;
        private const int WM_CHAR = 0x0102;
        

        private static LowLevelKeyboardProc _procKeyBoard = HookKeyBoardCallback;
        private static LowLevelKeyboardProc _procMouse = HookMouseCallback;
        
        private static IntPtr _hookIDKeyBoard = IntPtr.Zero;
        private static IntPtr _hookIDMouse = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelKeyboardProc proc, int type = 0)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(type == 0 ? WH_KEYBOARD_LL : WH_MOUSE_LL, proc,
                    type == 0 ? GetModuleHandle(curModule.ModuleName) : GetModuleHandle(null), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }
        private static IntPtr HookMouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //MSLLHOOKSTRUCT mouseLowLevelHook = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            //POINT point = mouseLowLevelHook.pt;
            string event_name = "mouse.move";
            if (wParam == (IntPtr)WM_MOUSEMOVE)
            {
                event_name = "mouse.move";
            }
            /*else if (wParam == (IntPtr)WM_KEYUP)
            {
                event_name = "key.up";
            }*/
            //int vkCode = Marshal.ReadInt32(lParam);
            //Console.WriteLine(event_name + ":" + point.X + "," + point.Y);
            Console.WriteLine(event_name + ":aaa");
            return CallNextHookEx(_hookIDMouse, nCode, wParam, lParam);
        }
        private static IntPtr HookKeyBoardCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            string event_name = "";
            if(wParam == (IntPtr)WM_KEYDOWN)
            {
                event_name = "key.down";
            } else if (wParam == (IntPtr)WM_KEYUP)
            {
                event_name = "key.up";
            }
            int vkCode = Marshal.ReadInt32(lParam);
            Console.WriteLine(event_name + ":" + (Keys)vkCode + "," + vkCode);
            return CallNextHookEx(_hookIDKeyBoard, nCode, wParam, lParam);
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //hook keyboar
            if(args.Length == 0)
            {
                _hookIDKeyBoard = SetHook(_procKeyBoard, 0);
                _hookIDMouse = SetHook(_procMouse, 1);
            } else
            {
                var argument = args[0].ToLowerInvariant();
                Console.WriteLine("argument: " + argument);
                string text = args[1];
                IntPtr hwnd = IntPtr.Parse(args[2]);
                switch (argument)
                {
                    case "--sendtext":
                        Console.WriteLine("text: " + text);
                        Console.WriteLine("hwnd: " + hwnd);
                        SendMessageW(hwnd, WM_SETTEXT, 0, text);
                        Console.WriteLine("end");
                        break;
                    case "--sendchar":
                        int timeout = Int32.Parse(args[3]);
                        Console.WriteLine("text: " + text);
                        Console.WriteLine("hwnd: " + hwnd);
                        Console.WriteLine("timeout: " + timeout);
                        string[] texts = text.Split(";");
                        Console.WriteLine("texts " + texts.Length);

                        for (int i = 0; i < texts.Length; i++)
                        {
                            int cha = Int32.Parse(texts[i]);
                            Console.WriteLine("char:" + cha);

                            SendMessageA(hwnd, 224, cha, 0);
                            //Thread.Sleep(timeout);
                        }
                        Console.WriteLine("end");
                        break;
                }
            }
            
            
            //getting notepad's process | at least one instance of notepad must be running
            //Process notepadProccess = Process.GetProcessesByName("notepad")[0];

            //getting notepad's textbox handle from the main window's handle
            //the textbox is called 'Edit'
            //IntPtr notepadTextbox = FindWindowEx(notepadProccess.MainWindowHandle, IntPtr.Zero, "Edit", null);
            //Debug.WriteLine("notepadTextbox " + notepadTextbox);
            //sending the message to the textbox
            ///SendMessage(notepadTextbox.ToInt32(), WM_SETTEXT, 0, "This is the new Text!!!!!");
            Application.Run();
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessageW(IntPtr hWnd, int wMsg, int wParam, string lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessageA(IntPtr hWnd, int wMsg, int wParam, int lParam);
        //include FindWindowEx
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    }
}