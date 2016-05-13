using System;

namespace MinecraftClient.Bot
{
	public partial class Bot
	{
		/* ================================================== */
		/*   Main events to subscribe for creating your bot   */
		/* ================================================== */

		public delegate void DSimple();

		/// <summary>
		/// Anything you want to initialize your bot, will be called on load by MinecraftCom
		///
		/// NOTE: Chat messages cannot be sent at this point in the login process.  If you want to send
		/// a message when the bot is loaded, use onGameJoined.
		/// </summary>
		public event DSimple onInitialize;
		public bool hasInitializeHandlers()	{ return onInitialize != null; }
		public void doInitialize() { if(hasInitializeHandlers()) { onInitialize(); } }

		/// <summary>
		/// Anything you want to before destroy your bot, will be called on destroy
		/// </summary>
		public event DSimple onDestroy;
		public bool hasDestroyHandlers()	{ return onDestroy != null; }
		public void doDestroy() { if(hasDestroyHandlers()) { onDestroy(); } }

		/// <summary>
		/// Called after the server has been joined successfully and chat messages are able to be sent.
		///
		/// NOTE: This is not always right after joining the server - if the bot was loaded after logging
		/// in this is still called.
		/// </summary>
		public event DSimple onGameJoined;
		public bool hasGameJoinHandlers()	{ return onGameJoined != null; }
		public void doGameJoin() { if(hasGameJoinHandlers()) { onGameJoined(); } }

		/// <summary>
		/// Will be called every ~100ms (10fps) if loaded in MinecraftCom
		/// </summary>
		public event DSimple onUpdate;
		public bool hasUpdateHandlers()	{ return onUpdate != null; }
		public void doUpdate() { if(hasUpdateHandlers()) { onUpdate(); } }

		public delegate void DTextRecieved(string text);

		/// <summary>
		/// Any text sent by the server will be sent here by MinecraftCom
		/// </summary>
		/// <param name="text">Text from the server</param>
		public event DTextRecieved onTextRecieved;
		public bool hasTextRecieveHandlers()	{ return onTextRecieved != null; }
		public void doTextRecieve(string text) { if(hasTextRecieveHandlers()) { onTextRecieved(text); } }

		public delegate bool DDisconnect(DisconnectReason reason, string message);

		/// <summary>
		/// Is called when the client has been disconnected fom the server
		/// </summary>
		/// <param name="reason">Disconnect Reason</param>
		/// <param name="message">Kick message, if any</param>
		/// <returns>Return TRUE if the client is about to restart</returns>
		public event DDisconnect onDisconnect;
		public bool hasDisconnectHandlers()	{ return onDisconnect != null; }
		public bool doDisconnect(DisconnectReason reason, string message) { if(hasDisconnectHandlers()) { return onDisconnect(reason, message); } return false; }

		public delegate void DPluginMessage(string channel, byte[] data);

		/// <summary>
		/// Called when a plugin channel message is received.
		/// The given channel must have previously been registered with RegisterPluginChannel.
		/// This can be used to communicate with server mods or plugins.  See wiki.vg for more
		/// information about plugin channels: http://wiki.vg/Plugin_channel
		/// </summary>
		/// <param name="channel">The name of the channel</param>
		/// <param name="data">The payload for the message</param>
		public event DPluginMessage onPluginMessage;
		public bool hasPluginMessageHandlers()	{ return onPluginMessage != null; }
		public void doPluginMessage(string channel, byte[] data) { if(hasPluginMessageHandlers()) { onPluginMessage(channel, data); } }
	}
}

