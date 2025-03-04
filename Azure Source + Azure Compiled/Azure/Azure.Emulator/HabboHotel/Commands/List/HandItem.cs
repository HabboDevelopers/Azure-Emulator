﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class HandItem. This class cannot be inherited.
    /// </summary>
    internal sealed class HandItem : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandItem"/> class.
        /// </summary>
        public HandItem()
        {
            MinRank = 1;
            Description = "Lets you pick a hand item, e.g. A drink";
            Usage = ":handitem [ItemId]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            ushort itemId;
            if (!ushort.TryParse(pms[0], out itemId)) return true;

            var user = session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().UserName);
            if (user.RidingHorse)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("horse_handitem_error"));
                return true;
            }
            if (user.IsLyingDown)
                return true;

            user.CarryItem(itemId);
            return true;
        }
    }
}