using System;
using System.Data;

namespace Azure.HabboHotel.Users.Inventory
{
    /// <summary>
    /// Class UserPreferences.
    /// </summary>
    internal class UserPreferences
    {
        /// <summary>
        /// The _user identifier
        /// </summary>
        private readonly uint _userId;

        internal bool PreferOldChat;
        internal bool IgnoreRoomInvite;
        internal bool DisableCameraFollow;
        internal string Volume = "100,100,100";
        internal int NewnaviX = 110;
        internal int NewnaviY = 47;
        internal int NewnaviWidth = 425;
        internal int NewnaviHeight = 500;
        internal int ChatColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferences"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal UserPreferences(uint userId)
        {
            _userId = userId;
            DataRow row;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM users_preferences WHERE userid = " + _userId);
                queryReactor.AddParameter("userid", _userId);
                row = queryReactor.GetRow();

                if (row == null)
                {
                    queryReactor.RunFastQuery("REPLACE INTO users_preferences (userid, volume, newnavi_x, newnavi_y, newnavi_width, newnavi_height) VALUES (" + _userId + ", '100,100,100','110','47','425','500')");
                    return;
                }
            }
            PreferOldChat = Azure.EnumToBool((string)row["prefer_old_chat"]);
            IgnoreRoomInvite = Azure.EnumToBool((string)row["ignore_room_invite"]);
            DisableCameraFollow = Azure.EnumToBool((string)row["disable_camera_follow"]);
            Volume = (string)row["volume"];
            NewnaviX = Convert.ToInt32(row["newnavi_x"]);
            NewnaviY = Convert.ToInt32(row["newnavi_y"]);
            NewnaviWidth = Convert.ToInt32(row["newnavi_width"]);
            NewnaviHeight = Convert.ToInt32(row["newnavi_height"]);
            ChatColor = Convert.ToInt32(row["chat_color"]);
        }

        internal void Save()
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "UPDATE users_preferences SET volume = @volume, prefer_old_chat = @prefer_old_chat, ignore_room_invite = @ignore_room_invite, newnavi_x = @newnavi_x, newnavi_y = @newnavi_y, newnavi_width = @newnavi_width, newnavi_height = @newnavi_height, disable_camera_follow = @disable_camera_follow, chat_color = @chat_color WHERE userid = @userid");
                queryReactor.AddParameter("userid", _userId);
                queryReactor.AddParameter("prefer_old_chat", Azure.BoolToEnum(PreferOldChat));
                queryReactor.AddParameter("ignore_room_invite", Azure.BoolToEnum(IgnoreRoomInvite));
                queryReactor.AddParameter("volume", Volume);
                queryReactor.AddParameter("newnavi_x", NewnaviX);
                queryReactor.AddParameter("newnavi_y", NewnaviY);
                queryReactor.AddParameter("newnavi_width", NewnaviWidth);
                queryReactor.AddParameter("newnavi_height", NewnaviHeight);
                queryReactor.AddParameter("disable_camera_follow", Azure.BoolToEnum(DisableCameraFollow));
                queryReactor.AddParameter("chat_color", ChatColor);
                queryReactor.RunQuery();
            }
        }
    }
}