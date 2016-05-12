using System;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/// <summary>
		/// Get the current Minecraft World
		/// </summary>
		/// <returns>Minecraft world or null if associated setting is disabled</returns>

		protected Mapping.World GetWorld()
		{
			if (Settings.TerrainAndMovements)
				return Handler.GetWorld();
			return null;
		}

	}
}

