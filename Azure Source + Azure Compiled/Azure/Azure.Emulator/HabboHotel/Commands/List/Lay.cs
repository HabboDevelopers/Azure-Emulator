﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Lay. This class cannot be inherited.
    /// </summary>
    internal sealed class Lay : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lay"/> class.
        /// </summary>
        public Lay()
        {
            MinRank = 5;
            Description = "Makes you lay.";
            Usage = ":lay";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var currentRoom = session.GetHabbo().CurrentRoom;

            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo == null) return true;

            if (roomUserByHabbo.IsSitting || roomUserByHabbo.RidingHorse || roomUserByHabbo.IsWalking ||
                roomUserByHabbo.Statusses.ContainsKey("lay"))
                return true;

            if (roomUserByHabbo.RotBody % 2 != 0) roomUserByHabbo.RotBody--;
            roomUserByHabbo.Statusses.Add("lay", "0.55");
            roomUserByHabbo.IsLyingDown = true;
            roomUserByHabbo.UpdateNeeded = true;
            return true;
        }
    }
}