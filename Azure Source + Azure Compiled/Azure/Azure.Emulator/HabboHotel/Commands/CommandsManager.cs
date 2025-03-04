﻿using Azure.HabboHotel.Commands.List;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Polls;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using Azure.Messages.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Azure.HabboHotel.Commands
{
    /// <summary>
    /// Class CommandsManager.
    /// </summary>
    public static class CommandsManager
    {
        #region Definitions

        /// <summary>
        /// The commands dictionary
        /// </summary>
        public static SortedDictionary<string, Command> CommandsDictionary = new SortedDictionary<string, Command>();

        /// <summary>
        /// The alias dictionary
        /// </summary>
        public static Dictionary<string, string> AliasDictionary = new Dictionary<string, string>();

        #endregion Definitions

        #region Initialization

        /// <summary>
        /// Registers this instance.
        /// </summary>
        public static void Register()
        {
            #region General

            CommandsDictionary.Add("about", new About());
            CommandsDictionary.Add("copy", new CopyLook());
            CommandsDictionary.Add("moonwalk", new MoonWalk());
            CommandsDictionary.Add("faceless", new FaceLess());
            CommandsDictionary.Add("habnam", new HabNam());
            CommandsDictionary.Add("brilho", new Brilho());
            CommandsDictionary.Add("friends", new Friends());
            CommandsDictionary.Add("comandos", new CommandList());
            CommandsDictionary.Add("diagonal", new DisableDiagonal());
            CommandsDictionary.Add("follow", new FollowUser());
            #endregion General

            #region Currency

            CommandsDictionary.Add("credits", new GiveCredits());
            CommandsDictionary.Add("duckets", new GiveDuckets());
            CommandsDictionary.Add("diamonds", new GiveDiamonds());
            CommandsDictionary.Add("massdiamonds", new MassDiamonds());
            CommandsDictionary.Add("masscredits", new MassCredits());


            #endregion Currency

            #region Room Actions

            CommandsDictionary.Add("sit", new Sit());
            CommandsDictionary.Add("levantar", new Stand());
            CommandsDictionary.Add("deitar", new Lay());
            CommandsDictionary.Add("dance", new Dance());
            CommandsDictionary.Add("pickall", new PickAll());
            CommandsDictionary.Add("pickpets", new PickPets());
            CommandsDictionary.Add("mutebots", new MuteBots());
            CommandsDictionary.Add("mutepets", new MutePets());
            CommandsDictionary.Add("enable", new Enable());
            CommandsDictionary.Add("empty", new Empty());
            CommandsDictionary.Add("unload", new Unload());
            CommandsDictionary.Add("reload", new Unload(true));
            CommandsDictionary.Add("deletargrupo", new DeleteGroup());
            CommandsDictionary.Add("setspeed", new SetSpeed());

            #endregion Room Actions

            #region Staff

            CommandsDictionary.Add("ltd", new LTD());
            CommandsDictionary.Add("n", new RefreshNavigator());
            CommandsDictionary.Add("wroom", new WhisperRoom());
            CommandsDictionary.Add("whotel", new WhisperHotel());
            CommandsDictionary.Add("sayall", new SayAll());
            CommandsDictionary.Add("makepublic", new MakePublic());
            CommandsDictionary.Add("makeprivate", new MakePrivate());
            CommandsDictionary.Add("giverank", new GiveRank());
            CommandsDictionary.Add("refresh_quests", new RefreshQuests());
            CommandsDictionary.Add("refresh_polls", new RefreshPolls());
            CommandsDictionary.Add("refresh_achievements", new RefreshAchievements());
            CommandsDictionary.Add("refresh_groups", new RefreshGroups());
            CommandsDictionary.Add("i", new RefreshItems());
            CommandsDictionary.Add("c", new RefreshCatalogue());
            CommandsDictionary.Add("refresh_ranks", new RefreshRanks());
            CommandsDictionary.Add("refresh_settings", new RefreshSettings());
            CommandsDictionary.Add("refresh_songs", new RefreshSongs());
            CommandsDictionary.Add("bh", new RefreshBannedHotels());
            CommandsDictionary.Add("p", new RefreshPromos());
            CommandsDictionary.Add("refresh_extrathings", new RefreshExtraThings());
            CommandsDictionary.Add("freeze", new Freeze());
            CommandsDictionary.Add("userinfo", new UserInfo());
            CommandsDictionary.Add("roomalert", new RoomAlert());
            CommandsDictionary.Add("ra", new RoomAlert());
            CommandsDictionary.Add("hotelalert", new HotelAlert());
            CommandsDictionary.Add("ha", new HotelAlert());
            CommandsDictionary.Add("staffalert", new StaffAlert());
            CommandsDictionary.Add("sa", new StaffAlert());
            CommandsDictionary.Add("eventha", new EventAlert());
            CommandsDictionary.Add("alert", new Alert());
            CommandsDictionary.Add("kick", new Kick());
            CommandsDictionary.Add("teleport", new TelePort());
            CommandsDictionary.Add("roombadge", new RoomBadge());
            CommandsDictionary.Add("removebadge", new RemoveBadge());
            CommandsDictionary.Add("givebadge", new GiveBadge());
            CommandsDictionary.Add("massbadge", new MassBadge());
            CommandsDictionary.Add("ban", new BanUser());
            CommandsDictionary.Add("unban", new UnBanUser());
            CommandsDictionary.Add("superban", new SuperBan());
            CommandsDictionary.Add("fastwalk", new FastWalk());
            CommandsDictionary.Add("goboom", new GoBoom());
            CommandsDictionary.Add("massenable", new MassEnable());
            CommandsDictionary.Add("massdance", new MassDance());
            CommandsDictionary.Add("makesay", new MakeSay());
            CommandsDictionary.Add("empty_user", new EmptyUser());
            CommandsDictionary.Add("handitem", new HandItem());
            CommandsDictionary.Add("summon", new Summon());
            CommandsDictionary.Add("summonall", new SummonAll());
            CommandsDictionary.Add("unmute", new UnMute());
            CommandsDictionary.Add("mute", new Mute());
            CommandsDictionary.Add("roomunmute", new RoomUnMute());
            CommandsDictionary.Add("roommute", new RoomMute());
            CommandsDictionary.Add("roomkick", new RoomKickUsers());
            CommandsDictionary.Add("override", new Override());
            CommandsDictionary.Add("ipban", new BanUserIp());
            CommandsDictionary.Add("addblackword", new AddBlackWord());
            CommandsDictionary.Add("deleteblackword", new DeleteBlackWord());
            CommandsDictionary.Add("developer", new Developer());
            CommandsDictionary.Add("spull", new SpullUser());
            CommandsDictionary.Add("startquestion", new StartQuestion());
            CommandsDictionary.Add("dc", new DisconnectUser());
            CommandsDictionary.Add("hal", new HotelAlertLink());
            CommandsDictionary.Add("redeemcredits", new RedeemCredits());
            CommandsDictionary.Add("flood", new FloodUser());
            CommandsDictionary.Add("invisible", new GoInvisible());

            #endregion Staff

            #region VIP

            CommandsDictionary.Add("pull", new PullUser());
            CommandsDictionary.Add("push", new PushUser());
            CommandsDictionary.Add("kill", new Kill());
            CommandsDictionary.Add("disco", new Disco());

            #endregion VIP
            //CommandsDictionary.Add("test", new Test());
            UpdateInfo();
        }

        /// <summary>
        /// Updates the information.
        /// </summary>
        public static void UpdateInfo()
        {
            using (var dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT command, description, params, rank, alias FROM server_fuses");
                var commandsTable = dbClient.GetTable();

                foreach (DataRow commandRow in commandsTable.Rows)
                {
                    var key = commandRow["command"].ToString();
                    if (!CommandsDictionary.ContainsKey(key)) continue;

                    var command = CommandsDictionary[key];

                    if (!string.IsNullOrEmpty(commandRow["description"].ToString())) command.Description = commandRow["description"].ToString();
                    if (!string.IsNullOrEmpty(commandRow["params"].ToString())) command.Usage = ':' + key + " [" + commandRow["params"] + "]";
                    if (!string.IsNullOrEmpty(commandRow["alias"].ToString()))
                    {
                        var aliasStr = commandRow["alias"].ToString().Replace(" ", "").Replace(";", ",");
                        foreach (var alias in aliasStr.Split(',').Where(alias => !string.IsNullOrEmpty(alias)))
                        {
                            if (AliasDictionary.ContainsKey(alias))
                            {
                                Out.WriteLine("Duplicate alias key: " + alias, "Azure.HabboHotel.CommandsManager", ConsoleColor.DarkRed);
                                continue;
                            }
                            if (CommandsDictionary.ContainsKey(alias))
                            {
                                Out.WriteLine("An alias cannot have same name as a normal command", "Azure.HabboHotel.CommandsManager", ConsoleColor.DarkRed);
                                continue;
                            }
                            AliasDictionary.Add(alias, key);
                        }
                        command.Alias = aliasStr;
                    }
                    short minRank;
                    if (short.TryParse(commandRow["rank"].ToString(), out minRank)) command.MinRank = minRank;
                }
            }
        }

        #endregion Initialization

        #region Methods

        /// <summary>
        /// Tries the execute.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryExecute(string str, GameClient client)
        {
            if (string.IsNullOrEmpty(str) || client.GetHabbo() == null || !client.GetHabbo().InRoom) return false;

            var pms = str.Split(' ');
            var commandName = pms[0];

            if (AliasDictionary.ContainsKey(commandName)) commandName = AliasDictionary[commandName];

            if (!CommandsDictionary.ContainsKey(commandName)) return false;
            var command = CommandsDictionary[commandName];

            if (!CanUse(command.MinRank, client)) return false;

            if (command.MinParams == -2 || (command.MinParams == -1 && pms.Count() > 1) || command.MinParams != -1 && command.MinParams == pms.Count() - 1)
            {
                return command.Execute(client, pms.Skip(1).ToArray());
            }
            client.SendWhisper(Azure.GetLanguage().GetVar("use_the_command_as") + command.Usage);
            return true;
        }

        /// <summary>
        /// Determines whether this instance can use the specified minimum rank.
        /// </summary>
        /// <param name="minRank">The minimum rank.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if this instance can use the specified minimum rank; otherwise, <c>false</c>.</returns>
        public static bool CanUse(short minRank, GameClient user)
        {
            var habbo = user.GetHabbo();

            var userRank = habbo.Rank;
            var staff = habbo.HasFuse("fuse_any_room_controller");

            switch (minRank)
            {
                case -3:
                    return habbo.HasFuse("fuse_vip_commands") || habbo.VIP;

                case -2:
                    return staff ||
                           habbo.CurrentRoom.RoomData.OwnerId == habbo.Id;

                case -1:
                    return staff || habbo.CurrentRoom.RoomData.OwnerId == habbo.Id || habbo.CurrentRoom.CheckRights(user);

                case 0: //disabled
                    return false;
            }
            return userRank >= minRank;
        }
        #endregion Methods
    }
}