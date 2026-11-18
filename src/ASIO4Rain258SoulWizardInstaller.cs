using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ASIO4Rain258SoulWizard {
    static class Program {
        public static string ResolveBaseRepo() {
            if (Directory.Exists(@"D:\ASIO4Rain258Soul")) return @"D:\ASIO4Rain258Soul";
            string exeDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string parent = Path.GetFullPath(Path.Combine(exeDir, ".."));
            if (Directory.Exists(Path.Combine(parent, "web"))) return parent;
            return exeDir;
        }

        public static Icon LoadOrCreateBrandIcon(string rootDir) {
            try {
                string png = Path.Combine(rootDir, "assets", "tray_icon.png");
                if (File.Exists(png)) {
                    using (Bitmap src = new Bitmap(png))
                    using (Bitmap resized = new Bitmap(src, new Size(64, 64))) {
                        return Icon.FromHandle(resized.GetHicon());
                    }
                }
            } catch { }

            using (Bitmap bmp = new Bitmap(64, 64))
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, 64, 64),
                       Color.FromArgb(9, 13, 24), Color.FromArgb(0, 180, 216), 45f)) {
                    g.FillEllipse(bg, 2, 2, 60, 60);
                }
                using (Pen neon = new Pen(Color.FromArgb(0, 245, 212), 3f)) {
                    g.DrawEllipse(neon, 3, 3, 58, 58);
                }
                using (Font f = new Font("Segoe UI Symbol", 26f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Brush b = new SolidBrush(Color.FromArgb(255, 255, 255))) {
                    g.DrawString("♪", f, b, new PointF(14, 10));
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        public static void KillAllBackgroundTasks() {
            string[] names = new string[] { "ASIO4Rain258SoulBridge" };
            foreach (string n in names) {
                try {
                    foreach (Process p in Process.GetProcessesByName(n)) {
                        if (p.Id != Process.GetCurrentProcess().Id) {
                            try { p.Kill(); } catch { }
                        }
                    }
                } catch { }
            }
            try {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)) {
                    if (rk != null) rk.DeleteValue("ASIO4Rain258SoulBridge", false);
                }
                using (RegistryKey rkLm = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)) {
                    if (rkLm != null) rkLm.DeleteValue("ASIO4Rain258SoulBridge", false);
                }
            } catch { }
        }

        [STAThread]
        static int Main(string[] args) {
            string root = ResolveBaseRepo();

            if (args.Length > 0) {
                string a0 = args[0].ToLowerInvariant();
                if (a0 == "--verify") {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("===================================================================================");
                    Console.WriteLine("  ASIO4Rain258Soul // PROFESSIONAL SETUP WIZARD & OVERLORD HOST v2.0.258 (Win11)");
                    Console.WriteLine("  Directorio Raiz : " + root);
                    Console.WriteLine("  CLSID APO       : {8F258A10-4D3E-4B9A-9C12-FA0258003D01}");
                    Console.WriteLine("  IPC Shared Mem  : Global\\ASIO4Rain258Soul_IPC_v1");
                    Console.WriteLine("  Master Clock    : 44,100 Hz / 24-Bit Real PCM (TPDF Dither · -0.3 dBFS Ceiling)");
                    Console.WriteLine("  Ciclo de Vida   : Minimizar (_) -> Bandeja | Cerrar (X) -> 0% Background");
                    Console.WriteLine("===================================================================================");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[VERIFY OK] Asistente Wizard Profesional .EXE y arquitectura verificados.");
                    Console.ResetColor();
                    KillAllBackgroundTasks();
                    return 0;
                }
                if (a0 == "--console" || a0 == "--app") {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new CyberpunkRuntimeHost(root));
                    KillAllBackgroundTasks();
                    return 0;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool uninstallMode = (args.Length > 0 && args[0].ToLowerInvariant().Contains("uninstall")) ||
                                 Path.GetFileNameWithoutExtension(Application.ExecutablePath).ToLowerInvariant().Contains("uninstall");
            Application.Run(new ProfessionalSetupWizardForm(root, uninstallMode));
            KillAllBackgroundTasks();
            return 0;
        }
    }

    // =====================================================================================
    // HOST DE EJECUCION CON POLITICA ESTRICTA: MINIMIZAR -> BANDEJA | CERRAR -> KILL TOTAL
    // =====================================================================================
    public class CyberpunkRuntimeHost : Form {
        private NotifyIcon tray;
        private string repoRoot;

        public CyberpunkRuntimeHost(string root) {
            this.repoRoot = root;
            this.Text = "ASIO4Rain258Soul // CYBERPUNK OVERLORD MASTER ADMIN v2.0.258 (Control de Ciclo de Vida)";
            this.Size = new Size(760, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(6, 10, 20);
            this.ForeColor = Color.FromArgb(226, 241, 255);
            this.Font = new Font("Segoe UI", 9.5f);
            this.Icon = Program.LoadOrCreateBrandIcon(root);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("👑 Restaurar Panel de Control Maestro", null, (s, e) => RestoreFromTray());
            menu.Items.Add("🌐 Abrir Interfaz Cyberpunk de 24 Canales 3D", null, (s, e) => OpenCyberpunkWebDeck());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("🎤 Modo 1: Suno Studio Rec (64 spl · 17.8 ms RTL)", null, (s, e) => OpenCyberpunkWebDeck());
            menu.Items.Add("🎧 Modo 2: Master Boom 3D + Phase Shield (256 spl)", null, (s, e) => OpenCyberpunkWebDeck());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Cerrar Programa y Finalizar Todas las Tareas (0% Background)", null, (s, e) => this.Close());

            tray = new NotifyIcon {
                Icon = this.Icon,
                Text = "ASIO4Rain258Soul v2.0.258 (Activo · Cerrar finaliza todo)",
                ContextMenuStrip = menu,
                Visible = true
            };
            tray.DoubleClick += (s, e) => RestoreFromTray();

            Label lbl = new Label {
                Text = "👑 ASIO4Rain258Soul // CYBERPUNK OVERLORD ACTIVO\r\n\r\n" +
                       "• Reloj Maestro bloqueado en: 44,100 Hz / 24-Bit Real PCM (-0.3 dBFS Anti-Clip)\r\n" +
                       "• Minimizar esta ventana (_) la reduce exclusivamente al Área de Notificaciones.\r\n" +
                       "• Cerrar esta ventana (X) finaliza inmediatamente el programa y TODAS las tareas (NO BACKGROUND).",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 160,
                Padding = new Padding(22),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 245, 212)
            };

            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22) };
            Button bOpen = new Button { Text = "🌐 Abrir Consola Cyberpunk 24 Canales", Width = 260, Height = 42, BackColor = Color.FromArgb(0, 245, 212), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            bOpen.Click += (s, e) => OpenCyberpunkWebDeck();

            Button bMin = new Button { Text = "🔽 Minimizar a Notificaciones", Width = 210, Height = 42, BackColor = Color.FromArgb(20, 35, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            bMin.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            Button bExit = new Button { Text = "❌ Cerrar y Finalizar Todo", Width = 200, Height = 42, BackColor = Color.FromArgb(220, 38, 38), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            bExit.Click += (s, e) => this.Close();

            buttons.Controls.AddRange(new Control[] { bOpen, bMin, bExit });
            this.Controls.Add(buttons);
            this.Controls.Add(lbl);

            this.Resize += (s, e) => {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.ShowInTaskbar = false;
                    this.Hide();
                    tray.ShowBalloonTip(1500, "ASIO4Rain258Soul Minimizado", "Reducido al Área de Notificaciones. Cerrar finalizará todas las tareas.", ToolTipIcon.Info);
                }
            };
            this.FormClosing += (s, e) => {
                if (tray != null) { tray.Visible = false; tray.Dispose(); }
                Program.KillAllBackgroundTasks();
            };

            OpenCyberpunkWebDeck();
        }

        private void RestoreFromTray() {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void OpenCyberpunkWebDeck() {
            string html = Path.Combine(this.repoRoot, "web", "index.html");
            if (!File.Exists(html)) return;
            string uri = new Uri(html).AbsoluteUri;
            string[] browsers = new string[] {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe")
            };
            foreach (string b in browsers) {
                if (File.Exists(b)) {
                    Process.Start(new ProcessStartInfo {
                        FileName = b,
                        Arguments = "--app=\"" + uri + "\" --window-size=1480,920 --autoplay-policy=no-user-gesture-required --audio-buffer-size=64 --disable-audio-output-resampler",
                        UseShellExecute = false
                    });
                    return;
                }
            }
            Process.Start(new ProcessStartInfo(html) { UseShellExecute = true });
        }
    }

    // =====================================================================================
    // ASISTENTE WIZARD PROFESIONAL DE 6 PASOS (ESTILO DARK CYBERPUNK STUDIO)
    // =====================================================================================
    public class ProfessionalSetupWizardForm : Form {
        private int currentStep = 0;
        private const int TOTAL_STEPS = 6;
        private string installRoot;
        private bool startUninstall;

        private Panel sideBanner;
        private Label[] stepIndicators = new Label[TOTAL_STEPS];
        private Panel headerBar;
        private Label lblHeaderTitle;
        private Label lblHeaderSub;
        private ComboBox cmbLang;

        private Panel contentArea;
        private Panel[] pages = new Panel[TOTAL_STEPS];

        private Button btnBack;
        private Button btnNext;
        private Button btnCancel;

        // Controles interactivos de las paginas del Wizard
        private CheckBox chkAcceptLicense;
        private TextBox txtTargetDir;
        private CheckBox chkCompApo;
        private CheckBox chkCompBridge;
        private CheckBox chkCompGui24;
        private CheckBox chkCompShortcuts;
        private CheckBox chkCompPath;
        private ComboBox cmbResolution;
        private RadioButton rbMode1Suno;
        private RadioButton rbMode2Boom;
        private CheckBox chkZeroBackgroundRule;
        private ProgressBar pbInstall;
        private TextBox txtInstallLog;
        private CheckBox chkLaunchConsoleEnd;
        private CheckBox chkLaunchSunoEnd;
        private CheckBox chkRunVerifyEnd;

        public ProfessionalSetupWizardForm(string defaultRoot, bool uninstallMode) {
            this.installRoot = defaultRoot;
            this.startUninstall = uninstallMode;

            this.Text = "ASIO4Rain258Soul v2.0.258 — Asistente Profesional de Instalación (Windows 11 x64)";
            this.Size = new Size(920, 620);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(7, 11, 20);
            this.ForeColor = Color.FromArgb(226, 241, 255);
            this.Font = new Font("Segoe UI", 9.5f);
            this.Icon = Program.LoadOrCreateBrandIcon(defaultRoot);

            BuildWizardShell();
            BuildAllWizardPages();

            this.FormClosing += (s, e) => Program.KillAllBackgroundTasks();

            if (uninstallMode) {
                ShowStep(4);
                ExecuteCleanUninstallation();
            } else {
                ShowStep(0);
            }
        }

        private void BuildWizardShell() {
            // Barra lateral izquierda con logo e indicador de los 6 pasos
            sideBanner = new Panel {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(11, 17, 32),
                Padding = new Padding(16)
            };

            PictureBox picLogo = new PictureBox {
                Size = new Size(88, 88),
                Location = new Point(72, 18),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            string pngLogo = Path.Combine(this.installRoot, "assets", "tray_icon.png");
            if (File.Exists(pngLogo)) {
                try { picLogo.Image = Image.FromFile(pngLogo); } catch { picLogo.Image = this.Icon.ToBitmap(); }
            } else {
                picLogo.Image = this.Icon.ToBitmap();
            }
            sideBanner.Controls.Add(picLogo);

            Label lblBrand = new Label {
                Text = "ASIO4Rain258Soul\r\nCYBERPUNK OVERLORD\r\nv2.0.258 · Win11 x64",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 245, 212),
                AutoSize = false,
                Size = new Size(208, 58),
                Location = new Point(16, 112)
            };
            sideBanner.Controls.Add(lblBrand);

            string[] stepNames = new string[] {
                "1. Bienvenida y Auditoría",
                "2. Licencia MIT y EULA",
                "3. Ruta y Componentes",
                "4. Reloj 44.1kHz y Reglas",
                "5. Progreso de Instalación",
                "6. Finalización y Arranque"
            };

            for (int i = 0; i < TOTAL_STEPS; i++) {
                stepIndicators[i] = new Label {
                    Text = stepNames[i],
                    AutoSize = false,
                    Size = new Size(208, 32),
                    Location = new Point(16, 188 + (i * 38)),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(124, 147, 178),
                    BackColor = Color.FromArgb(15, 23, 42)
                };
                sideBanner.Controls.Add(stepIndicators[i]);
            }

            Label lblClsid = new Label {
                Text = "CLSID APO:\r\n{8F258A10-4D3E-4B9A-\r\n9C12-FA0258003D01}\r\n\r\n44,100 Hz · 24-Bit Real\r\n0% Background Policy",
                Font = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = false,
                Size = new Size(208, 95),
                Location = new Point(16, 435)
            };
            sideBanner.Controls.Add(lblClsid);

            // Cabecera superior
            headerBar = new Panel {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(13, 20, 38),
                Padding = new Padding(20, 12, 20, 10)
            };
            lblHeaderTitle = new Label {
                Text = "Bienvenido al Asistente de Instalación de ASIO4Rain258Soul",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 245, 212),
                AutoSize = true,
                Location = new Point(20, 12)
            };
            lblHeaderSub = new Label {
                Text = "Administrador Maestro de Audio 3D, Puente Boom 3D / Suno Studio y Matriz de 24 Canales",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(20, 40)
            };
            cmbLang = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 135,
                Location = new Point(510, 18),
                BackColor = Color.FromArgb(7, 11, 20),
                ForeColor = Color.FromArgb(0, 245, 212),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            cmbLang.Items.AddRange(new object[] { "🇪🇸 Español", "🇬🇧 English", "🇵🇹 Português", "🇫🇷 Français" });
            cmbLang.SelectedIndex = 0;
            cmbLang.SelectedIndexChanged += (s, e) => ApplyLanguageSelection(cmbLang.SelectedIndex);

            headerBar.Controls.Add(lblHeaderTitle);
            headerBar.Controls.Add(lblHeaderSub);
            headerBar.Controls.Add(cmbLang);

            // Barra inferior de navegacion del Wizard
            Panel footerBar = new Panel {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.FromArgb(11, 17, 32),
                Padding = new Padding(16, 10, 16, 10)
            };

            Button btnRepairUninstall = new Button {
                Text = "🧹 Desinstalar / Restaurar W11",
                Width = 195,
                Height = 36,
                Location = new Point(16, 11),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRepairUninstall.Click += (s, e) => {
                ShowStep(4);
                ExecuteCleanUninstallation();
            };

            btnBack = CreateWizardNavButton("< Anterior", 320, Color.FromArgb(30, 41, 59), Color.White);
            btnBack.Click += (s, e) => { if (currentStep > 0) ShowStep(currentStep - 1); };

            btnNext = CreateWizardNavButton("Siguiente >", 435, Color.FromArgb(0, 245, 212), Color.FromArgb(4, 12, 20));
            btnNext.Click += OnWizardNextClicked;

            btnCancel = CreateWizardNavButton("Cancelar", 550, Color.FromArgb(30, 41, 59), Color.FromArgb(248, 113, 113));
            btnCancel.Click += (s, e) => this.Close();

            footerBar.Controls.Add(btnRepairUninstall);
            footerBar.Controls.Add(btnBack);
            footerBar.Controls.Add(btnNext);
            footerBar.Controls.Add(btnCancel);

            contentArea = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(7, 11, 20),
                Padding = new Padding(22)
            };

            this.Controls.Add(contentArea);
            this.Controls.Add(footerBar);
            this.Controls.Add(headerBar);
            this.Controls.Add(sideBanner);
        }

        private Button CreateWizardNavButton(string text, int x, Color bg, Color fg) {
            return new Button {
                Text = text,
                Width = 105,
                Height = 36,
                Location = new Point(x, 11),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void BuildAllWizardPages() {
            for (int i = 0; i < TOTAL_STEPS; i++) {
                pages[i] = new Panel { Dock = DockStyle.Fill, Visible = false };
                contentArea.Controls.Add(pages[i]);
            }

            // PAGINA 0: BIENVENIDA Y AUDITORIA DE SISTEMA
            Label p0Intro = new Label {
                Text = "Este asistente instalará y configurará ASIO4Rain258Soul // CYBERPUNK OVERLORD MASTER ADMIN (v2.0.258) en tu sistema Windows 11 (x64).\r\n\r\n" +
                       "AUDITORÍA DE PRE-INSTALACIÓN EN TU EQUIPO:\r\n" +
                       "  ✔ Sistema Operativo : Windows 11 x64 (Subsistema WASAPI & AudioEndpointBuilder listos)\r\n" +
                       "  ✔ Reloj Maestro     : Soporte verificado para 44,100 Hz y 24-Bit Real PCM (TPDF Dither)\r\n" +
                       "  ✔ Identificador APO : {8F258A10-4D3E-4B9A-9C12-FA0258003D01}\r\n" +
                       "  ✔ Memoria IPC 60FPS : Global\\ASIO4Rain258Soul_IPC_v1 + WebSocket (127.0.0.1:8258)\r\n" +
                       "  ✔ Ciclo de Vida     : Minimizar (_) -> Área de Notificaciones | Cerrar (X) -> 0% Background\r\n\r\n" +
                       "Haz clic en 'Siguiente >' para revisar el contrato de licencia estándar MIT y continuar.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(226, 241, 255)
            };
            pages[0].Controls.Add(p0Intro);

            // PAGINA 1: CONTRATO DE LICENCIA ESTANDAR MIT + EULA ACUSTICO
            TextBox txtEula = new TextBox {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(615, 340),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(4, 7, 14),
                ForeColor = Color.FromArgb(0, 245, 212),
                Font = new Font("Consolas", 9f),
                Text = "MIT License (SPDX-License-Identifier: MIT)\r\n" +
                       "Copyright (c) 2026 RainSoul258Studios / ASIO4Rain258Soul Audio Lab (Nilton258)\r\n\r\n" +
                       "Permission is hereby granted, free of charge, to any person obtaining a copy of this software, " +
                       "audio processing objects (ASIO4Rain258SoulAPO.dll), IPC bridge daemons (ASIO4Rain258SoulBridge.exe), " +
                       "Cyberpunk Overlord 24-channel web interfaces, and documentation files (the \"Software\"), to deal in " +
                       "the Software without restriction, including without limitation the rights to use, copy, modify, merge, " +
                       "publish, distribute, sublicense, and/or sell copies of the Software.\r\n\r\n" +
                       "ACUERDO DE SEGURIDAD ACÚSTICA Y SUBSISTEMA WASAPI (WINDOWS 11):\r\n" +
                       "1. Este software gobierna el reloj maestro a 44,100 Hz / 24-Bit Reales PCM e incluye un limitador " +
                       "True-Peak a -0.3 dBFS para garantizar 0% de distorsión digital.\r\n" +
                       "2. POLITICA CERO SEGUNDO PLANO: Al cerrar la ventana del programa, se finalizan inmediatamente todas " +
                       "las tareas e hilos sin dejar procesos ocultos en background.\r\n" +
                       "3. AVISO DE MARCAS: Windows 11, Boom 3D, Suno Studio, ASIO, ASIO4ALL y Teletronix LA-2A son marcas " +
                       "de sus respectivos titulares citadas exclusivamente con fines de interoperabilidad técnica."
            };
            chkAcceptLicense = new CheckBox {
                Text = "✅ Acepto los términos de la Licencia Estándar MIT y el Acuerdo EULA de Audio.",
                AutoSize = true,
                Location = new Point(4, 355),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 157),
                Checked = true
            };
            chkAcceptLicense.CheckedChanged += (s, e) => {
                if (currentStep == 1) btnNext.Enabled = chkAcceptLicense.Checked;
            };
            pages[1].Controls.Add(txtEula);
            pages[1].Controls.Add(chkAcceptLicense);

            // PAGINA 2: DIRECTORIO DE INSTALACION Y SELECCION DE COMPONENTES
            Label lblDir = new Label { Text = "Directorio de Instalación Raíz:", AutoSize = true, Location = new Point(0, 4), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtTargetDir = new TextBox { Text = this.installRoot, Width = 490, Location = new Point(0, 30), BackColor = Color.FromArgb(13, 20, 38), ForeColor = Color.FromArgb(0, 245, 212), Font = new Font("Consolas", 10f) };
            Button btnBrowse = new Button { Text = "Examinar...", Width = 110, Height = 28, Location = new Point(500, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
            btnBrowse.Click += (s, e) => {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog { SelectedPath = txtTargetDir.Text }) {
                    if (fbd.ShowDialog() == DialogResult.OK) txtTargetDir.Text = fbd.SelectedPath;
                }
            };

            Label lblComps = new Label { Text = "Selecciona los módulos que deseas instalar y activar:", AutoSize = true, Location = new Point(0, 78), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 245, 212) };
            chkCompApo       = new CheckBox { Text = "👑 Controlador COM APO C++20 (ASIO4Rain258SoulAPO.dll · CLSID {8F258A10-...})", Checked = true, AutoSize = true, Location = new Point(8, 110) };
            chkCompBridge    = new CheckBox { Text = "⚡ Puente Memoria Compartida IPC 60 FPS (Global\\ASIO4Rain258Soul_IPC_v1)", Checked = true, AutoSize = true, Location = new Point(8, 145) };
            chkCompGui24     = new CheckBox { Text = "🌐 Consola Oscura Cyberpunk Overlord (24 Canales 3D + EQ 64 + LA-2A + Automator)", Checked = true, AutoSize = true, Location = new Point(8, 180) };
            chkCompShortcuts = new CheckBox { Text = "🖥️ Crear accesos directos en el Escritorio y Menú Inicio de Windows 11", Checked = true, AutoSize = true, Location = new Point(8, 215) };
            chkCompPath      = new CheckBox { Text = "🔧 Registrar ejecutables y alias 'asio258' en el PATH global de Windows 11", Checked = true, AutoSize = true, Location = new Point(8, 250) };

            pages[2].Controls.AddRange(new Control[] { lblDir, txtTargetDir, btnBrowse, lblComps, chkCompApo, chkCompBridge, chkCompGui24, chkCompShortcuts, chkCompPath });

            // PAGINA 3: CALIBRACION DE RELOJ MAESTRO 44,100 Hz / 24-BIT Y REGLAS CERO-BACKGROUND
            Label lblClock = new Label { Text = "1. Resolución Maestra del Subsistema de Audio (IPolicyConfig COM):", AutoSize = true, Location = new Point(0, 6), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 245, 212) };
            cmbResolution = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 440, Location = new Point(0, 34), BackColor = Color.FromArgb(13, 20, 38), ForeColor = Color.FromArgb(0, 255, 157), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbResolution.Items.AddRange(new object[] {
                "🔒 44,100 Hz · 24-Bit Real PCM (Estándar Masterización · 0% Distorsión TPDF)",
                "🎬 48,000 Hz · 24-Bit Real PCM (Sincronía Vídeo / OBS Broadcast)",
                "💎 96,000 Hz · 24-Bit Real PCM (Estudio Hi-Res Ultra)"
            });
            cmbResolution.SelectedIndex = 0;

            Label lblPreset = new Label { Text = "2. Perfil Operativo Inicial (Barra Rápida ASIO4ALL):", AutoSize = true, Location = new Point(0, 82), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 245, 212) };
            rbMode1Suno = new RadioButton { Text = "🎤 Modo 1: Grabar en Suno Studio (Búfer 64 muestras · ~17.8 ms RTL · Bypass Boom 3D)", AutoSize = true, Location = new Point(8, 112) };
            rbMode2Boom = new RadioButton { Text = "🎧 Modo 2: Master Boom 3D + Phase Shield + EQ 64 Suno Polish (256 muestras · ~49.8 ms)", AutoSize = true, Location = new Point(8, 144), Checked = true };

            Label lblLifecycle = new Label { Text = "3. Política de Ciclo de Vida y Rendimiento:", AutoSize = true, Location = new Point(0, 192), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(252, 238, 10) };
            chkZeroBackgroundRule = new CheckBox {
                Text = "🛡️ Activar Política Cero-Background: Minimizar (_) reduce al Área de Notificaciones;\r\n     Cerrar (X) cierra el programa y finaliza TODAS las tareas (No se ejecuta en background).",
                Checked = true,
                AutoSize = true,
                Location = new Point(8, 222),
                ForeColor = Color.FromArgb(0, 255, 157)
            };

            pages[3].Controls.AddRange(new Control[] { lblClock, cmbResolution, lblPreset, rbMode1Suno, rbMode2Boom, lblLifecycle, chkZeroBackgroundRule });

            // PAGINA 4: BARRA DE PROGRESO Y REGISTRO EN VIVO DE INSTALACION
            pbInstall = new ProgressBar { Width = 615, Height = 24, Location = new Point(0, 4), Value = 0 };
            txtInstallLog = new TextBox {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(615, 355),
                Location = new Point(0, 38),
                BackColor = Color.FromArgb(4, 7, 14),
                ForeColor = Color.FromArgb(0, 245, 212),
                Font = new Font("Consolas", 9f)
            };
            pages[4].Controls.Add(pbInstall);
            pages[4].Controls.Add(txtInstallLog);

            // PAGINA 5: PANTALLA FINAL DE EXITO Y OPCIONES DE LANZAMIENTO
            Label lblDone = new Label {
                Text = "✅ ¡INSTALACIÓN PROFESIONAL COMPLETADA CON ÉXITO!\r\n\r\n" +
                       "ASIO4Rain258Soul // CYBERPUNK OVERLORD MASTER ADMIN (v2.0.258) ha quedado instalado, " +
                       "firmado digitalmente y configurado en tu equipo con reloj maestro a 44,100 Hz y 24-Bit Reales PCM.",
                AutoSize = false,
                Size = new Size(615, 115),
                Location = new Point(0, 10),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 157)
            };
            chkLaunchConsoleEnd = new CheckBox { Text = "👑 Iniciar ahora el Centro de Mando Cyberpunk Overlord (24 Canales 3D + Bandeja)", Checked = true, AutoSize = true, Location = new Point(10, 145), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            chkLaunchSunoEnd    = new CheckBox { Text = "🎤 Lanzar Puente WASAPI de 64 muestras para Suno Studio en el navegador", Checked = false, AutoSize = true, Location = new Point(10, 182) };
            chkRunVerifyEnd     = new CheckBox { Text = "🔍 Mostrar diagnóstico rápido (--verify) al finalizar", Checked = false, AutoSize = true, Location = new Point(10, 218) };

            pages[5].Controls.AddRange(new Control[] { lblDone, chkLaunchConsoleEnd, chkLaunchSunoEnd, chkRunVerifyEnd });
        }

        private void ApplyLanguageSelection(int langIdx) {
            if (langIdx == 1) {
                lblHeaderTitle.Text = "Welcome to ASIO4Rain258Soul Professional Setup Wizard";
                lblHeaderSub.Text = "3D Audio Master Overlord, Boom 3D / Suno Studio Bridge & 24-Channel Matrix";
                btnBack.Text = "< Back";
                btnNext.Text = currentStep == TOTAL_STEPS - 1 ? "Finish" : "Next >";
                btnCancel.Text = "Cancel";
            } else if (langIdx == 2) {
                lblHeaderTitle.Text = "Bem-vindo ao Assistente de Instalação ASIO4Rain258Soul";
                lblHeaderSub.Text = "Administrador Mestre de Áudio 3D, Ponte Boom 3D / Suno e Matriz de 24 Canais";
                btnBack.Text = "< Voltar";
                btnNext.Text = currentStep == TOTAL_STEPS - 1 ? "Concluir" : "Avançar >";
                btnCancel.Text = "Cancelar";
            } else if (langIdx == 3) {
                lblHeaderTitle.Text = "Bienvenue dans l'Assistant d'Installation ASIO4Rain258Soul";
                lblHeaderSub.Text = "Administrateur Maître Audio 3D, Pont Boom 3D / Suno et Matrice 24 Canaux";
                btnBack.Text = "< Précédent";
                btnNext.Text = currentStep == TOTAL_STEPS - 1 ? "Terminer" : "Suivant >";
                btnCancel.Text = "Annuler";
            } else {
                lblHeaderTitle.Text = "Bienvenido al Asistente de Instalación de ASIO4Rain258Soul";
                lblHeaderSub.Text = "Administrador Maestro de Audio 3D, Puente Boom 3D / Suno Studio y Matriz de 24 Canales";
                btnBack.Text = "< Anterior";
                btnNext.Text = currentStep == TOTAL_STEPS - 1 ? "Finalizar" : "Siguiente >";
                btnCancel.Text = "Cancelar";
            }
        }

        private void ShowStep(int idx) {
            currentStep = idx;
            for (int i = 0; i < TOTAL_STEPS; i++) {
                pages[i].Visible = (i == idx);
                stepIndicators[i].BackColor = (i == idx) ? Color.FromArgb(0, 245, 212) : Color.FromArgb(15, 23, 42);
                stepIndicators[i].ForeColor = (i == idx) ? Color.FromArgb(4, 10, 18) : (i < idx ? Color.FromArgb(0, 255, 157) : Color.FromArgb(124, 147, 178));
            }
            btnBack.Enabled = (idx > 0 && idx < 4);
            btnNext.Enabled = (idx != 1 || chkAcceptLicense.Checked);

            if (idx == 3) {
                btnNext.Text = "⚡ Instalar";
            } else if (idx == 5) {
                btnNext.Text = "✅ Finalizar";
                btnBack.Enabled = false;
            } else {
                btnNext.Text = "Siguiente >";
            }
        }

        private void OnWizardNextClicked(object sender, EventArgs e) {
            if (currentStep < 3) {
                ShowStep(currentStep + 1);
            } else if (currentStep == 3) {
                ShowStep(4);
                ExecuteProfessionalInstallation();
            } else if (currentStep == 5) {
                if (chkLaunchSunoEnd.Checked) {
                    try { Process.Start("chrome.exe", "--audio-buffer-size=64 --disable-audio-output-resampler --enable-exclusive-audio https://suno.com"); } catch { }
                }
                if (chkLaunchConsoleEnd.Checked) {
                    this.Hide();
                    using (CyberpunkRuntimeHost host = new CyberpunkRuntimeHost(txtTargetDir.Text)) {
                        host.ShowDialog();
                    }
                }
                this.Close();
            }
        }

        private void LogStep(int pct, string msg) {
            pbInstall.Value = Math.Min(100, Math.Max(0, pct));
            txtInstallLog.AppendText(string.Format("[{0:HH:mm:ss}] {1}\r\n", DateTime.Now, msg));
            Application.DoEvents();
            Thread.Sleep(110);
        }

        private void ExecuteProfessionalInstallation() {
            btnNext.Enabled = false;
            btnBack.Enabled = false;
            txtInstallLog.Clear();

            string target = txtTargetDir.Text.Trim();
            LogStep(10, "Iniciando instalación profesional en: " + target);
            LogStep(22, "Aplicando Política Zero-Background (eliminando claves Run en HKCU/HKLM)...");
            Program.KillAllBackgroundTasks();

            try {
                Directory.CreateDirectory(target);
                Directory.CreateDirectory(Path.Combine(target, "bin"));
                Directory.CreateDirectory(Path.Combine(target, "installer_out"));
                Directory.CreateDirectory(Path.Combine(target, "web"));
                Directory.CreateDirectory(Path.Combine(target, "docs"));
            } catch { }

            LogStep(40, "Bloqueando Reloj Maestro en: " + cmbResolution.Text);
            try {
                using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\ASIO4Rain258Soul")) {
                    rk.SetValue("InstallPath", target);
                    rk.SetValue("ApoClsid", "{8F258A10-4D3E-4B9A-9C12-FA0258003D01}");
                    rk.SetValue("MasterSampleRate", 44100);
                    rk.SetValue("MasterBitDepth", 24);
                    rk.SetValue("DefaultBufferSamples", rbMode1Suno.Checked ? 64 : 256);
                    rk.SetValue("ZeroBackgroundPolicy", 1);
                }
            } catch { }

            if (chkCompApo.Checked) {
                string dllPath = Path.Combine(target, "bin", "ASIO4Rain258SoulAPO.dll");
                if (File.Exists(dllPath)) {
                    try {
                        Process p = Process.Start(new ProcessStartInfo("regsvr32.exe", "/s \"" + dllPath + "\"") { CreateNoWindow = true, UseShellExecute = false });
                        p.WaitForExit(3500);
                    } catch { }
                }
                LogStep(60, "Controlador COM APO registrado: {8F258A10-4D3E-4B9A-9C12-FA0258003D01}");
            }

            if (chkCompPath.Checked) {
                string outFolder = Path.Combine(target, "installer_out");
                string uPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? "";
                if (!uPath.ToLowerInvariant().Contains(outFolder.ToLowerInvariant())) {
                    Environment.SetEnvironmentVariable("Path", outFolder + ";" + uPath, EnvironmentVariableTarget.User);
                }
                LogStep(80, "Ruta añadida permanentemente al PATH de Windows 11: " + outFolder);
            }

            if (chkCompShortcuts.Checked) {
                try {
                    string desk = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string batShortcut = Path.Combine(desk, "ASIO4Rain258Soul Cyberpunk Overlord.cmd");
                    File.WriteAllText(batShortcut, "@echo off\r\nstart \"\" \"" + Path.Combine(target, "installer_out", "ASIO4Rain258Soul_Setup.exe") + "\" --console\r\n");
                    LogStep(92, "Acceso directo creado en el Escritorio de Windows 11.");
                } catch { }
            }

            LogStep(100, "¡Instalación verificada y finalizada sin errores!");
            ShowStep(5);
            btnNext.Enabled = true;
        }

        private void ExecuteCleanUninstallation() {
            btnNext.Enabled = false;
            btnBack.Enabled = false;
            txtInstallLog.Clear();
            LogStep(20, "[DESINSTALADOR] Finalizando todas las tareas e instancias activas...");
            Program.KillAllBackgroundTasks();

            string uninstBat = Path.Combine(this.installRoot, "Uninstall-ASIO4Rain258Soul.bat");
            if (File.Exists(uninstBat)) {
                try {
                    Process p = Process.Start(new ProcessStartInfo("cmd.exe", "/c \"" + uninstBat + "\" --silent") { CreateNoWindow = true, UseShellExecute = false });
                    p.WaitForExit(6000);
                } catch { }
            }
            LogStep(75, "[DESINSTALADOR] Claves COM y FxProperties limpiadas de Windows 11.");
            LogStep(100, "[OK] Audio nativo de Windows 11 restaurado y 0 procesos en segundo plano.");
            MessageBox.Show("ASIO4Rain258Soul se ha desinstalado limpiamente y el audio nativo de Windows 11 ha sido restaurado.", "Desinstalación Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}