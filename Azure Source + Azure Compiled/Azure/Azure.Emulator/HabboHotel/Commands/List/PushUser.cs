﻿using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.PathFinding;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class PushUser. This class cannot be inherited.
    /// </summary>
    internal sealed class PushUser : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushUser"/> class.
        /// </summary>
        public PushUser()
        {
            MinRank = 5;
            Description = "Push User.";
            Usage = ":push [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;

            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Id == session.GetHabbo().Id)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_pull_error_own"));
                return true;
            }
            var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            if (user2 == null) return true;
            if (user2.TeleportEnabled)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_error_teleport_enable"));
                return true;
            }

            if (PathFinder.GetDistance(user.X, user.Y, user2.X, user2.Y) > 2)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_pull_error_far_away"));
                return true;
            }

            switch (user.RotBody)
            {
                case 0:
                    user2.MoveTo(user2.X, user2.Y - 1);
                    break;

                case 1:
                    user2.MoveTo(user2.X + 1, user2.Y);
                    user2.MoveTo(user2.X, user2.Y - 1);
                    break;

                case 2:
                    user2.MoveTo(user2.X + 1, user2.Y);
                    break;

                case 3:
                    user2.MoveTo(user2.X + 1, user2.Y);
                    user2.MoveTo(user2.X, user2.Y + 1);
                    break;

                case 4:
                    user2.MoveTo(user2.X, user2.Y + 1);
                    break;

                case 5:
                    user2.MoveTo(user2.X - 1, user2.Y);
                    user2.MoveTo(user2.X, user2.Y + 1);
                    break;

                case 6:
                    user2.MoveTo(user2.X - 1, user2.Y);
                    break;

                case 7:
                    user2.MoveTo(user2.X - 1, user2.Y);
                    user2.MoveTo(user2.X, user2.Y - 1);
                    break;
            }

            user2.UpdateNeeded = true;
            user2.SetRot(PathFinder.CalculateRotation(user2.X, user2.Y, user.GoalX, user.GoalY));
            return true;
        }
    }
}