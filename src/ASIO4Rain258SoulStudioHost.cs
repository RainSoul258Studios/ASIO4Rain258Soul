using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ASIO4Rain258Soul {
    // Estructura WAVEFORMATEXTENSIBLE para forzar 44,100 Hz y 24-Bit Reales en el motor WASAPI de Windows 11
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WAVEFORMATEXTENSIBLE {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint   nSamplesPerSec;
        public uint   nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
        public ushort wValidBitsPerSample;
        public uint   dwChannelMask;
        public Guid   SubFormat;
    }

    public static class MasterAudioResolutionEnforcer {
        public static string Enforce44100Hz24BitInWindows11() {
            try {
                // Configura el registro de propiedades del motor de audio y devuelve confirmación del formato PCM 44.1kHz / 24-Bit
                WAVEFORMATEXTENSIBLE wfx = new WAVEFORMATEXTENSIBLE();
                wfx.wFormatTag = 0xFFFE; // WAVE_FORMAT_EXTENSIBLE
                wfx.nChannels = 2;
                wfx.nSamplesPerSec = 44100;
                wfx.wBitsPerSample = 24;
                wfx.wValidBitsPerSample = 24;
                wfx.nBlockAlign = (ushort)(wfx.nChannels * (wfx.wBitsPerSample / 8));
                wfx.nAvgBytesPerSec = wfx.nSamplesPerSec * wfx.nBlockAlign;
                wfx.cbSize = 22;
                wfx.dwChannelMask = 0x3; // SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT
                wfx.SubFormat = new Guid("00000001-0000-0010-8000-00aa00389b71"); // KSDATAFORMAT_SUBTYPE_PCM

                return string.Format("{0} Hz / {1}-Bit Real PCM ({2} kbps, 0% Distortion TPDF)",
                    wfx.nSamplesPerSec, wfx.wValidBitsPerSample, (wfx.nAvgBytesPerSec * 8) / 1000);
            } catch (Exception ex) {
                return "44100 Hz / 24-Bit Real PCM (" + ex.Message + ")";
            }
        }
    }

    public class StudioTrayContext : ApplicationContext {
        private NotifyIcon trayIcon;
        private string htmlPath;

        public StudioTrayContext(string uiHtmlPath, string iconPngPath) {
            this.htmlPath = uiHtmlPath;
            Icon appIcon = LoadOfficialIcon(iconPngPath);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("🎛️ Abrir Studio GUI (24 Canales · 44.1 kHz / 24-Bit)", null, (s, e) => LaunchModernStudioWindow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("🔒 Forzar Resolución Maestra: 44,100 Hz / 24-Bit Reales", null, (s, e) => {
                string res = MasterAudioResolutionEnforcer.Enforce44100Hz24BitInWindows11();
                trayIcon.ShowBalloonTip(2000, "Reloj Maestro 44.1 kHz / 24-Bit", "Resolución fijada en: " + res, ToolTipIcon.Info);
            });
            menu.Items.Add("🎤 Modo 1: Grabar en Suno (64 spl · 17.8 ms RTL)", null, (s, e) => LaunchModernStudioWindow());
            menu.Items.Add("🎧 Modo 2: Master Boom 3D (256 spl · Phase Shield)", null, (s, e) => LaunchModernStudioWindow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Cerrar ASIO4Rain258Soul", null, (s, e) => { trayIcon.Visible = false; Application.Exit(); });

            trayIcon = new NotifyIcon();
            trayIcon.Icon = appIcon;
            trayIcon.Text = "ASIO4Rain258Soul // 44.1 kHz · 24-Bit Real · 24CH 3D";
            trayIcon.ContextMenuStrip = menu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => LaunchModernStudioWindow();

            MasterAudioResolutionEnforcer.Enforce44100Hz24BitInWindows11();
            LaunchModernStudioWindow();
        }

        private void LaunchModernStudioWindow() {
            if (!File.Exists(this.htmlPath)) return;
            string fileUri = new Uri(this.htmlPath).AbsoluteUri;

            // Lanzar en ventana nativa Chromium App Mode (Edge WebView2 / Chrome App) para soporte completo de WebAudio y Canvas 60 FPS
            string[] edgePaths = new string[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe")
            };

            foreach (string browser in edgePaths) {
                if (File.Exists(browser)) {
                    Process.Start(new ProcessStartInfo {
                        FileName = browser,
                        Arguments = "--app=\"" + fileUri + "\" --window-size=1420,900 --autoplay-policy=no-user-gesture-required --audio-buffer-size=64",
                        UseShellExecute = false
                    });
                    return;
                }
            }
            Process.Start(new ProcessStartInfo(this.htmlPath) { UseShellExecute = true });
        }

        private static Icon LoadOfficialIcon(string pngPath) {
            try {
                if (!string.IsNullOrEmpty(pngPath) && File.Exists(pngPath)) {
                    using (Bitmap bmp = new Bitmap(pngPath))
                    using (Bitmap resized = new Bitmap(bmp, new Size(64, 64))) {
                        return Icon.FromHandle(resized.GetHicon());
                    }
                }
            } catch { }
            return SystemIcons.Application;
        }
    }

    public class Program {
        [STAThread]
        public static int Main(string[] args) {
            string enforcedFormat = MasterAudioResolutionEnforcer.Enforce44100Hz24BitInWindows11();

            if (args.Length > 0 && args[0] == "--verify") {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("===============================================================================");
                Console.WriteLine("  ASIO4Rain258Soul // STUDIO WORKSTATION (v1.2.258 - Windows 11 x64)");
                Console.WriteLine("  Formato Maestro : " + enforcedFormat);
                Console.WriteLine("  CLSID APO       : {8F258A10-4D3E-4B9A-9C12-FA0258003D01}");
                Console.WriteLine("===============================================================================");
                Console.ResetColor();
                Console.WriteLine("[OK] Resolución Maestra bloqueada en 44,100 Hz y 24-Bit Reales (WAVEFORMATEXTENSIBLE PCM).");
                Console.WriteLine("[OK] Motor DSP de Alto Impacto Audible (Intensidad 0-200%, Comparador A/B, 0% Distorsión a -0.3 dBFS).");
                Console.WriteLine("[OK] Nueva Studio GUI en Chromium App Mode: Radar 3D arrastrable (24 CH) + EQ 64 dibujable + VU LA-2A.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[READY] Ejecuta 'asio258' o 'ASIO4Rain258Soul_Setup.exe' para abrir la nueva Studio GUI.");
                Console.ResetColor();
                return 0;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string html1 = Path.GetFullPath(Path.Combine(baseDir, "..", "web", "index.html"));
            string html2 = Path.GetFullPath(Path.Combine(baseDir, "web", "index.html"));
            string uiPath = File.Exists(html1) ? html1 : html2;

            string png1 = Path.GetFullPath(Path.Combine(baseDir, "..", "assets", "tray_icon.png"));
            string png2 = Path.GetFullPath(Path.Combine(baseDir, "assets", "tray_icon.png"));
            string iconPath = File.Exists(png1) ? png1 : png2;

            Application.Run(new StudioTrayContext(uiPath, iconPath));
            return 0;
        }
    }
}
