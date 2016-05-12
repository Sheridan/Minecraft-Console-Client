using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace MinecraftClient.Bot
{

	///
	/// Welcome to the Bot API file !
	/// The virtual class "ChatBot" contains anything you need for creating chat bots
	/// Inherit from this class while adding your bot class to the folder "ChatBots".
	/// Override the methods you want for handling events: Initialize, Update, GetText.
	/// Once your bot is created, read the explanations below to start using it in the MinecraftClient app.
	///
	/// Pieces of code to add in other parts of the program for your bot. Line numbers are approximative.
	/// Settings.cs:73  | public static bool YourBot_Enabled = false;
	/// Settings.cs:74  | private enum ParseMode { /* [...] */, YourBot };
	/// Settings.cs:106 | case "yourbot": pMode = ParseMode.YourBot; break;
	/// Settings.cs:197 | case ParseMode.YourBot: switch (argName.ToLower()) { case "enabled": YourBot_Enabled = str2bool(argValue); break; } break;
	/// Settings.cs:267 | + "[YourBot]\r\n" + "enabled=false\r\n"
	/// McTcpClient:110 | if (Settings.YourBot_Enabled) { handler.BotLoad(new ChatBots.YourBot()); }
	/// Here your are. Now you will have a setting in MinecraftClient.ini for enabling your brand new bot.
	/// Delete MinecraftClient.ini to re-generate it or add the lines [YourBot] and enabled=true to the existing one.
	///

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

