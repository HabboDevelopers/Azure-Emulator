﻿using Azure.Configuration;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.Pets;
using Azure.HabboHotel.Quests;
using Azure.HabboHotel.RoomBots;
using Azure.HabboHotel.SoundMachine;
using Azure.Messages;
using Azure.Messages.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace Azure.HabboHotel.Catalogs
{
    /// <summary>
    /// Class Catalog.
    /// </summary>
    class Catalog
    {
        /// <summary>
        /// The last sent offer
        /// </summary>
        internal static int LastSentOffer;
        /// <summary>
        /// The categories
        /// </summary>
        internal HybridDictionary Categories;
        /// <summary>
        /// The offers
        /// </summary>
        internal HybridDictionary Offers;
        /// <summary>
        /// The flat offers
        /// </summary>
        internal Dictionary<int, uint> FlatOffers;
        /// <summary>
        /// The habbo club items
        /// </summary>
        internal List<CatalogItem> HabboClubItems;
        /// <summary>
        /// The ecotron rewards
        /// </summary>
        internal List<EcotronReward> EcotronRewards;
        /// <summary>
        /// The ecotron levels
        /// </summary>
        internal List<int> EcotronLevels;

        /// <summary>
        /// Checks the name of the pet.
        /// </summary>
        /// <param name="petName">Name of the pet.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool CheckPetName(string petName)
        {
            return petName.Length >= 3 && petName.Length <= 15 && Azure.IsValidAlphaNumeric(petName);
        }

        /// <summary>
        /// Creates the bot.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="look">The look.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="bartender">if set to <c>true</c> [bartender].</param>
        /// <returns>RoomBot.</returns>
        internal static RoomBot CreateBot(uint userId, string name, string look, string motto, string gender, bool bartender)
        {
            uint botId;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("INSERT INTO bots (user_id,name,motto,look,gender,walk_mode,is_bartender) VALUES (@h_user,@b_name,@b_motto,@b_look,@b_gender,@b_walk,@b_bartender)");
                queryReactor.AddParameter("h_user", userId);
                queryReactor.AddParameter("b_name", name);
                queryReactor.AddParameter("b_motto", motto);
                queryReactor.AddParameter("b_look", look);
                queryReactor.AddParameter("b_gender", gender);
                queryReactor.AddParameter("b_walk", "freeroam");
                queryReactor.AddParameter("b_bartender", bartender ? "1" : "0");
                botId = Convert.ToUInt32(queryReactor.InsertQuery());
            }

            return new RoomBot(botId, userId, 0u, AIType.Generic, "freeroam", name, motto, look, 0, 0, 0.0, 0, 0, 0, 0, 0, null, null, gender, 0, bartender);
        }

        /// <summary>
        /// Creates the pet.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="Race">The race.</param>
        /// <param name="Color">The color.</param>
        /// <param name="Rarity">The rarity.</param>
        /// <returns>Pet.</returns>
        internal static Pet CreatePet(uint userId, string name, int type, string Race, string Color, int Rarity = 0)
        {
            var pet = new Pet(404u, userId, 0u, name, (uint)type, Race, Color, 0, 100, 100, 0, Azure.GetUnixTimeStamp(), 0, 0, 0.0, false, 0, 0, -1, Rarity, DateTime.Now.AddHours(36.0), DateTime.Now.AddHours(48.0), null)
            {
                DbState = DatabaseUpdateState.NeedsUpdate
            };
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat(new object[]
                    {
                        "INSERT INTO bots (user_id,name, ai_type) VALUES (",
                        pet.OwnerId,
                        ",@",
                        pet.PetId,
                        "name, 'pet')"
                    }));

                queryReactor.AddParameter(string.Format("{0}name", pet.PetId), pet.Name);

                pet.PetId = (uint)queryReactor.InsertQuery();

                queryReactor.SetQuery(string.Concat(new object[]
                    {
                        "INSERT INTO pets_data (id,type,race,color,experience,energy,createstamp,rarity,lasthealth_stamp,untilgrown_stamp) VALUES (",
                        pet.PetId,
                        ", ",
                        pet.Type,
                        ",@",
                        pet.PetId,
                        "race,@",
                        pet.PetId,
                        "color,0,100,UNIX_TIMESTAMP(), ",
                        Rarity,
                        ", UNIX_TIMESTAMP(now() + INTERVAL 36 HOUR), UNIX_TIMESTAMP(now() + INTERVAL 48 HOUR))"
                    }));

                queryReactor.AddParameter(string.Format("{0}race", pet.PetId), pet.Race);
                queryReactor.AddParameter(string.Format("{0}color", pet.PetId), pet.Color);
                queryReactor.RunQuery();
            }

            if (pet.Type == 16u)
            {
                pet.MoplaBreed = MoplaBreed.CreateMonsterplantBreed(pet);
                pet.Name = pet.MoplaBreed.Name;
                pet.DbState = DatabaseUpdateState.NeedsUpdate;
            }

            var clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (clientByUserId != null)
                Azure.GetGame().GetAchievementManager().ProgressUserAchievement(clientByUserId, "ACH_PetLover", 1, false);

            return pet;
        }

        /// <summary>
        /// Generates the pet from row.
        /// </summary>
        /// <param name="Row">The row.</param>
        /// <param name="mRow">The m row.</param>
        /// <returns>Pet.</returns>
        internal static Pet GeneratePetFromRow(DataRow Row, DataRow mRow)
        {
            if (Row == null) return null;
            MoplaBreed moplaBreed = null;

            if (Convert.ToUInt32(mRow["type"]) == 16u)
            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(string.Format("SELECT * FROM pets_plants WHERE pet_id = {0}", Convert.ToUInt32(Row["id"])));
                    var row = queryReactor.GetRow();
                    moplaBreed = new MoplaBreed(row);
                }
            }

            return new Pet(Convert.ToUInt32(Row["id"]), Convert.ToUInt32(Row["user_id"]),
                Convert.ToUInt32(Row["room_id"]), (string)Row["name"], Convert.ToUInt32(mRow["type"]),
                (string)mRow["race"], (string)mRow["color"], (int)mRow["experience"], (int)mRow["energy"],
                (int)mRow["nutrition"], (int)mRow["respect"], Convert.ToDouble(mRow["createstamp"]), (int)Row["x"],
                (int)Row["y"], Convert.ToDouble(Row["z"]), (int)mRow["have_saddle"] == 1, (int)mRow["anyone_ride"],
                (int)mRow["hairdye"], (int)mRow["pethair"], (int)mRow["rarity"],
                Azure.UnixToDateTime((int)mRow["lasthealth_stamp"]),
                Azure.UnixToDateTime((int)mRow["untilgrown_stamp"]), moplaBreed);
        }

        /// <summary>
        /// Gets the item from offer.
        /// </summary>
        /// <param name="offerId">The offer identifier.</param>
        /// <returns>CatalogItem.</returns>
        internal CatalogItem GetItemFromOffer(int offerId)
        {
            CatalogItem result = null;
            if (FlatOffers.ContainsKey(offerId))
                result = (CatalogItem)Offers[FlatOffers[offerId]];
            return result ?? (Azure.GetGame().GetCatalog().GetItem(Convert.ToUInt32(offerId)));
        }

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="pageLoaded">The page loaded.</param>
        internal void Initialize(IQueryAdapter dbClient, out uint pageLoaded)
        {
            Initialize(dbClient);
            pageLoaded = (uint)Categories.Count;
        }

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            Categories = new HybridDictionary();
            Offers = new HybridDictionary();
            FlatOffers = new Dictionary<int, uint>();
            EcotronRewards = new List<EcotronReward>();
            EcotronLevels = new List<int>();
            HabboClubItems = new List<CatalogItem>();

            dbClient.SetQuery("SELECT * FROM catalog_items ORDER BY order_num ASC");
            var table = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM catalog_pages ORDER BY order_num ASC");
            var table2 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM catalog_ecotron ORDER BY reward_level ASC");
            var table3 = dbClient.GetTable();
            dbClient.SetQuery("SELECT * FROM catalog_items WHERE specialName LIKE 'HABBO_CLUB_VIP%'");
            var table4 = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    if (string.IsNullOrEmpty(dataRow["item_names"].ToString()) || string.IsNullOrEmpty(dataRow["amounts"].ToString()))
                        continue;

                    var source = dataRow["item_names"].ToString();
                    var firstItem = dataRow["item_names"].ToString().Split(';')[0];

                    Item item;

                    if (!Azure.GetGame().GetItemManager().GetItem(firstItem, out item))
                        continue;

                    var num = !source.Contains(';') ? item.FlatId : -1;
                    if (!dataRow.IsNull("specialName"))
                        item.PublicName = (string)dataRow["specialName"];

                    var catalogItem = new CatalogItem(dataRow, item.PublicName);

                    if (catalogItem.GetFirstBaseItem() == null)
                        continue;

                    Offers.Add(catalogItem.Id, catalogItem);

                    if (num != -1 && !FlatOffers.ContainsKey(num))
                        FlatOffers.Add(num, catalogItem.Id);
                }
            }
            if (table2 != null)
            {
                foreach (DataRow dataRow2 in table2.Rows)
                {
                    var visible = false;
                    var enabled = false;
                    var comingSoon = false;
                    if (dataRow2["visible"].ToString() == "1")
                        visible = true;
                    if (dataRow2["enabled"].ToString() == "1")
                        enabled = true;
                    Categories.Add(Convert.ToUInt16(dataRow2["id"]),
                        new CatalogPage(Convert.ToUInt16(dataRow2["id"]), short.Parse(dataRow2["parent_id"].ToString()),
                            (string)dataRow2["code_name"], (string)dataRow2["caption"], visible, enabled, comingSoon,
                            Convert.ToUInt32(dataRow2["min_rank"]), (int)dataRow2["icon_image"],
                            (string)dataRow2["page_layout"], (string)dataRow2["page_headline"],
                            (string)dataRow2["page_teaser"], (string)dataRow2["page_special"],
                            (string)dataRow2["page_text1"], (string)dataRow2["page_text2"],
                            (string)dataRow2["page_text_details"], (string)dataRow2["page_text_teaser"],
                            (string)dataRow2["page_link_description"], (string)dataRow2["page_link_pagename"],
                            (int)dataRow2["order_num"], ref Offers));
                }
            }

            if (table3 != null)
            {
                foreach (DataRow dataRow3 in table3.Rows)
                {
                    EcotronRewards.Add(new EcotronReward(Convert.ToUInt32(dataRow3["display_id"]),
                        Convert.ToUInt32(dataRow3["item_id"]), Convert.ToUInt32(dataRow3["reward_level"])));
                    if (!EcotronLevels.Contains(Convert.ToInt16(dataRow3["reward_level"]))) EcotronLevels.Add(Convert.ToInt16(dataRow3["reward_level"]));
                }
            }

            if (table4 != null)
            {
                foreach (DataRow row in table4.Rows)
                {
                    HabboClubItems.Add(new CatalogItem(row, row.IsNull("specialName") ? "Habbo VIP" : (string)row["specialName"]));
                }
            }
            return;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>CatalogItem.</returns>
        internal CatalogItem GetItem(uint itemId)
        {
            return Offers.Contains(itemId) ? (CatalogItem)Offers[itemId] : null;
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>CatalogPage.</returns>
        internal CatalogPage GetPage(ushort page)
        {
            return !Categories.Contains(page) ? null : (CatalogPage)Categories[page];
        }

        /// <summary>
        /// Handles the purchase.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="priceAmount">The price amount.</param>
        /// <param name="isGift">if set to <c>true</c> [is gift].</param>
        /// <param name="giftUser">The gift user.</param>
        /// <param name="giftMessage">The gift message.</param>
        /// <param name="giftSpriteId">The gift sprite identifier.</param>
        /// <param name="giftLazo">The gift lazo.</param>
        /// <param name="giftColor">Color of the gift.</param>
        /// <param name="undef">if set to <c>true</c> [undef].</param>
        /// <param name="group">The group.</param>
        internal void HandlePurchase(GameClient session, ushort pageId, int itemId, string extraData, int priceAmount,
            bool isGift, string giftUser, string giftMessage, int giftSpriteId, int giftLazo, int giftColor, bool undef,
            uint @group)
        {
            if (priceAmount < 1 || priceAmount > 100) priceAmount = 1;
            var totalPrice = priceAmount;
            var limitedId = 0;
            var limtot = 0;
            CatalogItem item;
            uint toUserId;

            {
                if (priceAmount >= 6) totalPrice -= Convert.ToInt32(Math.Ceiling(Convert.ToDouble(priceAmount) / 6)) * 2 - 1;

                if (!Categories.Contains(pageId)) return;
                var catalogPage = (CatalogPage)Categories[pageId];
                if (catalogPage == null || !catalogPage.Enabled || !catalogPage.Visible || session == null ||
                    session.GetHabbo() == null) return;
                if (catalogPage.MinRank > session.GetHabbo().Rank || catalogPage.Layout == "sold_ltd_items") return;
                item = catalogPage.GetItem(itemId);
                if (item == null) return;
                if (catalogPage.Layout == "vip_buy" || catalogPage.Layout == "club_buy" || HabboClubItems.Contains(item))
                {
                    if (session.GetHabbo().Credits < item.CreditsCost) return;
                    if (session.GetHabbo().ActivityPoints < item.DucketsCost) return;
                    if (session.GetHabbo().BelCredits < item.BelCreditsCost) return;
                    var array = item.Name.Split('_');
                    double dayLength;
                    if (item.Name.Contains("DAY")) dayLength = double.Parse(array[3]);
                    else if (item.Name.Contains("MONTH"))
                    {
                        var num4 = double.Parse(array[3]);
                        dayLength = Math.Ceiling((num4 * 31) - 0.205);
                    }
                    else if (item.Name.Contains("YEAR"))
                    {
                        var num5 = double.Parse(array[3]);
                        dayLength = (num5 * 31 * 12);
                    }
                    else dayLength = 31;
                    session.GetHabbo().GetSubscriptionManager().AddSubscription(dayLength);
                    if (item.CreditsCost > 0)
                    {
                        session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                        session.GetHabbo().UpdateCreditsBalance();
                    }
                    if (item.DucketsCost > 0)
                    {
                        session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                        session.GetHabbo().UpdateActivityPointsBalance();
                    }
                    if (item.BelCreditsCost > 0)
                    {
                        session.GetHabbo().BelCredits -= (int)item.BelCreditsCost * totalPrice;
                        session.GetHabbo().UpdateSeasonalCurrencyBalance();
                    }
                    return;
                }
                if (item.Name == "room_ad_plus_badge") return;
                if (item.ClubOnly && !session.GetHabbo().GetSubscriptionManager().HasSubscription)
                {
                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("CatalogPurchaseNotAllowedMessageComposer"));
                    serverMessage.AppendInteger(1);
                    session.SendMessage(serverMessage);
                    return;
                }
                var flag =
                    item.Items.Keys.Any(
                        current =>
                            InteractionTypes.AreFamiliar(GlobalInteractions.Pet, current.InteractionType));

                if (!flag &&
                    (item.CreditsCost * totalPrice < 0 || item.DucketsCost * totalPrice < 0 ||
                     item.BelCreditsCost * totalPrice < 0 ||
                     item.LoyaltyCost * totalPrice < 0)) return;
                if (item.IsLimited)
                {
                    totalPrice = 1;
                    priceAmount = 1;
                    if (item.LimitedSelled >= item.LimitedStack)
                    {
                        session.SendMessage(
                            new ServerMessage(LibraryParser.OutgoingRequest("CatalogLimitedItemSoldOutMessageComposer")));
                        return;
                    }
                    item.LimitedSelled++;
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.RunFastQuery(string.Concat(new object[]
                        {
                            "UPDATE catalog_items SET limited_sells = ",
                            item.LimitedSelled,
                            " WHERE id = ",
                            item.Id
                        }));
                        limitedId = item.LimitedSelled;
                        limtot = item.LimitedStack;
                    }
                }
                else if (isGift & priceAmount > 1)
                {
                    totalPrice = 1;
                    priceAmount = 1;
                }
                toUserId = 0u;
                if (session.GetHabbo().Credits < item.CreditsCost * totalPrice) return;
                if (session.GetHabbo().ActivityPoints < item.DucketsCost * totalPrice) return;
                if (session.GetHabbo().BelCredits < item.BelCreditsCost * totalPrice) return;
                if (session.GetHabbo().BelCredits < item.LoyaltyCost * totalPrice) return;
                if (item.CreditsCost > 0 && !isGift)
                {
                    session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                    session.GetHabbo().UpdateCreditsBalance();
                }
                if (item.DucketsCost > 0 && !isGift)
                {
                    session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                    session.GetHabbo().UpdateActivityPointsBalance();
                }
                if (item.BelCreditsCost > 0 && !isGift)
                {
                    session.GetHabbo().BelCredits -= (int)item.BelCreditsCost * totalPrice;
                    session.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
                if (item.LoyaltyCost > 0 && !isGift)
                {
                    session.GetHabbo().BelCredits -= (int)item.LoyaltyCost * totalPrice;
                    session.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
            }
            foreach (var baseItem in item.Items.Keys)
            {
                if (isGift)
                {
                    if ((DateTime.Now - session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
                    {
                        session.SendNotif(Azure.GetLanguage().GetVar("user_send_gift"));
                        return;
                    }
                    if (!baseItem.AllowGift) return;
                    DataRow row;
                    using (var queryreactor3 = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryreactor3.SetQuery("SELECT id FROM users WHERE username = @gift_user");
                        queryreactor3.AddParameter("gift_user", giftUser);
                        row = queryreactor3.GetRow();
                    }
                    if (row == null)
                    {
                        session.GetMessageHandler()
                            .GetResponse()
                            .Init(LibraryParser.OutgoingRequest("GiftErrorMessageComposer"));
                        session.GetMessageHandler().GetResponse().AppendString(giftUser);
                        session.GetMessageHandler().SendResponse();
                        return;
                    }
                    toUserId = Convert.ToUInt32(row["id"]);
                    if (toUserId == 0u)
                    {
                        session.GetMessageHandler()
                            .GetResponse()
                            .Init(LibraryParser.OutgoingRequest("GiftErrorMessageComposer"));
                        session.GetMessageHandler().GetResponse().AppendString(giftUser);
                        session.GetMessageHandler().SendResponse();
                        return;
                    }
                    if (item.CreditsCost > 0 && isGift)
                    {
                        session.GetHabbo().Credits -= (int)item.CreditsCost * totalPrice;
                        session.GetHabbo().UpdateCreditsBalance();
                    }
                    if (item.DucketsCost > 0 && isGift)
                    {
                        session.GetHabbo().ActivityPoints -= (int)item.DucketsCost * totalPrice;
                        session.GetHabbo().UpdateActivityPointsBalance();
                    }
                    if (item.BelCreditsCost > 0 && isGift)
                    {
                        session.GetHabbo().BelCredits -= (int)item.BelCreditsCost * totalPrice;
                        session.GetHabbo().UpdateSeasonalCurrencyBalance();
                    }
                    if (item.LoyaltyCost > 0 && isGift)
                    {
                        session.GetHabbo().BelCredits -= (int)item.LoyaltyCost * totalPrice;
                        session.GetHabbo().UpdateSeasonalCurrencyBalance();
                    }
                }
                if (isGift && baseItem.Type == 'e')
                {
                    session.SendNotif(Azure.GetLanguage().GetVar("user_send_gift_effect"));
                    return;
                }
                if (item.Name.StartsWith("builders_club_addon_"))
                {
                    int furniAmount =
                        Convert.ToInt32(item.Name.Replace("builders_club_addon_", "").Replace("furnis", ""));
                    session.GetHabbo().BuildersItemsMax += furniAmount;
                    var update =
                        new ServerMessage(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer"));
                    update.AppendInteger(session.GetHabbo().BuildersExpire);
                    update.AppendInteger(session.GetHabbo().BuildersItemsMax);
                    update.AppendInteger(2);
                    session.SendMessage(update);
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery("UPDATE users SET builders_items_max = @max WHERE id = @userId");
                        queryReactor.AddParameter("max", session.GetHabbo().BuildersItemsMax);
                        queryReactor.AddParameter("userId", session.GetHabbo().Id);
                        queryReactor.RunQuery();
                    }
                    session.SendMessage(CatalogPacket.PurchaseOk(item, item.Items));
                    session.SendNotif("${notification.builders_club.membership_extended.message}",
                        "${notification.builders_club.membership_extended.title}", "builders_club_membership_extended");
                    return;
                }
                if (item.Name.StartsWith("builders_club_time_"))
                {
                    int timeAmount = Convert.ToInt32(item.Name.Replace("builders_club_time_", "").Replace("seconds", ""));
                    session.GetHabbo().BuildersExpire += timeAmount;
                    var update =
                        new ServerMessage(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer"));
                    update.AppendInteger(session.GetHabbo().BuildersExpire);
                    update.AppendInteger(session.GetHabbo().BuildersItemsMax);
                    update.AppendInteger(2);
                    session.SendMessage(update);
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery("UPDATE users SET builders_expire = @max WHERE id = @userId");
                        queryReactor.AddParameter("max", session.GetHabbo().BuildersExpire);
                        queryReactor.AddParameter("userId", session.GetHabbo().Id);
                        queryReactor.RunQuery();
                    }
                    session.SendMessage(CatalogPacket.PurchaseOk(item, item.Items));
                    session.SendNotif("${notification.builders_club.membership_extended.message}",
                        "${notification.builders_club.membership_extended.title}", "builders_club_membership_extended");
                    return;
                }
                var text = string.Empty;
                var interactionType = baseItem.InteractionType;

                switch (interactionType)
                {
                    case Interaction.None:
                    case Interaction.Gate:
                    case Interaction.Bed:
                    case Interaction.Guillotine:
                    case Interaction.HCGate:
                    case Interaction.ScoreBoard:
                    case Interaction.VendingMachine:
                    case Interaction.Alert:
                    case Interaction.OneWayGate:
                    case Interaction.LoveShuffler:
                    case Interaction.HabboWheel:
                    case Interaction.Dice:
                    case Interaction.Bottle:
                    case Interaction.Hopper:
                    case Interaction.Teleport:
                    case Interaction.QuickTeleport:
                    case Interaction.Pet:
                    case Interaction.Pool:
                    case Interaction.Roller:
                    case Interaction.FootballGate:
                        break;
                    case Interaction.PostIt:
                        extraData = "FFFF33";
                        break;
                    case Interaction.RoomEffect:
                        {
                            double number = 0;
                            try
                            {
                                number = string.IsNullOrEmpty(extraData)
                                    ? 0.0
                                    : double.Parse(extraData, Azure.CultureInfo);
                            }
                            catch (Exception pException)
                            {
                                Logging.HandleException(pException,
                                    string.Format("Catalog.HandlePurchase: {0}", extraData));
                            }
                            extraData = number.ToString().Replace(',', '.');
                            break;
                        }
                    case Interaction.Dimmer:
                        extraData = "1,1,1,#000000,255";
                        break;

                    case Interaction.Trophy:
                        extraData = string.Concat(session.GetHabbo().UserName, Convert.ToChar(9), DateTime.Now.Day, "-", DateTime.Now.Month, "-", DateTime.Now.Year, Convert.ToChar(9), extraData);
                        break;

                    case Interaction.Rentals:
                        extraData = item.ExtraData;
                        break;
                    case Interaction.PetDog:
                    case Interaction.PetCat:
                    case Interaction.PetCrocodile:
                    case Interaction.PetTerrier:
                    case Interaction.PetBear:
                    case Interaction.PetPig:
                    case Interaction.PetLion:
                    case Interaction.PetRhino:
                    case Interaction.PetSpider:
                    case Interaction.PetTurtle:
                    case Interaction.PetChick:
                    case Interaction.PetFrog:
                    case Interaction.PetDragon:
                    case Interaction.PetHorse:
                    case Interaction.PetMonkey:
                    case Interaction.PetGnomo:
                    case Interaction.PetMonsterPlant:
                    case Interaction.PetWhiteRabbit:
                    case Interaction.PetEvilRabbit:
                    case Interaction.PetLoveRabbit:
                    case Interaction.PetCafeRabbit:
                    case Interaction.PetPigeon:
                    case Interaction.PetEvilPigeon:
                    case Interaction.PetDemonMonkey:
                    case Interaction.PetFools:
                    case Interaction.Pet25:
                    case Interaction.Pet26:
                    case Interaction.Pet27:
                    case Interaction.Pet28:
                    case Interaction.Pet29:
                    case Interaction.Pet30:
                    case Interaction.Pet31:
                    case Interaction.Pet32:
                    case Interaction.Pet33:
                    case Interaction.Pet34:
                        try
                        {
                            var data = extraData.Split('\n');
                            var petName = data[0];
                            var race = data[1];
                            var color = data[2];
                            int.Parse(race);
                            if (!CheckPetName(petName)) return;
                            if (race.Length != 1 && race.Length != 2) return;
                            if (color.Length != 6) return;
                            Azure.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(session, "ACH_PetLover", 1, false);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logging.HandleException(ex, "Catalog.HandlePurchase");
                            return;
                        }
                    default:
                        switch (interactionType)
                        {
                            case Interaction.Mannequin:
                                extraData = string.Concat("m", Convert.ToChar(5), "ch-215-92.lg-3202-1322-73", Convert.ToChar(5), "Mannequin");
                                break;

                            case Interaction.VipGate:
                            case Interaction.MysteryBox:
                            case Interaction.YoutubeTv:
                            case Interaction.TileStackMagic:
                            case Interaction.Tent:
                            case Interaction.BedTent:
                                break;
                            case Interaction.BadgeDisplay:
                                if (!session.GetHabbo().GetBadgeComponent().HasBadge(extraData)) extraData = "UMAD";
                                break;

                            case Interaction.FootballGate:
                                extraData = "hd-99999-99999.lg-270-62;hd-99999-99999.ch-630-62.lg-695-62";
                                break;

                            case Interaction.LoveLock:
                                extraData = "0";
                                break;

                            case Interaction.Pinata:
                            case Interaction.RunWaySage:
                            case Interaction.Shower:
                                extraData = "0";
                                break;

                            case Interaction.GroupForumTerminal:
                            case Interaction.GuildItem:
                            case Interaction.GuildGate:
                            case Interaction.GuildForum:
                            case Interaction.Poster:
                                break;

                            case Interaction.Moplaseed:
                                extraData = new Random().Next(0, 12).ToString();
                                break;

                            case Interaction.MusicDisc:
                                var song = SongManager.GetSongById(item.SongId);
                                if (song != null)
                                    extraData = string.Concat(session.GetHabbo().UserName, '\n', DateTime.Now.Year, '\n',
                                        DateTime.Now.Month, '\n', DateTime.Now.Day, '\n', song.LengthSeconds, '\n',
                                        song.Name);
                                break;

                            default:
                                extraData = item.ExtraData;
                                break;
                        }
                        break;
                }

                session.GetMessageHandler()
                    .GetResponse()
                    .Init(LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer"));
                session.GetMessageHandler().SendResponse();
                session.SendMessage(CatalogPacket.PurchaseOk(item, item.Items));
                if (isGift)
                {
                    var itemBySprite = Azure.GetGame()
                        .GetItemManager()
                        .GetItemBySprite(giftSpriteId, 's');

                    if (itemBySprite == null) return;
                    uint insertId;
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery("INSERT INTO items_rooms (base_item,user_id) VALUES (" + itemBySprite.ItemId + ", " + toUserId + ")");
                        insertId = (uint)queryReactor.InsertQuery();
                        queryReactor.SetQuery(string.Concat("INSERT INTO users_gifts (gift_id,item_id,extradata,giver_name,Message,ribbon,color,gift_sprite,show_sender,rare_id) VALUES (", insertId, ", ", baseItem.ItemId, ",@extradata, @name, @Message,", giftLazo, ",", giftColor, ",", giftSpriteId, ",", undef ? 1 : 0, ",", limitedId, ")"));
                        queryReactor.AddParameter("extradata", extraData);
                        queryReactor.AddParameter("name", giftUser);
                        queryReactor.AddParameter("message", giftMessage);
                        queryReactor.RunQuery();
                        if (session.GetHabbo().Id != toUserId)
                        {
                            Azure.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(session, "ACH_GiftGiver", 1, false);
                            Azure.GetGame()
                                .GetQuestManager()
                                .ProgressUserQuest(session, QuestType.GiftOthers, 0u);

                            queryReactor.RunFastQuery("UPDATE users_stats SET gifts_given = gifts_given + 1 WHERE id = " + session.GetHabbo().Id +
                                    ";UPDATE users_stats SET gifts_received = gifts_received + 1 WHERE id = " + toUserId);
                        }
                    }

                    var clientByUserId =
                    Azure.GetGame().GetClientManager().GetClientByUserId(toUserId);
                    if (clientByUserId != null)
                    {
                        clientByUserId.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(insertId, itemBySprite.ItemId, string.Concat(session.GetHabbo().Id, (char)9, giftMessage, (char)9, giftLazo, (char)9, giftColor, (char)9, ((undef) ? "1" : "0"), (char)9, session.GetHabbo().UserName, (char)9, session.GetHabbo().Look, (char)9, item.Name), 0u, false, false, 0, 0, "");
                        if (clientByUserId.GetHabbo().Id != session.GetHabbo().Id)
                            Azure.GetGame()
                                .GetAchievementManager()
                                .ProgressUserAchievement(clientByUserId, "ACH_GiftReceiver", 1, false);
                    }
                    session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
                    continue;
                }
                session.GetMessageHandler()
                    .GetResponse()
                    .Init(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
                session.GetMessageHandler().GetResponse().AppendInteger(1);
                var i = 1;
                if (baseItem.Type == 's') i = InteractionTypes.AreFamiliar(GlobalInteractions.Pet, baseItem.InteractionType) ? 3 : 1;
                session.GetMessageHandler().GetResponse().AppendInteger(i);
                var list = DeliverItems(session, baseItem,
                    priceAmount * (int)item.Items[baseItem], extraData, limitedId, limtot, text);

                session.GetMessageHandler().GetResponse().AppendInteger(list.Count);
                foreach (var current3 in list) session.GetMessageHandler().GetResponse().AppendInteger(current3.Id);
                session.GetMessageHandler().SendResponse();
                session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                if (InteractionTypes.AreFamiliar(GlobalInteractions.Pet,
                    baseItem.InteractionType)) session.SendMessage(session.GetHabbo().GetInventoryComponent().SerializePetInventory());
            }
            if (item.Badge.Length >= 1) session.GetHabbo().GetBadgeComponent().GiveBadge(item.Badge, true, session, false);
        }

        /// <summary>
        /// Delivers the items.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="limno">The limno.</param>
        /// <param name="limtot">The limtot.</param>
        /// <param name="songCode">The song code.</param>
        /// <returns>List&lt;UserItem&gt;.</returns>
        internal List<UserItem> DeliverItems(GameClient session, Item item, int amount, string extraData, int limno,
            int limtot, string songCode)
        {
            var list = new List<UserItem>();
            if (item.InteractionType == Interaction.PostIt)
                amount = amount * 20;

            {
                var a = item.Type;
                if (a == 'i' || a == 's')
                {
                    var i = 0;
                    while (i < amount)
                    {
                        var interactionType = item.InteractionType;
                        switch (interactionType)
                        {
                            case Interaction.Dimmer:
                                goto IL_F87;
                            case Interaction.Trophy:
                            case Interaction.Bed:
                            case Interaction.Guillotine:
                            case Interaction.ScoreBoard:
                            case Interaction.VendingMachine:
                            case Interaction.Alert:
                            case Interaction.OneWayGate:
                            case Interaction.LoveShuffler:
                            case Interaction.HabboWheel:
                            case Interaction.Dice:
                            case Interaction.Bottle:
                            case Interaction.Hopper:
                            case Interaction.Rentals:
                            case Interaction.Pet:
                            case Interaction.Pool:
                            case Interaction.Roller:
                            case Interaction.FootballGate:
                                goto IL_10C3;
                            case Interaction.Teleport:
                            case Interaction.QuickTeleport:
                                {
                                    var userItem = session.GetHabbo()
                                        .GetInventoryComponent()
                                        .AddNewItem(0u, item.ItemId, "0", 0u, true, false, 0, 0, "");
                                    var id = userItem.Id;
                                    var userItem2 = session.GetHabbo()
                                        .GetInventoryComponent()
                                        .AddNewItem(0u, item.ItemId, "0", 0u, true, false, 0, 0, "");
                                    var id2 = userItem2.Id;
                                    list.Add(userItem);
                                    list.Add(userItem2);
                                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                                    {
                                        queryReactor.RunFastQuery(string.Format("INSERT INTO items_teleports (tele_one_id,tele_two_id) VALUES ('{0}','{1}')", id, id2));
                                        queryReactor.RunFastQuery(string.Format("INSERT INTO items_teleports (tele_one_id,tele_two_id) VALUES ('{0}','{1}')", id2, id));
                                        break;
                                    }
                                }
                            case Interaction.PetDog:
                            case Interaction.PetCat:
                            case Interaction.PetCrocodile:
                            case Interaction.PetTerrier:
                            case Interaction.PetBear:
                            case Interaction.PetPig:
                            case Interaction.PetLion:
                            case Interaction.PetRhino:
                            case Interaction.PetSpider:
                            case Interaction.PetTurtle:
                            case Interaction.PetChick:
                            case Interaction.PetFrog:
                            case Interaction.PetDragon:
                            case Interaction.PetHorse:
                            case Interaction.PetMonkey:
                            case Interaction.PetGnomo:
                            case Interaction.PetMonsterPlant:
                            case Interaction.PetWhiteRabbit:
                            case Interaction.PetEvilRabbit:
                            case Interaction.PetLoveRabbit:
                            case Interaction.PetPigeon:
                            case Interaction.PetEvilPigeon:
                            case Interaction.PetDemonMonkey:
                            case Interaction.PetFools:
                            case Interaction.Pet25:
                            case Interaction.Pet26:
                            case Interaction.Pet27:
                            case Interaction.Pet28:
                            case Interaction.Pet29:
                            case Interaction.Pet30:
                            case Interaction.Pet31:
                            case Interaction.Pet32:
                            case Interaction.Pet33:
                            case Interaction.Pet34:
                                {
                                    var petData = extraData.Split('\n');
                                    var petId = int.Parse(item.Name.Replace("a0 pet", string.Empty));
                                    var generatedPet = CreatePet(session.GetHabbo().Id, petData[0], petId, petData[1], petData[2], 0);
                                    session.GetHabbo().GetInventoryComponent().AddPet(generatedPet);
                                    list.Add(session.GetHabbo().GetInventoryComponent().AddNewItem(0, 1534, "0", 0u, true, false, 0, 0, string.Empty));
                                    break;
                                }
                            default:
                                switch (interactionType)
                                {
                                    case Interaction.MusicDisc:
                                        goto IL_1067;
                                    case Interaction.PuzzleBox:
                                        goto IL_10C3;
                                    case Interaction.RoomBg:
                                        goto IL_FF7;
                                    default:
                                        switch (interactionType)
                                        {
                                            case Interaction.GuildItem:
                                            case Interaction.GuildGate:
                                            case Interaction.GroupForumTerminal:
                                                list.Add(session.GetHabbo()
                                                    .GetInventoryComponent()
                                                    .AddNewItem(0u, item.ItemId, "0", Convert.ToUInt32(extraData),
                                                        true, false, 0, 0, string.Empty));
                                                break;

                                            case Interaction.GuildForum:
                                                uint groupId;
                                                uint.TryParse(extraData, out groupId);
                                                var group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
                                                if (group != null)
                                                {
                                                    if (group.CreatorId == session.GetHabbo().Id)
                                                    {
                                                        session.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                                                        session.GetMessageHandler()
                                                            .GetResponse()
                                                            .AppendString("forums.delivered");
                                                        session.GetMessageHandler().GetResponse().AppendInteger(2);
                                                        session.GetMessageHandler()
                                                            .GetResponse()
                                                            .AppendString("groupId");
                                                        session.GetMessageHandler()
                                                            .GetResponse()
                                                            .AppendString(extraData);
                                                        session.GetMessageHandler()
                                                            .GetResponse()
                                                            .AppendString("groupName");
                                                        session.GetMessageHandler()
                                                            .GetResponse()
                                                            .AppendString(group.Name);
                                                        session.GetMessageHandler().SendResponse();
                                                        if (!group.HasForum)
                                                        {
                                                            group.HasForum = true;
                                                            group.UpdateForum();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        session.SendNotif(Azure.GetLanguage().GetVar("user_group_owner_error"));
                                                    }
                                                }
                                                list.Add(session.GetHabbo().GetInventoryComponent().AddNewItem(0u, item.ItemId, "0", Convert.ToUInt32(extraData), true, false, 0, 0, string.Empty));
                                                break;

                                            default:
                                                goto IL_10C3;
                                        }
                                        break;
                                }
                                break;
                        }
                    IL_10EE:
                        i++;
                        continue;
                    IL_F87:
                        var userItem3 = session.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(0u, item.ItemId, extraData, 0u, true, false, 0, 0, "");
                        var id3 = userItem3.Id;
                        list.Add(userItem3);
                        using (var queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                        {
                            queryreactor2.RunFastQuery(string.Format("INSERT INTO items_moodlight (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES ({0},'0',1,'#000000,255,0','#000000,255,0','#000000,255,0')", id3));
                            goto IL_10EE;
                        }
                    IL_FF7:
                        var userItem4 = session.GetHabbo().GetInventoryComponent().AddNewItem(0u, item.ItemId, extraData, 0u, true, false, 0, 0, string.Empty);
                        var id4 = userItem4.Id;
                        list.Add(userItem4);
                        using (var queryreactor3 = Azure.GetDatabaseManager().GetQueryReactor())
                        {
                            queryreactor3.RunFastQuery(string.Format("INSERT INTO items_toners VALUES ({0},'0',0,0,0)", id4));
                            goto IL_10EE;
                        }
                    IL_1067:
                        list.Add(session.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(0u, item.ItemId, extraData, 0u, true, false, 0, 0, songCode));
                        goto IL_10EE;
                    IL_10C3:
                        list.Add(session.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(0u, item.ItemId, extraData, 0u, true, false, limno, limtot, ""));
                        goto IL_10EE;
                    }
                    return list;
                }
                if (a == 'e')
                {
                    for (var j = 0; j < amount; j++)
                        session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(item.SpriteId, 7200, 0);
                    return list;
                }
                else if (a == 'r')
                {
                    if (item.Name == "bot_bartender")
                    {
                        var bot = CreateBot(session.GetHabbo().Id, "Mahw",
                            "hr-9534-39.hd-600-1.ch-819-92.lg-3058-64.sh-3064-110.wa-2005",
                            "¡Te calma la sed y sabe bailar!", "f", true);
                        session.GetHabbo().GetInventoryComponent().AddBot(bot);
                        session.SendMessage(session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
                    }
                    else
                    {
                        var bot2 = CreateBot(session.GetHabbo().Id, "Claudio",
                            "hr-3020-34.hd-3091-2.ch-225-92.lg-3058-100.sh-3089-1338.ca-3084-78-108.wa-2005",
                            "Habla, anda, baila y se viste", "m", false);
                        session.GetHabbo().GetInventoryComponent().AddBot(bot2);
                        session.SendMessage(session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
                    }
                    return list;
                }
                return list;
            }
        }

        /// <summary>
        /// Gets the random ecotron reward.
        /// </summary>
        /// <returns>EcotronReward.</returns>
        internal EcotronReward GetRandomEcotronReward()
        {
            var level = 1u;
            if (Azure.GetRandomNumber(1, 2000) == 2000)
                level = 5u;
            else if (Azure.GetRandomNumber(1, 200) == 200)
                level = 4u;
            else if (Azure.GetRandomNumber(1, 40) == 40)
                level = 3u;
            else if (Azure.GetRandomNumber(1, 4) == 4)
                level = 2u;
            var ecotronRewardsForLevel = GetEcotronRewardsForLevel(level);
            return
                ecotronRewardsForLevel[Azure.GetRandomNumber(0, (ecotronRewardsForLevel.Count - 1))];
        }

        /// <summary>
        /// Gets the ecotron rewards.
        /// </summary>
        /// <returns>List&lt;EcotronReward&gt;.</returns>
        internal List<EcotronReward> GetEcotronRewards() { return EcotronRewards; }

        /// <summary>
        /// Gets the ecotron rewards levels.
        /// </summary>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        internal List<int> GetEcotronRewardsLevels() { return EcotronLevels; }

        /// <summary>
        /// Gets the ecotron rewards for level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>List&lt;EcotronReward&gt;.</returns>
        internal List<EcotronReward> GetEcotronRewardsForLevel(uint level)
        {
            return EcotronRewards.Where(current => current.RewardLevel == level).ToList();
        }
    }
}