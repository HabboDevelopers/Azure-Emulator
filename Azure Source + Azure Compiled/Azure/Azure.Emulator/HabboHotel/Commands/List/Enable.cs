﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Enable. This class cannot be inherited.
    /// </summary>
    internal sealed class Enable : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Enable"/> class.
        /// </summary>
        public Enable()
        {
            MinRank = 1;
            Description = "Ative um efeito";
            Usage = ":enable";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user =
                session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().UserName);
            if (user.RidingHorse) return true;
            if (user.IsLyingDown) return true;
            ushort effect;

            if (!ushort.TryParse(pms[0], out effect)) return true;
            if (effect == 102 && !session.GetHabbo().HasFuse("fuse_mod")) return true;
            if (effect == 140 && !(session.GetHabbo().VIP || session.GetHabbo().HasFuse("fuse_vip_commands"))) return true;
            if (effect == 178 && !session.GetHabbo().HasFuse("fuse_mod")) return true; // TODO: Need ambassadors

            session.GetHabbo()
                .GetAvatarEffectsInventoryComponent()
                .ActivateCustomEffect(effect);

            return true;
        }
    }
}