﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class Sit : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sit"/> class.
        /// </summary>
        public Sit()
        {
            MinRank = 1;
            Description = "Sente-se.";
            Usage = ":sit";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetMessageHandler().Sit();
            return true;
        }
    }
}