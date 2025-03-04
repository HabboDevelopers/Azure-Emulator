﻿using Azure.Configuration;
using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RefreshItems. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshItems : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshItems"/> class.
        /// </summary>
        public RefreshItems()
        {
            MinRank = 7;
            Description = "Refreshes Items from Database.";
            Usage = ":i";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            FurniDataParser.SetCache();
            Azure.GetGame().Reloaditems();
            FurniDataParser.Clear();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_items"));
            return true;
        }
    }
}