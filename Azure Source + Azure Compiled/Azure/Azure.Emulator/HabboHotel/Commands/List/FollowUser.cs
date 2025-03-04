﻿using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class FollowUser. This class cannot be inherited.
    /// </summary>
    internal sealed class FollowUser : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowUser"/> class.
        /// </summary>
        public FollowUser()
        {
            MinRank = 1;
            Description = "Siga um usuário no hotel.";
            Usage = ":follow [Usuário]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null || client.GetHabbo() == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().CurrentRoom == null ||
                client.GetHabbo().CurrentRoom == session.GetHabbo().CurrentRoom)
                return false;
            var roomFwd =
                new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(client.GetHabbo().CurrentRoom.RoomId);
            session.SendMessage(roomFwd);

            return true;
        }
    }
}