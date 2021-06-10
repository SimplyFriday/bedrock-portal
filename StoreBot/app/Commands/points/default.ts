import { Command, command, param, metadata } from 'clime';
import { DiscordCommandContext } from '../../Services/discord-command-context';
import { SecurityLevel } from '../../Services/security-service';
import { MessageEmbed } from 'discord.js';

export const brief = 'Default points method';
export const description =
    'Doesn\'t actually do anything yet, but we don\'t want the default command to change data';

export const minimumSecurityLevel = SecurityLevel.Moderator;

@command()
export default class extends Command {
    @metadata
    async execute(
        context: DiscordCommandContext
    ) {
        let embed = new MessageEmbed()
        embed.title = "NTSH";
        embed.description = "Use 'points add --help' to get more info.";
        return embed;
    }
}