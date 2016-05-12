using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace MinecraftClient
{
	namespace Bot
	{
		/// <summary>
		/// The virtual base class for bots.
		/// </summary>
		public partial class Bot
		{
			public enum DisconnectReason { InGameKick, LoginRejected, ConnectionLost };

			//Handler will be automatically set on bot loading, don't worry about this
			public void SetHandler(McTcpClient handler) { this._handler = handler; }
			protected void SetMaster(Bot master) { this.master = master; }
			protected void LoadBot(Bot bot) { Handler.BotUnLoad(bot); Handler.BotLoad(bot); }
			private McTcpClient Handler { get { return master != null ? master.Handler : _handler; } }
			private McTcpClient _handler = null;
			private Bot master = null;
			private List<string> registeredPluginChannels = new List<String>();
			private Queue<string> chatQueue = new Queue<string>();
			private DateTime lastMessageSentTime = DateTime.MinValue;
			private bool CanSendTextNow
			{
				get
				{
					return DateTime.Now > lastMessageSentTime + Settings.botMessageDelay;
				}
			}

			/// <summary>
			/// Processes the current chat message queue, displaying a message after enough time passes.
			/// </summary>
			internal void ProcessQueuedText()
			{
				if (chatQueue.Count > 0)
				{
					if (CanSendTextNow)
					{
						string text = chatQueue.Dequeue();
						LogToConsole("Sending '" + text + "'");
						lastMessageSentTime = DateTime.Now;
						Handler.SendText(text);
					}
				}
			}

			/// <summary>
			/// Send text to the server. Can be anything such as chat messages or commands
			/// </summary>
			/// <param name="text">Text to send to the server</param>
			/// <param name="sendImmediately">Whether the message should be sent immediately rather than being queued to avoid chat spam</param>
			/// <returns>True if the text was sent with no error</returns>

			protected bool SendText(string text, bool sendImmediately = false)
			{
				if (Settings.botMessageDelay.TotalSeconds > 0 && !sendImmediately)
				{
					if (!CanSendTextNow)
					{
						chatQueue.Enqueue(text);
						// TODO: We don't know whether there was an error at this point, so we assume there isn't.
						// Might not be the best idea.
						return true;
					}
				}

				LogToConsole("Sending '" + text + "'");
				lastMessageSentTime = DateTime.Now;
				return Handler.SendText(text);
			}

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

			/// <summary>
			/// Run a script from a file using a Scripting bot
			/// </summary>
			/// <param name="filename">File name</param>
			/// <param name="playername">Player name to send error messages, if applicable</param>

			protected void RunScript(string filename, string playername = "")
			{
				Handler.BotLoad(new MinecraftClient.Bots.Script(filename, playername));
			}

			/// <summary>
			/// Perform an internal MCC command (not a server command, use SendText() instead for that!)
			/// </summary>
			/// <param name="command">The command to process</param>
			/// <returns>TRUE if the command was indeed an internal MCC command</returns>

			protected bool PerformInternalCommand(string command)
			{
				string temp = "";
				return Handler.PerformInternalCommand(command, ref temp);
			}

			/// <summary>
			/// Perform an internal MCC command (not a server command, use SendText() instead for that!)
			/// </summary>
			/// <param name="command">The command to process</param>
			/// <param name="response_msg">May contain a confirmation or error message after processing the command, or "" otherwise.</param>
			/// <returns>TRUE if the command was indeed an internal MCC command</returns>

			protected bool PerformInternalCommand(string command, ref string response_msg)
			{
				return Handler.PerformInternalCommand(command, ref response_msg);
			}

			/// <summary>
			/// Verify that a string contains only a-z A-Z 0-9 and _ characters.
			/// </summary>

			protected static bool IsValidName(string username)
			{
				if (String.IsNullOrEmpty(username))
					return false;

				foreach (char c in username)
					if (!((c >= 'a' && c <= 'z')
						|| (c >= 'A' && c <= 'Z')
						|| (c >= '0' && c <= '9')
						|| c == '_') )
						return false;

				return true;
			}

			/// <summary>
			/// Get a Y-M-D h:m:s timestamp representing the current system date and time
			/// </summary>

			protected static string GetTimestamp()
			{
				DateTime time = DateTime.Now;
				return String.Format("{0}-{1}-{2} {3}:{4}:{5}",
					time.Year.ToString("0000"),
					time.Month.ToString("00"),
					time.Day.ToString("00"),
					time.Hour.ToString("00"),
					time.Minute.ToString("00"),
					time.Second.ToString("00"));
			}

			/// <summary>
			/// Disconnect from the server and restart the program
			/// It will unload and reload all the bots and then reconnect to the server
			/// </summary>
			/// <param name="attempts">If connection fails, the client will make X extra attempts</param>

			protected void ReconnectToTheServer(int ExtraAttempts = 3)
			{
				McTcpClient.ReconnectionAttemptsLeft = ExtraAttempts;
				Program.Restart();
			}

			/// <summary>
			/// Disconnect from the server and exit the program
			/// </summary>

			protected void DisconnectAndExit()
			{
				Program.Exit();
			}

			/// <summary>
			/// Unload the chatbot, and release associated memory.
			/// </summary>

			protected void UnloadBot()
			{
				Handler.BotUnLoad(this);
			}

			/// <summary>
			/// Registers the given plugin channel for use by this chatbot.
			/// </summary>
			/// <param name="channel">The name of the channel to register</param>

			protected void RegisterPluginChannel(string channel)
			{
				this.registeredPluginChannels.Add(channel);
				Handler.RegisterPluginChannel(channel, this);
			}

			/// <summary>
			/// Unregisters the given plugin channel, meaning this chatbot can no longer use it.
			/// </summary>
			/// <param name="channel">The name of the channel to unregister</param>

			protected void UnregisterPluginChannel(string channel)
			{
				this.registeredPluginChannels.RemoveAll(chan => chan == channel);
				Handler.UnregisterPluginChannel(channel, this);
			}

			/// <summary>
			/// Sends the given plugin channel message to the server, if the channel has been registered.
			/// See http://wiki.vg/Plugin_channel for more information about plugin channels.
			/// </summary>
			/// <param name="channel">The channel to send the message on.</param>
			/// <param name="data">The data to send.</param>
			/// <param name="sendEvenIfNotRegistered">Should the message be sent even if it hasn't been registered by the server or this bot?  (Some Minecraft channels aren't registered)</param>
			/// <returns>Whether the message was successfully sent.  False if there was a network error or if the channel wasn't registered.</returns>

			protected bool SendPluginChannelMessage(string channel, byte[] data, bool sendEvenIfNotRegistered = false)
			{
				if (!sendEvenIfNotRegistered)
				{
					if (!this.registeredPluginChannels.Contains(channel))
					{
						return false;
					}
				}
				return Handler.SendPluginChannelMessage(channel, data, sendEvenIfNotRegistered);
			}

			/// <summary>
			/// Load entries from a file as a string array, removing duplicates and empty lines
			/// </summary>
			/// <param name="file">File to load</param>
			/// <returns>The string array or an empty array if failed to load the file</returns>

			protected string[] LoadDistinctEntriesFromFile(string file)
			{
				if (File.Exists(file))
				{
					//Read all lines from file, remove lines with no text, convert to lowercase,
					//remove duplicate entries, convert to a string array, and return the result.
					return File.ReadAllLines(file)
						.Where(line => !String.IsNullOrWhiteSpace(line))
						.Select(line => line.ToLower())
						.Distinct().ToArray();
				}
				else
				{
					LogToConsole("File not found: " + Settings.Alerts_MatchesFile);
					return new string[0];
				}
			}
		}
	}
}

