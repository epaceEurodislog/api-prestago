
using System;
using System.IO;

namespace PrestagoIntegration.Utils
{
    public static class Logger
    {
        private static readonly string LOG_FILE = "prestago_integration.log";
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    using (var writer = new StreamWriter(LOG_FILE, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - INFO - {message}");
                    }
                }
                Console.WriteLine($"INFO: {message}");
            }
            catch
            {
                // Ignorer les erreurs de journalisation
            }
        }

        public static void LogError(string context, Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    using (var writer = new StreamWriter(LOG_FILE, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR - {context}");
                        writer.WriteLine($"Message: {ex.Message}");
                        writer.WriteLine($"StackTrace: {ex.StackTrace}");

                        if (ex.InnerException != null)
                        {
                            writer.WriteLine($"InnerException: {ex.InnerException.Message}");
                        }

                        writer.WriteLine(new string('-', 80));
                    }
                }
                Console.WriteLine($"ERROR ({context}): {ex.Message}");
            }
            catch
            {
                // Ignorer les erreurs de journalisation
            }
        }
    }
}
