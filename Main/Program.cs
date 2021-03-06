using System;
using System.Windows.Forms;
using System.Threading;
using GoldBox.Logging;
using System.IO;
using GoldBox.Engine;

namespace Main
{
    static class Program
    {
        static MainForm main;
        static Thread engineThread;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Config.Setup();

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledExceptions;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logger.SetExitFunc(seg043.print_and_exit);

            main = new MainForm();

            var threadDelegate = new ThreadStart(EngineThread);
            engineThread = new Thread(threadDelegate);
            engineThread.Name = "Engine";
            engineThread.Start();

            Application.Run(main);
        }

        private static void HandleUnhandledExceptions(object sender, UnhandledExceptionEventArgs args)
        {
            var exception = (Exception)args.ExceptionObject;

            string logFile = Path.Combine(Logger.GetPath(), "Crash Log.txt");

            using (TextWriter tw = new StreamWriter(logFile, true))
            {
                tw.WriteLine("");
                tw.WriteLine("{0} {1}", Application.ProductName, Application.ProductVersion);
                tw.WriteLine("{0}", DateTime.Now);
                tw.WriteLine("Unhandled exception: " + exception);
            }

            MessageBox.Show(string.Format("Unexpected Error, please send '{0}' to simeon.pilgrim@gmail.com", logFile), "Unexpected Error");
            Environment.Exit(1);
        }

        public delegate void VoidDelegate();

        static void EngineStopped()
        {
            VoidDelegate d = delegate()
            {
                Application.Exit();
            };
            main.Invoke(d);
        }

        static void EngineThread()
        {
            GameEngine.__SystemInit(EngineStopped);
            GameEngine.PROGRAM();

            EngineStopped();
        }
    }
}