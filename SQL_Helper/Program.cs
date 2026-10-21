namespace SQL_Helper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            if (args.Contains("--verify"))
            {
                string report = args.LastOrDefault(a => a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) ?? Path.Combine(Environment.CurrentDirectory, "verification-results.txt");
                Environment.ExitCode = ProjectVerification.RunAsync(report, args.Contains("--with-sql")).GetAwaiter().GetResult();
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new MainDashboard());
        }
    }
}
