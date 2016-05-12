using System;
using MinecraftClient.Mapping;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/// <summary>
		/// Get the current location of the player
		/// </summary>
		/// <returns>Minecraft world or null if associated setting is disabled</returns>

		protected Location GetCurrentLocation()
		{
			return Handler.GetCurrentLocation();
		}

		/// <summary>
		/// Move to the specified location
		/// </summary>
		/// <param name="location">Location to reach</param>
		/// <param name="allowUnsafe">Allow possible but unsafe locations</param>
		/// <returns>True if a path has been found</returns>

		protected bool MoveToLocation(Location location, bool allowUnsafe = false)
		{
			return Handler.MoveTo(location, allowUnsafe);
		}
	}
}

