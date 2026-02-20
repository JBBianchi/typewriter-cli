using System;
using System.Globalization;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Typewriter.VisualStudio
{
    public class Log
        : ILog
    {
        private static Log _instance;

        private readonly DTE _dte;
        private OutputWindowPane _outputWindowPane;

        internal Log(DTE dte)
        {
            _dte = dte;
            _instance = this;
        }

        public static ILog Instance => _instance;

        public static void Debug(string message, params object[] parameters)
        {
#if DEBUG
            _instance?.Write("DEBUG", message, parameters);
#endif
        }

        public static void Info(string message, params object[] parameters)
        {
            _instance?.Write("INFO", message, parameters);
        }

        public static void Warn(string message, params object[] parameters)
        {
            _instance?.Write("WARNING", message, parameters);
        }

        public static void Error(string message, params object[] parameters)
        {
            _instance?.Write("ERROR", message, parameters);
        }

        public void LogDebug(
            string message,
            params object[] parameters) =>
            Write("DEBUG", message, parameters);

        public void LogInfo(
            string message,
            params object[] parameters) =>
            Write("INFO", message, parameters);

        public void LogWarning(
            string message,
            params object[] parameters) =>
            Write("WARNING", message, parameters);

        public void LogError(
            string message,
            params object[] parameters) =>
            Write("ERROR", message, parameters);

        private void Write(string type, string message, object[] parameters)
        {
            message = $"{DateTime.Now:HH:mm:ss.fff} {type}: {message}";

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (parameters.Any())
                    {
                        OutputWindow.OutputString(string.Format(CultureInfo.InvariantCulture, message, parameters) +
                                                  Environment.NewLine);
                    }
                    else
                    {
                        OutputWindow.OutputString(message + Environment.NewLine);
                    }
                }
                catch
                {
                }
            });
        }

        private OutputWindowPane OutputWindow
        {
            get
            {
                return ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    if (_outputWindowPane != null)
                    {
                        return _outputWindowPane;
                    }

                    var window = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                    var outputWindow = (OutputWindow) window.Object;

                    for (uint i = 1; i <= outputWindow.OutputWindowPanes.Count; i++)
                    {
                        if (outputWindow.OutputWindowPanes.Item(i).Name
                            .Equals(nameof(Typewriter), StringComparison.CurrentCultureIgnoreCase))
                        {
                            _outputWindowPane = outputWindow.OutputWindowPanes.Item(i);
                            break;
                        }
                    }

                    return _outputWindowPane ?? (_outputWindowPane = outputWindow.OutputWindowPanes.Add(nameof(Typewriter)));
                });
            }
        }
    }
}