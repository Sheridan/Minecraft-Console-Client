using System;
using System.Collections.Generic;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		private List<string> registeredPluginChannels = new List<String>();
		
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
	}
}

