using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClient.Bots
{
    /// <summary>
    /// This bot sends a command every 60 seconds in order to stay non-afk.
    /// </summary>

	public class AntiAFK : Bot.Bot
    {
        private int count;
        private int timeping;

        /// <summary>
        /// This bot sends a /ping command every X seconds in order to stay non-afk.
        /// </summary>
        /// <param name="pingparam">Time amount between each ping (10 = 1s, 600 = 1 minute, etc.)</param>

        public AntiAFK(int pingparam)
        {
			onUpdate += Update;
            count = 0;
            timeping = pingparam;
            if (timeping < 10) { timeping = 10; } //To avoid flooding
        }

        public void Update()
        {
            count++;
            if (count == timeping)
            {
                SendText(Settings.AntiAFK_Command);
                count = 0;
            }
        }
    }
}
