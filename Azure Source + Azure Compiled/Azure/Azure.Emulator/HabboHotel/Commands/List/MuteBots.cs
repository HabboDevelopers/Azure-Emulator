﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class MuteBots. This class cannot be inherited.
    /// </summary>
    internal sealed class MuteBots : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MuteBots"/> class.
        /// </summary>
        public MuteBots()
        {
            MinRank = 5;
            Description = "Mute bots in your own room.";
            Usage = ":mutebots";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            room.MutedBots = !room.MutedBots;
            session.SendNotif(Azure.GetLanguage().GetVar("user_room_mute_bots"));

            return true;
        }
    }
}