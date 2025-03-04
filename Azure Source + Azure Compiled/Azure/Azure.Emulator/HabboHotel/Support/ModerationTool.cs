using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Users;
using Azure.Messages;
using Azure.Messages.Parsers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace Azure.HabboHotel.Support
{
    /// <summary>
    /// Class ModerationTool.
    /// </summary>
    public class ModerationTool
    {
        /// <summary>
        /// The tickets
        /// </summary>
        internal List<SupportTicket> Tickets;

        /// <summary>
        /// The moderation templates
        /// </summary>
        internal Dictionary<uint, ModerationTemplate> ModerationTemplates;

        /// <summary>
        /// The user message presets
        /// </summary>
        internal List<string> UserMessagePresets;

        /// <summary>
        /// The room message presets
        /// </summary>
        internal List<string> RoomMessagePresets;

        /// <summary>
        /// The support ticket hints
        /// </summary>
        internal StringDictionary SupportTicketHints;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationTool"/> class.
        /// </summary>
        internal ModerationTool()
        {
            this.Tickets = new List<SupportTicket>();
            this.UserMessagePresets = new List<string>();
            this.RoomMessagePresets = new List<string>();
            this.SupportTicketHints = new StringDictionary();
            this.ModerationTemplates = new Dictionary<uint, ModerationTemplate>();
        }

        /// <summary>
        /// Sends the ticket update to moderators.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        internal static void SendTicketUpdateToModerators(SupportTicket ticket)
        {
        }

        /// <summary>
        /// Sends the ticket to moderators.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        internal static void SendTicketToModerators(SupportTicket ticket)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolIssueMessageComposer"));
            message = ticket.Serialize(message);
            Azure.GetGame().GetClientManager().StaffAlert(message);
        }

        /// <summary>
        /// Performs the room action.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="kickUsers">if set to <c>true</c> [kick users].</param>
        /// <param name="lockRoom">if set to <c>true</c> [lock room].</param>
        /// <param name="inappropriateRoom">if set to <c>true</c> [inappropriate room].</param>
        /// <param name="message">The message.</param>
        internal static void PerformRoomAction(GameClient modSession, uint roomId, bool kickUsers, bool lockRoom, bool inappropriateRoom, ServerMessage message)
        {
            Room room = Azure.GetGame().GetRoomManager().GetRoom(roomId);
            if (room == null)
            {
                return;
            }
            if (lockRoom)
            {
                room.RoomData.State = 1;
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Format("UPDATE rooms_data SET state = 'locked' WHERE id = {0}", room.RoomId));
                }
            }
            if (inappropriateRoom)
            {
                room.RoomData.Name = "Quarto Inapropriado";
                room.RoomData.Description = "Wulles detectou que esse quarto � inapropriado para o Hotel!";
                room.ClearTags();
                room.RoomData.SerializeRoomData(message, modSession, false, true);
            }
            if (kickUsers)
            {
                room.OnRoomKick();
            }
        }

        /// <summary>
        /// Mods the action result.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="result">if set to <c>true</c> [result].</param>
        internal static void ModActionResult(uint userId, bool result)
        {
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            clientByUserId.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("ModerationActionResultMessageComposer"));
            clientByUserId.GetMessageHandler().GetResponse().AppendInteger(userId);
            clientByUserId.GetMessageHandler().GetResponse().AppendBool(false);
            clientByUserId.GetMessageHandler().SendResponse();
        }

        /// <summary>
        /// Serializes the room tool.
        /// </summary>
        /// <param name="Data">The data.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage SerializeRoomTool(RoomData Data)
        {
            Room room = Azure.GetGame().GetRoomManager().GetRoom(Data.Id);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationRoomToolMessageComposer"));
            serverMessage.AppendInteger(Data.Id);
            serverMessage.AppendInteger(Data.UsersNow);
            if (room != null)
            {
                serverMessage.AppendBool(room.GetRoomUserManager().GetRoomUserByHabbo(Data.Owner) != null);
            }
            else
            {
                serverMessage.AppendBool(false);
            }
            serverMessage.AppendInteger(room != null ? room.RoomData.OwnerId : 0);
            serverMessage.AppendString(Data.Owner);
            serverMessage.AppendBool(room != null);
            serverMessage.AppendString(Data.Name);
            serverMessage.AppendString(Data.Description);
            serverMessage.AppendInteger(Data.TagCount);
            foreach (string current in Data.Tags)
            {
                serverMessage.AppendString(current);
            }
            serverMessage.AppendBool(false);
            return serverMessage;
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="soft">if set to <c>true</c> [soft].</param>
        internal static void KickUser(GameClient modSession, uint userId, string message, bool soft)
        {
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId == null || clientByUserId.GetHabbo().CurrentRoomId < 1u || clientByUserId.GetHabbo().Id == modSession.GetHabbo().Id)
            {
                ModerationTool.ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }
            if (clientByUserId.GetHabbo().Rank >= modSession.GetHabbo().Rank)
            {
                ModerationTool.ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }
            Room room = Azure.GetGame().GetRoomManager().GetRoom(clientByUserId.GetHabbo().CurrentRoomId);
            if (room == null)
            {
                return;
            }
            room.GetRoomUserManager().RemoveUserFromRoom(clientByUserId, true, false);
            clientByUserId.CurrentRoomUserId = -1;
            if (soft)
            {
                return;
            }
            clientByUserId.SendNotif(message);
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("UPDATE users_info SET cautions = cautions + 1 WHERE user_id = {0}", userId));
            }
        }

        /// <summary>
        /// Alerts the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="caution">if set to <c>true</c> [caution].</param>
        internal static void AlertUser(GameClient modSession, uint userId, string message, bool caution)
        {
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId == null)
            {
                return;
            }
            clientByUserId.SendModeratorMessage(message);
        }

        /// <summary>
        /// Locks the trade.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="length">The length.</param>
        internal static void LockTrade(GameClient modSession, uint userId, string message, int length)
        {
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId == null)
            {
                return;
            }
            int num = length;

            {
                if (!clientByUserId.GetHabbo().CheckTrading())
                {
                    num += Azure.GetUnixTimeStamp() - clientByUserId.GetHabbo().TradeLockExpire;
                }
                clientByUserId.GetHabbo().TradeLocked = true;
                clientByUserId.GetHabbo().TradeLockExpire = Azure.GetUnixTimeStamp() + num;
                clientByUserId.SendNotif(message);
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Format("UPDATE users SET trade_lock = '1', trade_lock_expire = '{0}' WHERE id = '{1}'", clientByUserId.GetHabbo().TradeLockExpire, clientByUserId.GetHabbo().Id));
                }
            }
        }

        /// <summary>
        /// Bans the user.
        /// </summary>
        /// <param name="modSession">The mod session.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="length">The length.</param>
        /// <param name="message">The message.</param>
        internal static void BanUser(GameClient modSession, uint userId, int length, string message)
        {
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId == null || clientByUserId.GetHabbo().Id == modSession.GetHabbo().Id)
            {
                ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }
            if (clientByUserId.GetHabbo().Rank >= modSession.GetHabbo().Rank)
            {
                ModActionResult(modSession.GetHabbo().Id, false);
                return;
            }
            double lengthSeconds = length;
            Azure.GetGame().GetBanManager().BanUser(clientByUserId, modSession.GetHabbo().UserName, lengthSeconds, message, false, false);
        }

        /// <summary>
        /// Serializes the user information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">User not found in database.</exception>
        internal static ServerMessage SerializeUserInfo(uint userId)
        {
            {
                ServerMessage result;
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT id, username, online, mail, ip_last, look , rank , trade_lock , trade_lock_expire FROM users WHERE id = " + userId);
                    DataRow row = queryReactor.GetRow();
                    queryReactor.SetQuery("SELECT reg_timestamp, login_timestamp, cfhs, cfhs_abusive, cautions, bans FROM users_info WHERE user_id = " + userId);
                    DataRow row2 = queryReactor.GetRow();
                    if (row == null)
                    {
                        throw new NullReferenceException("User not found in database.");
                    }
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolUserToolMessageComposer"));
                    serverMessage.AppendInteger(Convert.ToUInt32(row["id"]));
                    serverMessage.AppendString((string)row["username"]);
                    serverMessage.AppendString((string)row["look"]);
                    if (row2 != null)
                    {
                        serverMessage.AppendInteger((int)Math.Ceiling((Azure.GetUnixTimeStamp() - (double)row2["reg_timestamp"]) / 60.0));

                        serverMessage.AppendInteger((int)Math.Ceiling((Azure.GetUnixTimeStamp() - (double)row2["login_timestamp"]) / 60.0));
                    }
                    else
                    {
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                    }
                    serverMessage.AppendBool(Azure.GetGame().GetClientManager().GetClientByUserId(Convert.ToUInt32(row["id"])) != null);
                    if (row2 != null)
                    {
                        serverMessage.AppendInteger((int)row2["cfhs"]);
                        serverMessage.AppendInteger((int)row2["cfhs_abusive"]);
                        serverMessage.AppendInteger((int)row2["cautions"]);
                        serverMessage.AppendInteger((int)row2["bans"]);
                    }
                    else
                    {
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendInteger(0);
                    }
                    serverMessage.AppendInteger(0);
                    serverMessage.AppendString((row["trade_lock"].ToString() == "1") ? Azure.UnixToDateTime(int.Parse(row["trade_lock_expire"].ToString())).ToLongDateString() : "Not trade-locked");
                    serverMessage.AppendString(((uint)row["rank"] < 6u) ? ((string)row["ip_last"]) : "127.0.0.1");
                    serverMessage.AppendInteger(Convert.ToUInt32(row["id"]));
                    serverMessage.AppendInteger(0);
                    serverMessage.AppendString(string.Format("E-Mail:         {0}", row["mail"]));
                    serverMessage.AppendString(string.Format("Rank ID:        {0}", (uint)row["rank"]));
                    result = serverMessage;
                }
                return result;
            }
        }

        /// <summary>
        /// Serializes the room visits.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage SerializeRoomVisits(uint userId)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolRoomVisitsMessageComposer"));
            serverMessage.AppendInteger(userId);

            var user = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (user == null || user.GetHabbo() == null)
            {
                serverMessage.AppendString("Not online");
                serverMessage.AppendInteger(0);
                return serverMessage;
            }

            serverMessage.AppendString(user.GetHabbo().UserName);
            serverMessage.StartArray();

            foreach (var roomData in user.GetHabbo()
                .RecentlyVisitedRooms.Select(
                    roomId => Azure.GetGame().GetRoomManager().GenerateRoomData(roomId))
                .Where(roomData => roomData != null))
            {
                serverMessage.AppendInteger(roomData.Id);
                serverMessage.AppendString(roomData.Name);

                serverMessage.AppendInteger(0); //hour
                serverMessage.AppendInteger(0); //min

                serverMessage.SaveArray();
            }

            serverMessage.EndArray();
            return serverMessage;
        }

        /// <summary>
        /// Serializes the user chatlog.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage SerializeUserChatlog(uint userId)
        {
            ServerMessage result;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT DISTINCT room_id FROM users_chatlogs WHERE user_id = {0} ORDER BY timestamp DESC LIMIT 4", userId));
                DataTable table = queryReactor.GetTable();
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolUserChatlogMessageComposer"));
                serverMessage.AppendInteger(userId);
                serverMessage.AppendString(Azure.GetGame().GetClientManager().GetNameById(userId));
                if (table != null)
                {
                    serverMessage.AppendInteger(table.Rows.Count);
                    IEnumerator enumerator = table.Rows.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            var dataRow = (DataRow)enumerator.Current;
                            queryReactor.SetQuery(string.Concat(new object[]
                            {
                                "SELECT user_id,timestamp,message FROM users_chatlogs WHERE room_id = ",
                                (uint)dataRow["room_id"],
                                " AND user_id = ",
                                userId,
                                " ORDER BY timestamp DESC LIMIT 30"
                            }));
                            DataTable table2 = queryReactor.GetTable();
                            RoomData roomData = Azure.GetGame().GetRoomManager().GenerateRoomData((uint)dataRow["room_id"]);
                            if (table2 != null)
                            {
                                serverMessage.AppendByte(1);
                                serverMessage.AppendShort(2);
                                serverMessage.AppendString("roomName");
                                serverMessage.AppendByte(2);
                                serverMessage.AppendString(roomData == null ? "This room was deleted" : roomData.Name);
                                serverMessage.AppendString("roomId");
                                serverMessage.AppendByte(1);
                                serverMessage.AppendInteger((uint)dataRow["room_id"]);
                                serverMessage.AppendShort(table2.Rows.Count);
                                IEnumerator enumerator2 = table2.Rows.GetEnumerator();
                                try
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        var dataRow2 = (DataRow)enumerator2.Current;
                                        Habbo habboForId = Azure.GetHabboById((uint)dataRow2["user_id"]);
                                        Azure.UnixToDateTime((double)dataRow2["timestamp"]);
                                        if (habboForId == null)
                                        {
                                            return null;
                                        }
                                        serverMessage.AppendInteger(((int)(Azure.GetUnixTimeStamp() - (double)dataRow2["timestamp"])));

                                        serverMessage.AppendInteger(habboForId.Id);
                                        serverMessage.AppendString(habboForId.UserName);
                                        serverMessage.AppendString(dataRow2["message"].ToString());
                                        serverMessage.AppendBool(false);
                                    }
                                    continue;
                                }
                                finally
                                {
                                    var disposable = enumerator2 as IDisposable;
                                    if (disposable != null)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }
                            serverMessage.AppendByte(1);
                            serverMessage.AppendShort(0);
                            serverMessage.AppendShort(0);
                        }
                        goto IL_29B;
                    }
                    finally
                    {
                        var disposable2 = enumerator as IDisposable;
                        if (disposable2 != null)
                        {
                            disposable2.Dispose();
                        }
                    }
                }
                serverMessage.AppendInteger(0);
            IL_29B:
                result = serverMessage;
            }
            return result;
        }

        /// <summary>
        /// Serializes the ticket chatlog.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="roomData">The room data.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">No room found.</exception>
        internal static ServerMessage SerializeTicketChatlog(SupportTicket ticket, RoomData roomData, double timestamp)
        {
            var message = new ServerMessage();
            RoomData room = Azure.GetGame().GetRoomManager().GenerateRoomData(ticket.RoomId);
            if (room == null)
            {
                throw new NullReferenceException("No room found.");
            }

            message.Init(LibraryParser.OutgoingRequest("ModerationToolIssueChatlogMessageComposer"));

            message.AppendInteger(ticket.TicketId);
            message.AppendInteger(ticket.SenderId);
            message.AppendInteger(ticket.ReportedId);
            message.AppendInteger(ticket.RoomId);

            message.AppendByte(1);
            message.AppendShort(2);
            message.AppendString("roomName");
            message.AppendByte(2);
            message.AppendString(ticket.RoomName);
            message.AppendString("roomId");
            message.AppendByte(1);
            message.AppendInteger(ticket.RoomId);

            var tempChatlogs = room.RoomChat.Reverse().Skip(Math.Max(0, room.RoomChat.Count() - 60)).Take(60).ToList();

            message.AppendShort(tempChatlogs.Count());
            foreach (var chatLog in tempChatlogs)
            {
                chatLog.Serialize(ref message);
            }
            tempChatlogs = null;

            return message;
        }

        /// <summary>
        /// Serializes the room chatlog.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>ServerMessage.</returns>
        /// <exception cref="System.NullReferenceException">No room found.</exception>
        internal static ServerMessage SerializeRoomChatlog(uint roomId)
        {
            var message = new ServerMessage();
            Room room = Azure.GetGame().GetRoomManager().LoadRoom(roomId);
            if (room == null || room.RoomData == null)
                throw new NullReferenceException("No room found.");

            message.Init(LibraryParser.OutgoingRequest("ModerationToolRoomChatlogMessageComposer"));
            message.AppendByte(1); //type
            message.AppendShort(2);
            message.AppendString("roomName");
            message.AppendByte(2);
            message.AppendString(room.RoomData.Name);
            message.AppendString("roomId");
            message.AppendByte(1);
            message.AppendInteger(room.RoomData.Id);

            var tempChatlogs = room.RoomData.RoomChat.Reverse().Skip(Math.Max(0, room.RoomData.RoomChat.Count() - 60)).Take(60).ToList();

            message.AppendShort(tempChatlogs.Count());
            foreach (var chatLog in tempChatlogs)
            {
                chatLog.Serialize(ref message);
            }
            tempChatlogs = null;

            return message;
        }

        /// <summary>
        /// Serializes the tool.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeTool(GameClient session)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadModerationToolMessageComposer"));
            serverMessage.AppendInteger(Tickets.Count);
            foreach (var current in Tickets) current.Serialize(serverMessage);
            serverMessage.AppendInteger(UserMessagePresets.Count);
            foreach (var current2 in UserMessagePresets) serverMessage.AppendString(current2);

            IEnumerable<ModerationTemplate> enumerable = (from x in ModerationTemplates.Values
                                                          where x.Category == -1
                                                          select x).ToArray();

            serverMessage.AppendInteger(enumerable.Count());
            using (var enumerator3 = enumerable.GetEnumerator())
            {
                var first = true;
                while (enumerator3.MoveNext())
                {
                    var template = enumerator3.Current;
                    IEnumerable<ModerationTemplate> enumerable2 = (from x in ModerationTemplates.Values
                                                                   where x.Category == (long)((ulong)template.Id)
                                                                   select x).ToArray();
                    serverMessage.AppendString(template.CName);
                    serverMessage.AppendBool(first);
                    serverMessage.AppendInteger(enumerable2.Count());
                    foreach (var current3 in enumerable2)
                    {
                        serverMessage.AppendString(current3.Caption);
                        serverMessage.AppendString(current3.BanMessage);
                        serverMessage.AppendInteger(current3.BanHours);
                        serverMessage.AppendInteger(Azure.BoolToInteger(current3.AvatarBan));
                        serverMessage.AppendInteger(Azure.BoolToInteger(current3.Mute));
                        serverMessage.AppendInteger(Azure.BoolToInteger(current3.TradeLock));
                        serverMessage.AppendString(current3.WarningMessage);
                        serverMessage.AppendBool(true); //showHabboWay
                    }
                    first = false;
                }
            }

            // but = button
            serverMessage.AppendBool(true); //ticket_queue_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_chatlogs")); //chatlog_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_alert")); //message_but
            serverMessage.AppendBool(true); //modaction_but
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_ban")); //ban_but
            serverMessage.AppendBool(true);
            serverMessage.AppendBool(session.GetHabbo().HasFuse("fuse_kick")); //kick_but

            serverMessage.AppendInteger(RoomMessagePresets.Count);
            foreach (var current4 in RoomMessagePresets) serverMessage.AppendString(current4);

            return serverMessage;
        }

        /// <summary>
        /// Loads the message presets.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadMessagePresets(IQueryAdapter dbClient)
        {
            this.UserMessagePresets.Clear();
            this.RoomMessagePresets.Clear();
            this.SupportTicketHints.Clear();
            this.ModerationTemplates.Clear();
            dbClient.SetQuery("SELECT type,message FROM moderation_presets WHERE enabled = 2");
            DataTable table = dbClient.GetTable();
            dbClient.SetQuery("SELECT word,hint FROM moderation_tickethints");
            DataTable table2 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM moderation_templates");
            DataTable table3 = dbClient.GetTable();
            if (table == null || table2 == null)
            {
                return;
            }
            foreach (DataRow dataRow in table.Rows)
            {
                var item = (string)dataRow["message"];
                string a = dataRow["type"].ToString().ToLower();

                if (a != "message")
                {
                    if (a == "roommessage")
                    {
                        this.RoomMessagePresets.Add(item);
                    }
                }
                else
                {
                    this.UserMessagePresets.Add(item);
                }
            }
            foreach (DataRow dataRow2 in table2.Rows)
            {
                this.SupportTicketHints.Add((string)dataRow2[0], (string)dataRow2[1]);
            }
            foreach (DataRow dataRow3 in table3.Rows)
            {
                this.ModerationTemplates.Add(uint.Parse(dataRow3["id"].ToString()), new ModerationTemplate(uint.Parse(dataRow3["id"].ToString()), short.Parse(dataRow3["category"].ToString()), dataRow3["cName"].ToString(), dataRow3["caption"].ToString(), dataRow3["warning_message"].ToString(), dataRow3["ban_message"].ToString(), short.Parse(dataRow3["ban_hours"].ToString()), dataRow3["avatar_ban"].ToString() == "1", dataRow3["mute"].ToString() == "1", dataRow3["trade_lock"].ToString() == "1"));
            }
        }

        /// <summary>
        /// Loads the pending tickets.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadPendingTickets(IQueryAdapter dbClient)
        {
            /*dbClient.SetQuery("SELECT * FROM moderation_tickets");
            DataTable table = dbClient.GetTable();
            if (table == null) return;
            foreach (DataRow dataRow in table.Rows)
            {
                var ticket = new SupportTicket((uint)dataRow[0], (int)dataRow[1], (int)dataRow[2], 3, (uint)dataRow[4], (uint)dataRow[5], (string)dataRow[7], (uint)dataRow[8], (string)dataRow[9], (double)dataRow[10], new List<string>());
                this.Tickets.Add(ticket);
                //this.SupportTicketHints.Add((string)dataRow2[0], (string)dataRow2[1]);
            }*/
        }

        /// <summary>
        /// Sends the new ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="category">The category.</param>
        /// <param name="type">The type.</param>
        /// <param name="reportedUser">The reported user.</param>
        /// <param name="message">The message.</param>
        /// <param name="messages">The messages.</param>
        internal void SendNewTicket(GameClient session, int category, int type, uint reportedUser, string message,
            List<string> messages)
        {
            uint id;

            if (session.GetHabbo().CurrentRoomId <= 0)
            {
                using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(string.Concat(new object[]
                    {
                        "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
                        category,
                        "','open','",
                        session.GetHabbo().Id,
                        "','",
                        reportedUser,
                        "','0',@message,'0','','",
                        Azure.GetUnixTimeStamp(),
                        "')"
                    }));
                    dbClient.AddParameter("message", message);
                    id = (uint)dbClient.InsertQuery();
                    dbClient.RunFastQuery(string.Format("UPDATE users_info SET cfhs = cfhs + 1 WHERE user_id = {0}", session.GetHabbo().Id));
                }

                var ticket = new SupportTicket(id, 1, category, type, session.GetHabbo().Id, reportedUser, message, 0u, "",
                    Azure.GetUnixTimeStamp(), messages);
                this.Tickets.Add(ticket);
                SendTicketToModerators(ticket);
                return;
            }

            RoomData data = Azure.GetGame().GetRoomManager().GenerateNullableRoomData(session.GetHabbo().CurrentRoomId);
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Concat(new object[]
                {
                    "INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
                    category,
                    "','open','",
                    session.GetHabbo().Id,
                    "','",
                    reportedUser,
                    "','0',@message,'",
                    data.Id,
                    "',@name,'",
                    Azure.GetUnixTimeStamp(),
                    "')"
                }));
                dbClient.AddParameter("message", message);
                dbClient.AddParameter("name", data.Name);
                id = (uint)dbClient.InsertQuery();
                dbClient.RunFastQuery(string.Format("UPDATE users_info SET cfhs = cfhs + 1 WHERE user_id = {0}", session.GetHabbo().Id));
            }
            var ticket2 = new SupportTicket(id, 1, category, type, session.GetHabbo().Id, reportedUser, message, data.Id, data.Name,
                Azure.GetUnixTimeStamp(), messages);
            this.Tickets.Add(ticket2);
            SendTicketToModerators(ticket2);
        }

        /// <summary>
        /// Serializes the open tickets.
        /// </summary>
        /// <param name="serverMessages">The server messages.</param>
        /// <param name="userId">The user identifier.</param>
        internal void SerializeOpenTickets(ref QueuedServerMessage serverMessages, uint userId)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("ModerationToolIssueMessageComposer"));
            foreach (SupportTicket current in this.Tickets.Where(current => current.Status == TicketStatus.Open || (current.Status == TicketStatus.Picked && current.ModeratorId == userId) || (current.Status == TicketStatus.Picked && current.ModeratorId == 0u)))
            {
                message = current.Serialize(message);
                serverMessages.AppendResponse(message);
            }
        }

        /// <summary>
        /// Gets the ticket.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <returns>SupportTicket.</returns>
        internal SupportTicket GetTicket(uint ticketId)
        {
            return this.Tickets.FirstOrDefault(current => current.TicketId == ticketId);
        }

        /// <summary>
        /// Picks the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        internal void PickTicket(GameClient session, uint ticketId)
        {
            SupportTicket ticket = this.GetTicket(ticketId);
            if (ticket == null || ticket.Status != TicketStatus.Open)
            {
                return;
            }
            ticket.Pick(session.GetHabbo().Id, true);
            SendTicketToModerators(ticket);
        }

        /// <summary>
        /// Releases the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        internal void ReleaseTicket(GameClient session, uint ticketId)
        {
            SupportTicket ticket = this.GetTicket(ticketId);
            if (ticket == null || ticket.Status != TicketStatus.Picked || ticket.ModeratorId != session.GetHabbo().Id)
            {
                return;
            }
            ticket.Release(true);
            SendTicketToModerators(ticket);
        }

        /// <summary>
        /// Closes the ticket.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="result">The result.</param>
        internal void CloseTicket(GameClient session, uint ticketId, int result)
        {
            SupportTicket ticket = this.GetTicket(ticketId);
            if (ticket == null || ticket.Status != TicketStatus.Picked || ticket.ModeratorId != session.GetHabbo().Id)
            {
                return;
            }
            GameClient clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(ticket.SenderId);
            int i;
            TicketStatus newStatus;
            switch (result)
            {
                case 1:
                    i = 1;
                    newStatus = TicketStatus.Invalid;
                    goto IL_9E;
                case 2:
                    i = 2;
                    newStatus = TicketStatus.Abusive;
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.RunFastQuery(string.Format("UPDATE users_info SET cfhs_abusive = cfhs_abusive + 1 WHERE user_id = {0}", ticket.SenderId));
                        goto IL_9E;
                    }
            }
            i = 0;
            newStatus = TicketStatus.Resolved;
        IL_9E:
            if (clientByUserId != null && (ticket.Type != 3 && ticket.Type != 4))
            {
                foreach (SupportTicket current2 in Tickets.FindAll(current => current.ReportedId == ticket.ReportedId && current.Status == TicketStatus.Picked))
                {
                    current2.Delete(true);
                    SendTicketToModerators(current2);
                    current2.Close(newStatus, true);
                }
                clientByUserId.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("ModerationToolUpdateIssueMessageComposer"));
                clientByUserId.GetMessageHandler().GetResponse().AppendInteger(1);
                clientByUserId.GetMessageHandler().GetResponse().AppendInteger(ticket.TicketId);
                clientByUserId.GetMessageHandler().GetResponse().AppendInteger(ticket.ModeratorId);
                clientByUserId.GetMessageHandler()
                              .GetResponse()
                              .AppendString((Azure.GetHabboById(ticket.ModeratorId) != null)
                                            ? Azure.GetHabboById(ticket.ModeratorId).UserName
                                            : "Undefined");
                clientByUserId.GetMessageHandler().GetResponse().AppendBool(false);
                clientByUserId.GetMessageHandler().GetResponse().AppendInteger(0);
                clientByUserId.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("ModerationTicketResponseMessageComposer"));
                clientByUserId.GetMessageHandler().GetResponse().AppendInteger(i);
                clientByUserId.GetMessageHandler().SendResponse();
            }
            using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.RunFastQuery(string.Format("UPDATE users_stats SET tickets_answered = tickets_answered+1 WHERE id={0} LIMIT 1", session.GetHabbo().Id));
            }
        }

        /// <summary>
        /// Userses the has pending ticket.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool UsersHasPendingTicket(uint id)
        {
            return this.Tickets.Any(current => current.SenderId == id && current.Status == TicketStatus.Open);
        }

        /// <summary>
        /// Deletes the pending ticket for user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal void DeletePendingTicketForUser(uint id)
        {
            foreach (SupportTicket current in this.Tickets.Where(current => current.SenderId == id))
            {
                current.Delete(true);
                SendTicketToModerators(current);
                break;
            }
        }

        /// <summary>
        /// Gets the pending ticket for user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>SupportTicket.</returns>
        internal SupportTicket GetPendingTicketForUser(uint id)
        {
            foreach (SupportTicket current in this.Tickets.Where(current => current.SenderId == id && current.Status == TicketStatus.Open))
            {
                return current;
            }
            return null;
        }

        /// <summary>
        /// Logs the staff entry.
        /// </summary>
        /// <param name="modName">Name of the mod.</param>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        /// <param name="description">The description.</param>
        internal void LogStaffEntry(string modName, string target, string type, string description)
        {
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("INSERT INTO server_stafflogs (staffuser,target,action_type,description) VALUES (@Username,@target,@type,@desc)");
                queryReactor.AddParameter("Username", modName);
                queryReactor.AddParameter("target", target);
                queryReactor.AddParameter("type", type);
                queryReactor.AddParameter("desc", description);
                queryReactor.RunQuery();
            }
        }
    }
}