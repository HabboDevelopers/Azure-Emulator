﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class GiveDiamonds. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveDiamonds : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GiveDiamonds"/> class.
        /// </summary>
        public GiveDiamonds()
        {
            MinRank = 5;
            Description = "Gives user Diamonds.";
            Usage = ":diamonds [USERNAME] [AMOUNT]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            int amount;
            if (!int.TryParse(pms[1], out amount))
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().BelCredits += amount;
            client.GetHabbo().UpdateSeasonalCurrencyBalance();
            client.SendNotif(string.Format(Azure.GetLanguage().GetVar("staff_gives_diamonds"), session.GetHabbo().UserName, amount));
            return true;
        }
    }
}