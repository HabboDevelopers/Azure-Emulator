﻿using Azure.HabboHotel.GameClients;
using System.Threading.Tasks;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Shutdown. This class cannot be inherited.
    /// </summary>
    internal sealed class Restart : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shutdown"/> class.
        /// </summary>
        public Restart()
        {
            MinRank = 7;
            Description = "Restart the Server.";
            Usage = ":restart";
            MinParams = 0;
        }


        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, string.Empty, "Restart",
                    "Issued Restart command!");
            new Task(Azure.PerformRestart).Start();
            return true;
        }
    }
}