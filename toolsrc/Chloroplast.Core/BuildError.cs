using System;
using System.Collections.Generic;
using System.Linq;

namespace Chloroplast.Core
{
    public class BuildError
    {
        public string FilePath { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public DateTime Timestamp { get; set; }

        public BuildError(string filePath, string errorMessage, Exception exception = null)
        {
            FilePath = filePath;
            ErrorMessage = errorMessage;
            Exception = exception;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] Error in '{FilePath}': {ErrorMessage}";
        }
    }

    public class BuildErrorCollection
    {
        private readonly List<BuildError> _errors = new List<BuildError>();

        public void AddError(string filePath, string errorMessage, Exception exception = null)
        {
            _errors.Add(new BuildError(filePath, errorMessage, exception));
        }

        public void AddError(BuildError error)
        {
            _errors.Add(error);
        }

        public IReadOnlyList<BuildError> Errors => _errors.AsReadOnly();

        public bool HasErrors => _errors.Count > 0;

        public int ErrorCount => _errors.Count;

        public void Clear()
        {
            _errors.Clear();
        }

        public void WriteErrorsToConsole()
        {
            if (!HasErrors) return;

            Console.Error.WriteLine();
            Console.Error.WriteLine("=== BUILD ERRORS ===");
            Console.Error.WriteLine($"Found {ErrorCount} error(s) during build:");
            Console.Error.WriteLine();

            foreach (var error in _errors)
            {
                Console.Error.WriteLine(error.ToString());
                if (error.Exception != null && !string.IsNullOrEmpty(error.Exception.Message))
                {
                    var message = error.Exception.Message;
                    
                    // For template compilation errors, extract just the compilation errors, not the full generated source
                    if (message.Contains("Failed to compile template") && message.Contains("Generated source code:"))
                    {
                        var lines = message.Split('\n');
                        bool inErrorSection = false;
                        var relevantLines = new List<string>();
                        
                        foreach (var line in lines)
                        {
                            if (line.Trim().StartsWith("Error(s):"))
                            {
                                inErrorSection = true;
                                relevantLines.Add(line.Trim());
                                continue;
                            }
                            
                            if (inErrorSection)
                            {
                                if (line.Trim().StartsWith("Generated source code:"))
                                {
                                    break; // Stop before the generated source code
                                }
                                if (line.Trim().StartsWith("- ") || string.IsNullOrWhiteSpace(line))
                                {
                                    relevantLines.Add(line.Trim());
                                }
                            }
                        }
                        
                        if (relevantLines.Any())
                        {
                            Console.Error.WriteLine("    Template compilation failed:");
                            foreach (var line in relevantLines.Where(l => !string.IsNullOrWhiteSpace(l)))
                            {
                                Console.Error.WriteLine($"    {line}");
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"    Details: Template compilation failed (see error log for full details)");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"    Details: {message}");
                    }
                }
                Console.Error.WriteLine();
            }
        }

        public void WriteErrorsToFile(string filePath)
        {
            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine($"Build Errors Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine(new string('=', 50));
                writer.WriteLine();

                foreach (var error in _errors)
                {
                    writer.WriteLine(error.ToString());
                    if (error.Exception != null)
                    {
                        writer.WriteLine($"Exception: {error.Exception}");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}