﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class MassDiamonds. This class cannot be inherited.
    /// </summary>
    internal sealed class MassDiamonds : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassDiamonds"/> class.
        /// </summary>
        public MassDiamonds()
        {
            MinRank = 7;
            Description = "Gives all the users online Diamonds.";
            Usage = ":massdiamonds [AMOUNT]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            int amount;
            if (!int.TryParse(pms[0], out amount))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            foreach (GameClient client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                if (client == null || client.GetHabbo() == null) continue;
                var habbo = client.GetHabbo();
                habbo.BelCredits += amount;
                client.GetHabbo().UpdateSeasonalCurrencyBalance();
                client.SendNotif(Azure.GetLanguage().GetVar("command_diamonds_one_give") + amount + (Azure.GetLanguage().GetVar("command_diamonds_two_give")));
            }
            return true;
        }
    }
}