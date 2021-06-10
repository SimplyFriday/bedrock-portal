import {CLI, ContextOptions, ExpectedError} from 'clime';
import { Client } from '@typeit/discord';
import { Message, MessageEmbed } from 'discord.js';
import { DiscordCommandContext } from './discord-command-context';
import { RealmSettings } from '../Models/realm-settings';
import { DiscordCli } from './discord-cli';

export class DiscordShim {
    constructor(public cli: DiscordCli, public client: Client, public message: Message) {}

    async execute(args:string): Promise<any> {
        try {
            let argArr = args.match(/[a-zA-Z0-9_-]+|"(?:\\"|[^"])+"/g);
            console.log (argArr);
            
            if (!argArr){
                argArr = [];
            }

            for (let i = 0; i < argArr.length; i++) {
                if (argArr[i].startsWith('"') && argArr[i].endsWith('"')) {
                    argArr[i] = argArr[i].substr(1, argArr[i].length - 2);
                }                
            }

            let options:ContextOptions = {
                commands: argArr,
                cwd: ""
            };

            let settings: RealmSettings;

            if (this.message.guild)
            {
                settings = new RealmSettings();
            } else {
                throw new Error("Huh, guild.id was missing. This shouldn't be possible, yet here we are.");
            }

            let context = new DiscordCommandContext(options, {message:this.message, client:this.client, realmSettings: settings} );

            return await this.cli.executeWithSecurity(argArr, context);
        } catch (error) {
            console.error(error);

            if(error.message){
                return error.message;
            }
        }

        let error = new MessageEmbed ()
        error.description = "An error occurred"
        error.title = "ERROR"

        return error;
    } 
}
