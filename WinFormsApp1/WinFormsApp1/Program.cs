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
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //hook keyboar
            _hookIDKeyBoard = SetHook(_procKeyBoard, 0);
            _hookIDMouse = SetHook(_procMouse, 1);

            Application.Run();
            
        }
    }
}