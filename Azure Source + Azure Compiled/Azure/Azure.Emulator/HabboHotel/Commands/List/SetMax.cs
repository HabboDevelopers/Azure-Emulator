﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class SetMax. This class cannot be inherited.
    /// </summary>
    internal sealed class SetMax : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetMax"/> class.
        /// </summary>
        public SetMax()
        {
            MinRank = 5;
            Description = "Set max users in a room.";
            Usage = ":setmax [Number from 1 to 200. If 10 < num > 100 requires VIP]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            ushort maxUsers;
            if (!ushort.TryParse(pms[0], out maxUsers) || maxUsers == 0 || maxUsers > 200)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_setmax_error_number"));
                return true;
            }

            if (maxUsers > 100 && !(session.GetHabbo().VIP || session.GetHabbo().HasFuse("fuse_vip_commands")))
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_setmax_error_max"));
                return true;
            }
            if (maxUsers < 10 && !(session.GetHabbo().VIP || session.GetHabbo().HasFuse("fuse_vip_commands")))
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_setmax_error_min"));
                return true;
            }

            session.GetHabbo().CurrentRoom.SetMaxUsers(maxUsers);
            return true;
        }
    }
}