using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace MinecraftClient
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
    /// The virtual class containing anything you need for creating chat bots.
    /// </summary>
	namespace Bot
	{
		public partial class Bot
	    {

	        /* =================================================================== */
	        /*  ToolBox - Methods below might be useful while creating your bot.   */
	        /*  You should not need to interact with other classes of the program. */
	        /*  All the methods in this ChatBot class should do the job for you.   */
	        /* =================================================================== */

	        



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

	        /// <summary>
	        /// Send a private message to a player
	        /// </summary>
	        /// <param name="player">Player name</param>
	        /// <param name="message">Message</param>

	        protected void SendPrivateMessage(string player, string message)
	        {
	            SendText(String.Format("/{0} {1} {2}", Settings.PrivateMsgsCmdName, player, message));
	        }

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

	        /// <summary>
	        /// Get the current location of the player
	        /// </summary>
	        /// <returns>Minecraft world or null if associated setting is disabled</returns>

	        protected Mapping.Location GetCurrentLocation()
	        {
	            return Handler.GetCurrentLocation();
	        }

	        /// <summary>
	        /// Move to the specified location
	        /// </summary>
	        /// <param name="location">Location to reach</param>
	        /// <param name="allowUnsafe">Allow possible but unsafe locations</param>
	        /// <returns>True if a path has been found</returns>

	        protected bool MoveToLocation(Mapping.Location location, bool allowUnsafe = false)
	        {
	            return Handler.MoveTo(location, allowUnsafe);
	        }






	    }
	}
}
