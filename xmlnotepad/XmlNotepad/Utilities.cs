using System;
using System.Xml;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace XmlNotepad
{
    public sealed class Utilities
    {
        private Utilities() { }

        public static void InitializeWriterSettings(XmlWriterSettings settings, IServiceProvider sp) {
            settings.CheckCharacters = false;
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;

            if (sp != null) {
                Settings s = (Settings)sp.GetService(typeof(Settings));
                if (s != null) {
                    settings.Indent = (bool)s["AutoFormatOnSave"];
                    IndentChar indentChar = (IndentChar)s["IndentChar"];
                    int indentLevel = (int)s["IndentLevel"];
                    char ch = (indentChar == IndentChar.Space) ? ' ' : '\t';
                    settings.IndentChars = new string(ch, indentLevel);
                    settings.NewLineChars = (string)s["NewLineChars"];
                }
            }
        }
        
        // Lighten up the given baseColor so it is easy to read on the system Highlight color background.
        public static Brush HighlightTextBrush(Color baseColor) {
            SolidBrush ht = SystemBrushes.Highlight as SolidBrush;
            Color selectedColor = ht != null ? ht.Color : Color.FromArgb(49, 106, 197);
            HLSColor cls = new HLSColor(baseColor);
            HLSColor hls = new HLSColor(selectedColor);
            int luminosity = (hls.Luminosity > 120) ? 20 : 220;
            return new SolidBrush(HLSColor.ColorFromHLS(cls.Hue, luminosity, cls.Saturation));
        }


        public static void OpenUrl(IntPtr hwnd, string url) {
            const int SW_SHOWNORMAL = 1;
            ShellExecute(hwnd, "open", url, null, Application.StartupPath, SW_SHOWNORMAL);
        }

        [DllImport("Shell32.dll", EntryPoint = "ShellExecuteA",
             SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true,
             CallingConvention = CallingConvention.StdCall)]
        static extern int ShellExecute(IntPtr handle, string verb, string file,
            string args, string dir, int show);
    }

    public static class CurrentEvent {
        public static EventArgs Event;
    }


}
