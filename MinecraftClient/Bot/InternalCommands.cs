using System;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
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
			if (!Handler.PerformInternalCommand (command, ref temp)) 
			{
				LogToConsole (temp);
				return false;
			}
			return true;
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
	}
}

