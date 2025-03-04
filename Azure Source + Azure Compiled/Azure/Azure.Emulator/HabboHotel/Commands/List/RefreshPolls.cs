﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RefreshPolls. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshPolls : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshPolls"/> class.
        /// </summary>
        public RefreshPolls()
        {
            MinRank = 7;
            Description = "Refreshes Polls from Database.";
            Usage = ":refresh_polls";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor()) Azure.GetGame().GetPollManager().Init(adapter);
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_polls"));
            return true;
        }
    }
}