using System;
using MinecraftClient.Mapping;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/// <summary>
		/// Send a private message to a player
		/// </summary>
		/// <param name="player">Player name</param>
		/// <param name="message">Message</param>

		protected void SendPrivateMessage(string player, string message)
		{
			SendText(String.Format("/{0} {1} {2}", Settings.PrivateMsgsCmdName, player, message));
		}

		#region Teleport
		/// <summary>
		/// Teleport player to other player
		/// </summary>
		/// <param name="player">Player name</param>
		/// <param name="targetPlayer">Target player name</param>

		protected void tp(string player, string targetPlayer)
		{
			if (IsValidName (targetPlayer)) 
			{
				SendText (String.Format ("/tp {0} {1}", player, targetPlayer));
			} 
			else 
			{
				LogToConsole (String.Format("Invalid target player {0}", targetPlayer));
			}
		}

		/// <summary>
		/// Teleport player to location
		/// </summary>
		/// <param name="player">Player name</param>
		/// <param name="location">Target location</param>

		protected void tp(string player, Location location)
		{
			SendText(String.Format("/tp {0} {1} {2} {3}", player, location.X, location.Y, location.Z));
		}

		/// <summary>
		/// Teleport self to other player
		/// </summary>
		/// <param name="targetPlayer">Target player name</param>

		protected void tp(string targetPlayer)
		{
			tp (Settings.Username, targetPlayer);
		}

		/// <summary>
		/// Teleport self to location
		/// </summary>
		/// <param name="location">Target location</param>

		protected void tp(Location location)
		{
			tp (Settings.Username, location);
		}
		#endregion
	}
}

