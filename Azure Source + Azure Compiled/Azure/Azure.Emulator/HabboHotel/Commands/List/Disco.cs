﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class MuteBots. This class cannot be inherited.
    /// </summary>
    internal sealed class Disco : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Disco"/> class.
        /// </summary>
        public Disco()
        {
            MinRank = 7;
            Description = "#@#CXW..2PC200 X.D.R";
            Usage = ":disco";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, true)) return true;

            room.DiscoMode = !room.DiscoMode;

            return true;
        }
    }
}