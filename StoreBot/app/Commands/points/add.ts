import { Command, command, param, params, metadata, Options, option } from 'clime';
import { MessageEmbed } from 'discord.js';
import { DiscordCommandContext } from '../../Services/discord-command-context';
import { SecurityLevel } from '../../Services/security-service';
import { ConfigHelper } from '../../Services/config-helper';
import request from 'request';


export const brief = 'Add points';
export const description =
    'Use this subcommand to add points for a given user.';

export const minimumSecurityLevel = SecurityLevel.Moderator;

@command()
export default class extends Command {
    async execute(
        @param({
            type: String,
            description: 'The user\'s discord handle without #1234 part',
            required: true
        })
        discordHandle: string,
        @param({
            type: Number,
            description: 'Amount of points to add',
            required: true
        })
        amount: number,
        context: DiscordCommandContext
    ) {
        let embed = new MessageEmbed()
        embed.title = "Add points";

        if (context.message.guild) {
            
            try {
                let ch = new ConfigHelper();
                let payload = new AddPointsPayload (amount, discordHandle, 1, ch.PortalApiSecret)
                console.log("Adding points");
                embed.description = "Success!"
                // request.post(ch.PortalUri + "/Store/AddCurrencyToUser", 
                //     {  
                //         headers : {'content-type': 'application/json'},
                //         body : JSON.stringify (payload)
                //     }, (error, response, body) => 
                //         { 
                //             console.log(response); 
                //             if (response.statusCode === 200)
                //             {
                //                 embed.description = "Success!"
                //             } else 
                //             {
                //                 embed.description = "Failed!"
                //             }
                //         } );
            } catch (ex)
            {
                embed.description = "Failed!"
            }
    }

        return embed;
    }
}

export class AddPointsPayload {
    Amount : number;
    DiscordId : string;
    CurrencyTransactionReason : number;
    Secret: string;

    constructor (amount: number, discordId:string, currencyTransactionReason: number, secret: string ) {
        this.Amount = amount;
        this.DiscordId = discordId;
        this.CurrencyTransactionReason = currencyTransactionReason;
        this.Secret = secret;
    }
}