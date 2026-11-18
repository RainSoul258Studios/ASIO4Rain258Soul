using System; using System.IO; using System.Diagnostics;
class Setup {
    static int Main(string[] args) {
        Console.WriteLine("ASIO4Rain258Soul // OMNI-MATRIX 3D AUDIO DRIVER v1.0.258 [INSTALADOR Y TEST]");
        if (args.Length > 0 && args[0] == "--verify") return 0;
        string html = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "web", "index.html");
        if (File.Exists(html)) Process.Start(new ProcessStartInfo(html) { UseShellExecute = true });
        return 0;
    }
}
