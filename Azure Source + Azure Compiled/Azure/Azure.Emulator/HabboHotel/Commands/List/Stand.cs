﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Stand. This class cannot be inherited.
    /// </summary>
    internal sealed class Stand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Stand"/> class.
        /// </summary>
        public Stand()
        {
            MinRank = 1;
            Description = "Levante-se.";
            Usage = ":levantar";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;

            if (user.IsSitting)
            {
                user.Statusses.Remove("sit");
                user.IsSitting = false;
                user.UpdateNeeded = true;
            }
            else if (user.IsLyingDown)
            {
                user.Statusses.Remove("lay");
                user.IsLyingDown = false;
                user.UpdateNeeded = true;
            }
            return true;
        }
    }
}