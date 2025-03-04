﻿using System;
using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RedeemCredits.
    /// </summary>
    internal sealed class RedeemCredits : Command
    {
        public RedeemCredits()
        {
            MinRank = 1;
            Description = "Converta suas barras em moedas.";
            Usage = ":redeemcredits";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            try
            {
                session.GetHabbo().GetInventoryComponent().Redeemcredits(session);
                session.SendNotif(Azure.GetLanguage().GetVar("command_redeem_credits"));
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
                session.SendNotif(Azure.GetLanguage().GetVar("command_redeem_credits"));
            }
            return true;
        }
    }
}