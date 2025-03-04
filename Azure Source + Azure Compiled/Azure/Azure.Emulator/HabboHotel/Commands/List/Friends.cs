﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class Friends : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sit"/> class.
        /// </summary>
        public Friends()
        {
            MinRank = 1;
            Description = "Permitir/Bloquear novos peidos de amizade.";
            Usage = ":friends";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().HasFriendRequestsDisabled = !session.GetHabbo().HasFriendRequestsDisabled;
            return true;
        }
    }
}