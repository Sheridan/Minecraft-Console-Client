using System;
using System.IO;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/// <summary>
		/// Write some text in the console. Nothing will be sent to the server.
		/// </summary>
		/// <param name="text">Log text to write</param>

		protected void LogToConsole(object text)
		{
			ConsoleIO.WriteLogLine(String.Format("[{0}] {1}", this.GetType().Name, text));
			string logfile = Settings.ExpandVars(Settings.chatbotLogFile);

			if (!String.IsNullOrEmpty(logfile))
			{
				if (!File.Exists(logfile))
				{
					try { Directory.CreateDirectory(Path.GetDirectoryName(logfile)); }
					catch { return; /* Invalid path or access denied */ }
					try { File.WriteAllText(logfile, ""); }
					catch { return; /* Invalid file name or access denied */ }
				}

				File.AppendAllLines(logfile, new string[] { GetTimestamp() + ' ' + text });
			}
		}
	}
}

