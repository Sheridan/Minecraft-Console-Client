using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace MinecraftClient.Bot
{


    /// <summary>
    /// The virtual class containing anything you need for creating chat bots.
    /// </summary>
	public partial class Bot
    {

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
        /// Remove color codes ("§c") from a text message received from the server
        /// </summary>

        protected static string GetVerbatim(string text)
        {
            if ( String.IsNullOrEmpty(text) )
                return String.Empty;

            int idx = 0;
            var data = new char[text.Length];

            for ( int i = 0; i < text.Length; i++ )
                if ( text[i] != '§' )
                    data[idx++] = text[i];
                else
                    i++;

            return new string(data, 0, idx);
        }



        /// <summary>
        /// Returns true if the text passed is a private message sent to the bot
        /// </summary>
        /// <param name="text">text to test</param>
        /// <param name="message">if it's a private message, this will contain the message</param>
        /// <param name="sender">if it's a private message, this will contain the player name that sends the message</param>
        /// <returns>Returns true if the text is a private message</returns>

        protected static bool IsPrivateMessage(string text, ref string message, ref string sender)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            text = GetVerbatim(text);

            //Built-in detection routine for private messages
            if (Settings.ChatFormat_Builtins)
            {
                string[] tmp = text.Split(' ');
                try
                {
                    //Detect vanilla /tell messages
                    //Someone whispers message (MC 1.5)
                    //Someone whispers to you: message (MC 1.7)
                    if (tmp.Length > 2 && tmp[1] == "whispers")
                    {
                        if (tmp.Length > 4 && tmp[2] == "to" && tmp[3] == "you:")
                        {
                            message = text.Substring(tmp[0].Length + 18); //MC 1.7
                        }
                        else message = text.Substring(tmp[0].Length + 10); //MC 1.5
                        sender = tmp[0];
                        return IsValidName(sender);
                    }

                    //Detect Essentials (Bukkit) /m messages
                    //[Someone -> me] message
                    //[~Someone -> me] message
                    else if (text[0] == '[' && tmp.Length > 3 && tmp[1] == "->"
                            && (tmp[2].ToLower() == "me]" || tmp[2].ToLower() == "moi]")) //'me' is replaced by 'moi' in french servers
                    {
                        message = text.Substring(tmp[0].Length + 4 + tmp[2].Length + 1);
                        sender = tmp[0].Substring(1);
                        if (sender[0] == '~') { sender = sender.Substring(1); }
                        return IsValidName(sender);
                    }

                    //Detect Modified server messages. /m
                    //[Someone @ me] message
                    else if (text[0] == '[' && tmp.Length > 3 && tmp[1] == "@"
                            && (tmp[2].ToLower() == "me]" || tmp[2].ToLower() == "moi]")) //'me' is replaced by 'moi' in french servers
                    {
                        message = text.Substring(tmp[0].Length + 4 + tmp[2].Length + 0);
                        sender = tmp[0].Substring(1);
                        if (sender[0] == '~') { sender = sender.Substring(1); }
                        return IsValidName(sender);
                    }

                    //Detect Essentials (Bukkit) /me messages with some custom prefix
                    //[Prefix] [Someone -> me] message
                    //[Prefix] [~Someone -> me] message
                    else if (text[0] == '[' && tmp[0][tmp[0].Length - 1] == ']'
                            && tmp[1][0] == '[' && tmp.Length > 4 && tmp[2] == "->"
                            && (tmp[3].ToLower() == "me]" || tmp[3].ToLower() == "moi]"))
                    {
                        message = text.Substring(tmp[0].Length + 1 + tmp[1].Length + 4 + tmp[3].Length + 1);
                        sender = tmp[1].Substring(1);
                        if (sender[0] == '~') { sender = sender.Substring(1); }
                        return IsValidName(sender);
                    }

                    //Detect Essentials (Bukkit) /me messages with some custom rank
                    //[Someone [rank] -> me] message
                    //[~Someone [rank] -> me] message
                    else if (text[0] == '[' && tmp.Length > 3 && tmp[2] == "->"
                            && (tmp[3].ToLower() == "me]" || tmp[3].ToLower() == "moi]"))
                    {
                        message = text.Substring(tmp[0].Length + 1 + tmp[1].Length + 4 + tmp[2].Length + 1);
                        sender = tmp[0].Substring(1);
                        if (sender[0] == '~') { sender = sender.Substring(1); }
                        return IsValidName(sender);
                    }

                    //Detect HeroChat PMsend
                    //From Someone: message
                    else if (text.StartsWith("From "))
                    {
                        sender = text.Substring(5).Split(':')[0];
                        message = text.Substring(text.IndexOf(':') + 2);
                        return IsValidName(sender);
                    }
                    else return false;
                }
                catch (IndexOutOfRangeException) { /* Not an expected chat format */ }
                catch (ArgumentOutOfRangeException) { /* Same here */ }
            }

            //User-defined regex for private chat messages
            if (Settings.ChatFormat_Private != null)
            {
                Match regexMatch = Settings.ChatFormat_Private.Match(text);
                if (regexMatch.Success && regexMatch.Groups.Count >= 3)
                {
                    sender = regexMatch.Groups[1].Value;
                    message = regexMatch.Groups[2].Value;
                    return IsValidName(sender);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the text passed is a public message written by a player on the chat
        /// </summary>
        /// <param name="text">text to test</param>
        /// <param name="message">if it's message, this will contain the message</param>
        /// <param name="sender">if it's message, this will contain the player name that sends the message</param>
        /// <returns>Returns true if the text is a chat message</returns>

        protected static bool IsChatMessage(string text, ref string message, ref string sender)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            text = GetVerbatim(text);
            
            //Built-in detection routine for public messages
            if (Settings.ChatFormat_Builtins)
            {
                string[] tmp = text.Split(' ');

                //Detect vanilla/factions Messages
                //<Someone> message
                //<*Faction Someone> message
                //<*Faction Someone>: message
                //<*Faction ~Nicknamed>: message
                if (text[0] == '<')
                {
                    try
                    {
                        text = text.Substring(1);
                        string[] tmp2 = text.Split('>');
                        sender = tmp2[0];
                        message = text.Substring(sender.Length + 2);
                        if (message.Length > 1 && message[0] == ' ')
                        { message = message.Substring(1); }
                        tmp2 = sender.Split(' ');
                        sender = tmp2[tmp2.Length - 1];
                        if (sender[0] == '~') { sender = sender.Substring(1); }
                        return IsValidName(sender);
                    }
                    catch (IndexOutOfRangeException) { /* Not a vanilla/faction message */ }
                    catch (ArgumentOutOfRangeException) { /* Same here */ }
                }

                //Detect HeroChat Messages
                //Public chat messages
                //[Channel] [Rank] User: Message
                else if (text[0] == '[' && text.Contains(':') && tmp.Length > 2)
                {
                    try
                    {
                        int name_end = text.IndexOf(':');
                        int name_start = text.Substring(0, name_end).LastIndexOf(']') + 2;
                        sender = text.Substring(name_start, name_end - name_start);
                        message = text.Substring(name_end + 2);
                        return IsValidName(sender);
                    }
                    catch (IndexOutOfRangeException) { /* Not a herochat message */ }
                    catch (ArgumentOutOfRangeException) { /* Same here */ }
                }

                //Detect (Unknown Plugin) Messages
                //**Faction<Rank> User : Message
                else if (text[0] == '*'
                    && text.Length > 1
                    && text[1] != ' '
                    && text.Contains('<') && text.Contains('>')
                    && text.Contains(' ') && text.Contains(':')
                    && text.IndexOf('*') < text.IndexOf('<')
                    && text.IndexOf('<') < text.IndexOf('>')
                    && text.IndexOf('>') < text.IndexOf(' ')
                    && text.IndexOf(' ') < text.IndexOf(':'))
                {
                    try
                    {
                        string prefix = tmp[0];
                        string user = tmp[1];
                        string semicolon = tmp[2];
                        if (prefix.All(c => char.IsLetterOrDigit(c) || new char[] { '*', '<', '>', '_' }.Contains(c))
                            && semicolon == ":")
                        {
                            message = text.Substring(prefix.Length + user.Length + 4);
                            return IsValidName(user);
                        }
                    }
                    catch (IndexOutOfRangeException) { /* Not a <unknown plugin> message */ }
                    catch (ArgumentOutOfRangeException) { /* Same here */ }
                }
            }

            //User-defined regex for public chat messages
            if (Settings.ChatFormat_Public != null)
            {
                Match regexMatch = Settings.ChatFormat_Public.Match(text);
                if (regexMatch.Success && regexMatch.Groups.Count >= 3)
                {
                    sender = regexMatch.Groups[1].Value;
                    message = regexMatch.Groups[2].Value;
                    return IsValidName(sender);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the text passed is a teleport request (Essentials)
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="sender">Will contain the sender's username, if it's a teleport request</param>
        /// <returns>Returns true if the text is a teleport request</returns>

        protected static bool IsTeleportRequest(string text, ref string sender)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            text = GetVerbatim(text);

            //Built-in detection routine for teleport requests
            if (Settings.ChatFormat_Builtins)
            {
                string[] tmp = text.Split(' ');

                //Detect Essentials teleport requests, prossibly with
                //nicknamed names or other modifications such as HeroChat
                if (text.EndsWith("has requested to teleport to you.")
                    || text.EndsWith("has requested that you teleport to them."))
                {
                    //<Rank> Username has requested...
                    //[Rank] Username has requested...
                    if (((tmp[0].StartsWith("<") && tmp[0].EndsWith(">"))
                        || (tmp[0].StartsWith("[") && tmp[0].EndsWith("]")))
                        && tmp.Length > 1)
                        sender = tmp[1];

                    //Username has requested...
                    else sender = tmp[0];

                    //~Username has requested...
                    if (sender.Length > 1 && sender[0] == '~')
                        sender = sender.Substring(1);

                    //Final check on username validity
                    return IsValidName(sender);
                }
            }

            //User-defined regex for teleport requests
            if (Settings.ChatFormat_TeleportRequest != null)
            {
                Match regexMatch = Settings.ChatFormat_TeleportRequest.Match(text);
                if (regexMatch.Success && regexMatch.Groups.Count >= 2)
                {
                    sender = regexMatch.Groups[1].Value;
                    return IsValidName(sender);
                }
            }

            return false;
        }

    }
}
