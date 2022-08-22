using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace ConsoleApp1
{
    class InputManager
    {
        private KeyboardSimulator ks = new KeyboardSimulator(new InputSimulator());

        private string ss09 = ")!@#$%^&*("; // special string 0 - 9

        [DllImport("USER32.DLL")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private IKeyboardSimulator KeyWithShift(IKeyboardSimulator kbs, VirtualKeyCode vkc)
        {
            kbs.KeyDown(VirtualKeyCode.SHIFT);
            kbs.KeyPress(vkc);
            kbs.KeyUp(VirtualKeyCode.SHIFT);
            return kbs;
        }

        public static Process? FindProcessByName(string pName = "PanGPA")
        {
            foreach (Process prs in Process.GetProcesses())
            {
                var s = prs.ProcessName;
                if (s.Contains(pName))
                {
                    return prs;
                }
            }
            return null;
        }

        public void TriggerGlobalProtect()
        {
            var p = new Process();
            p.StartInfo.FileName = @"C:\Program Files\Palo Alto Networks\GlobalProtect\PanGPA.exe";
            p.StartInfo.WorkingDirectory = @"C:\Program Files\Palo Alto Networks\GlobalProtect\";
            p.Start();
        }

        public bool OpenTray()
        {
            var pName = "PanGPA";
            var tray_prs = FindProcessByName(pName);   // find process
            if (tray_prs != null)
            {
                tray_prs.Kill();
                Console.WriteLine($"Kill process  : {pName}");
                this.ks.Sleep(2000);
            }
            Console.WriteLine($"Launch process :{pName}");
            this.TriggerGlobalProtect();
            this.ks.Sleep(2000);
            this.TriggerGlobalProtect();  // trigger twice, to open the small window
            tray_prs = FindProcessByName(pName);

            if (tray_prs == null)
            {
                Console.WriteLine($"Not found process {pName}");
                return false;
            }
            var hWnd = tray_prs.MainWindowHandle;
            SetForegroundWindow(hWnd);
            this.ks.KeyPress(VirtualKeyCode.SPACE);  // click the "Connect" button
            Console.WriteLine($"Found process  :{pName}. PID: {tray_prs.Id}. Click connect...");

            this.ks.Sleep(8000);  // sleep longer, in case it not pop up the dialog
            return true;
        }

        /**
         * Get Email and password from file.
         */
        public bool GetCredential(string filepath, ref string email, ref string pword)
        {
            if (!File.Exists(filepath))
            {
                Console.WriteLine($"Error: File not found: {filepath}");
                Console.WriteLine($"Current work dir: {Directory.GetCurrentDirectory()}");
                return false;
            }
            string[] lines = File.ReadAllLines(filepath);
            string _email = string.Empty;
            string _pword = string.Empty;
            foreach (string line in lines)
            {
                var l = line.Trim();
                if (string.IsNullOrWhiteSpace(l))
                    continue;
                if (l.StartsWith("#"))
                    continue;
                var arr = l.Split(':', 2);  // the line could be: email:aaa@bbb.ccc
                if (arr.Length != 2)
                {
                    Console.WriteLine($"Error: invalid line: {line} in file {filepath}");
                    return false;
                }
                if (arr[0].ToLower() == "email")
                {
                    _email = arr[1];
                }
                if (arr[0].ToLower() == "password" || arr[0].ToLower() == "pword")
                {
                    _pword = arr[1];
                }

            }
            if (string.IsNullOrEmpty(_email))
            {
                Console.WriteLine($"Not found email setting in {filepath}");
                return false;
            }
            if (string.IsNullOrEmpty(_pword))
            {
                Console.WriteLine($"Not found password setting in {filepath}");
                return false;
            }
            email = _email;
            pword = _pword;
            return true;
        }

        public string Run(string filepath)
        {
            string email = String.Empty;
            string pword = String.Empty;
            if (!this.GetCredential(filepath, ref email, ref pword))
            {
                Console.WriteLine("Failed to get credential (Email & password). Will exit.");
                return String.Empty;
            }

            if (!this.OpenTray())
            {
                Console.WriteLine("Open tray failed. Will exit.");
                return String.Empty;
            }
            Console.WriteLine("input email...");
            this.InputString(email);
            this.ks.KeyPress(VirtualKeyCode.TAB);
            this.ks.Sleep(1000);
            Console.WriteLine("input password...");
            this.InputString(pword);
            this.ks.KeyPress(VirtualKeyCode.TAB);
            this.ks.Sleep(1000);
            this.ks.KeyPress(VirtualKeyCode.SPACE); // click the "Sign In" button

            return email;
        }

        /**
         * ref: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-6.0
         */
        public void InputString(string str)
        {
            int sec = 3;
            Console.WriteLine($"Sleep {sec} seconds");
            ks.Sleep(sec * 1000);
            foreach (char c in str)
            {
                this.ks.Sleep(100); // sleep for short time. the UI experience will be better.
                int i = c;          // ascii
                //Console.WriteLine($"{c} ==> {i}");
                if (48 <= i && i <= 57)
                    ks.KeyPress((VirtualKeyCode)i);             // 0 ~ 9
                else if (65 <= i && i <= 90)
                    KeyWithShift(ks, (VirtualKeyCode)i);        // A ~ Z
                else if (97 <= i && i <= 122)
                    ks.KeyPress((VirtualKeyCode)(i - 32));      // a ~ z
                else if (c == ' ')
                    ks.KeyPress(VirtualKeyCode.SPACE);          // whitespace
                else if (c == ',')
                    ks.KeyPress(VirtualKeyCode.OEM_COMMA);      // ,
                else if (c == '<')
                    KeyWithShift(ks, VirtualKeyCode.OEM_COMMA); // <
                else if (c == '.')
                    ks.KeyPress(VirtualKeyCode.OEM_PERIOD);     // .
                else if (c == '>')
                    KeyWithShift(ks, VirtualKeyCode.OEM_PERIOD); // >
                else if (c == '/')
                    ks.KeyPress(VirtualKeyCode.OEM_2);          // /
                else if (c == '?')
                    KeyWithShift(ks, VirtualKeyCode.OEM_2);     // ?
                else if ((i = ss09.IndexOf(c)) >= 0)
                    KeyWithShift(ks, VirtualKeyCode.VK_0 + i);  // ! @ # $ % ^ & * ( )
                else if (c == '-')
                    ks.KeyPress((VirtualKeyCode)189);           // -
                else if (c == '_')
                    KeyWithShift(ks, (VirtualKeyCode)189);      // _
                else if (c == '=')
                    ks.KeyPress((VirtualKeyCode)187);           // =
                else if (c == '+')
                    KeyWithShift(ks, (VirtualKeyCode)187);      // +
                else if (c == ';')
                    ks.KeyPress((VirtualKeyCode)186);           // ;
                else if (c == ':')
                    KeyWithShift(ks, (VirtualKeyCode)186);      // :
                else if (c == '\'')
                    ks.KeyPress((VirtualKeyCode)222);           // '
                else if (c == '"')
                    KeyWithShift(ks, (VirtualKeyCode)222);      // "
                else if (c == '[')
                    ks.KeyPress((VirtualKeyCode)219);           // [
                else if (c == '{')
                    KeyWithShift(ks, (VirtualKeyCode)219);      // {
                else if (c == ']')
                    ks.KeyPress((VirtualKeyCode)221);           // ]
                else if (c == '}')
                    KeyWithShift(ks, (VirtualKeyCode)221);      // }
                else if (c == '\\')
                    ks.KeyPress((VirtualKeyCode)220);           // \
                else if (c == '|')
                    KeyWithShift(ks, (VirtualKeyCode)220);      // |
                else
                    Console.WriteLine($"Unknown char: {c}");
                // end if
            }
        }

        public void ListAll()
        {
            for (int i = 13; i < 14; i++)
            {
                ks.Sleep(3000);
                var k = i < 10 ? VirtualKeyCode.VK_0 + i : VirtualKeyCode.VK_A + i - 10;
                ks.KeyPress(k);
                ks.KeyPress(VirtualKeyCode.RETURN);
                for (int j = 0; j < 16; j++)
                {
                    k = i < 10 ? VirtualKeyCode.VK_0 + j : VirtualKeyCode.VK_A + j - 10;
                    ks.KeyPress(k);
                    ks.KeyPress(VirtualKeyCode.SPACE);
                    ks.KeyPress((VirtualKeyCode)(i*16+j));
                    ks.KeyPress(VirtualKeyCode.RETURN);
                }
            }
        }
    }
}
