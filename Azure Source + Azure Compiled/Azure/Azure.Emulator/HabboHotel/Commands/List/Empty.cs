﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Empty. This class cannot be inherited.
    /// </summary>
    internal sealed class Empty : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Empty"/> class.
        /// </summary>
        public Empty()
        {
            MinRank = 3;
            Description = "Limpe o seu inventário.";
            Usage = ":empty";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().GetInventoryComponent().ClearItems();
            return true;
        }
    }
}