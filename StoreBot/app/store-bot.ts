import { Discord, Client, On } from "@typeit/discord";
import * as Path from 'path';
import { CLI, Shim } from 'clime';
import { Message, MessageEmbed, TextChannel } from 'discord.js';
import { ConfigHelper } from './Services/config-helper';
import { DiscordCli } from './Services/discord-cli';
import { DiscordShim } from './Services/discord-shim';


const ValidCommands = ['help', 'points'];
const __Prefix: string = '__';

@Discord
export abstract class StoreBot {
    private static _client: Client;
    private static _configHelper: ConfigHelper;

    static start() {
        console.log("Starting Store Bot bot with prefix '" + __Prefix + "'...");
        this._client = new Client();
        this._configHelper = new ConfigHelper();

        this.startLoginLoop ();
    }

    static startLoginLoop() {
        try {
            this._client.login( this._configHelper.DiscordToken );
        } catch {
            console.log("Failed to log in");
            this.startLoginLoop();
        }
    }

    @On("message")
    private async onMessage(message: Message) {
        console.log(message.content);
        
        if (StoreBot._client.user &&
                message.author &&
                StoreBot._client.user.id !== message.author.id) {

            if (message.content.startsWith(__Prefix) && !message.author.bot) {
                let cmd = this.sanitizeInput(message.content);
                let ioFirstSpace = cmd.indexOf(" ");
                let baseCmd = "";

                if (ioFirstSpace === -1){
                    baseCmd = cmd.toLowerCase();
                    cmd = "";   
                } else{
                    baseCmd = cmd.substr(0, ioFirstSpace).toLowerCase();
                    cmd = cmd.substr(ioFirstSpace + 1);
                }

                console.log("baseCmd='" + baseCmd + "'")
                
                try {
                    if (baseCmd === 'help') {
                        let helpText = "```\nValid commands:\n\n";

                        ValidCommands.forEach(command => {
                            helpText = helpText + ' - ' + command +  '\n';
                        });

                        helpText = helpText + '\nAll commands accept the "--help" parameter, which will cause the command to print usage info.```\n';

                        message.channel.send(helpText);
                    } else  if (ValidCommands.includes(baseCmd)) {                        
                        let cmdDir = Path.join(__dirname, 'Commands', baseCmd);
                        
                        let cli = new DiscordCli(baseCmd, cmdDir);
                        let shim = new DiscordShim(cli, StoreBot._client,message);
                        
                        let result = await shim.execute(cmd);

                        if (result.text) {
                            // Remove all control characters
                            let cleanText = result.text.replace(/[\u001b\u009b][[()#;?]*(?:[0-9]{1,4}(?:;[0-9]{0,4})*)?[0-9A-ORZcf-nqry=><]/g, "");
                            message.channel.send("```\n" + cleanText + "\n```");
                        } else {
                            if (result instanceof MessageEmbed) {
                                result.type = "rich";
                                // Footer is required because embeds on android are dumb (adding a long footer
                                // coerces a minimum width). 
                                // https://www.reddit.com/r/discordapp/comments/co8f2x/uh_any_way_to_fix_this_it_happens_using_any_form/
                                result.footer = {text:"Store Bot | A helper for the BedrockPortal web host."};
                            }

                            message.channel.send(result);
                        }
                    }
                } catch (error) {
                    console.error(error);
                }
            } else if (!message.author.bot) {
                // Message is not bot and not command
                if (StoreBot._configHelper.PointsChannels.includes ((message.channel as TextChannel).name)){
                    
                }
            }
        }
    }

    sanitizeInput(content: string):string {
        return content
                .replace(__Prefix, "")              // Remove prefix from command
                .replace(/[\u2018\u2019]/g, "'")    // Curly single quote to regular
                .replace(/[\u201C\u201D]/g, '"')    // Curly double quote to regular
                .trim()
    }
}

StoreBot.start();