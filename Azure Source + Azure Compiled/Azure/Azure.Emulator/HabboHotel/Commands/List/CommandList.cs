﻿using System.Linq;
using Azure.Configuration;
using Azure.HabboHotel.GameClients;
using Azure.Messages;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class CommandList. This class cannot be inherited.
    /// </summary>
    internal sealed class CommandList : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandList"/> class.
        /// </summary>
        public CommandList()
        {
            MinRank = 1;
            Description = "Mostrar todos os comandos.";
            Usage = ":commands";
            MinParams = -2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            if (ExtraSettings.NewPageCommands)
            {
                session.SendMessage(StaticMessage.NewWayToOpenCommandsList);
                return true;
            }

            string commandList;
            if (pms.Length == 0)
            {
                commandList =
                    CommandsManager.CommandsDictionary.Where(
                        command => CommandsManager.CanUse(command.Value.MinRank, session))
                        .Aggregate(string.Empty,
                            (current, command) =>
                                current + (command.Value.Usage + " - " + command.Value.Description + "\n"));
            }
            else
            {
                if (pms[0].Length == 1)
                {
                    commandList =
                        CommandsManager.CommandsDictionary.Where(
                            command =>
                                command.Key.StartsWith(pms[0]) && CommandsManager.CanUse(command.Value.MinRank, session))
                            .Aggregate(string.Empty,
                                (current, command) =>
                                    current + (command.Value.Usage + " - " + command.Value.Description + "\n"));
                }
                else
                {
                    commandList =
                        CommandsManager.CommandsDictionary.Where(
                            command =>
                                command.Key.Contains(pms[0]) && CommandsManager.CanUse(command.Value.MinRank, session))
                            .Aggregate(string.Empty,
                                (current, command) =>
                                    current + (command.Value.Usage + " - " + command.Value.Description + "\n"));
                }
            }
            session.SendNotifWithScroll(commandList);

            return true;
        }
    }
}