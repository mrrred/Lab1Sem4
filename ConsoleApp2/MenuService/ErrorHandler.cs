using System;
using System.IO;

namespace ConsoleApp2.MenuService
{
    public static class ErrorHandler
    {
        public static string HandleException(Exception ex)
        {
            return ex switch
            {
                OutOfMemoryException => 
                    "Error: Insufficient memory. Try closing other applications or reducing data size.",
                
                IOException ioEx when ioEx.Message.Contains("disk", StringComparison.OrdinalIgnoreCase) || 
                                       ioEx.Message.Contains("space", StringComparison.OrdinalIgnoreCase) =>
                    "Error: Insufficient disk space. Free up space and try again.",
                
                IOException ioEx when ioEx.Message.Contains("access", StringComparison.OrdinalIgnoreCase) =>
                    "Error: Access denied. Check file permissions.",
                
                FileNotFoundException fnfEx =>
                    $"Error: File not found - {fnfEx.FileName}",
                
                DirectoryNotFoundException =>
                    "Error: Directory not found. Check the path.",
                
                IOException =>
                    "Error: File operation failed. The file may be in use or corrupted.",
                
                ArgumentException argEx =>
                    $"Error: Invalid input - {argEx.Message}",
                
                InvalidOperationException opEx =>
                    $"Error: Operation failed - {opEx.Message}",
                
                _ => $"Error: {ex.Message}"
            };
        }

        public static void LogError(Exception ex, string context)
        {
            try
            {
                string logPath = "error_log.txt";
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {context}: {ex.GetType().Name} - {ex.Message}\n";
                
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignore logging errors to prevent cascading failures
            }
        }
    }
}
