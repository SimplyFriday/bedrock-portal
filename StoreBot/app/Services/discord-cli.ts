import { CLI, Command, ExpectedError } from 'clime';
import { DiscordCommandContext } from './discord-command-context';
import { SecurityService, SecurityLevel } from './security-service';

// This class really only exists to inject the security stuff
export class DiscordCli extends CLI {
    public async executeWithSecurity (argv: string[], contextExtension:DiscordCommandContext, cwd?: string | undefined): Promise<any> {
        if (contextExtension instanceof DiscordCommandContext) {
            let shouldRun:boolean = false;
            
            if (!contextExtension.realmSettings.serverIsConfigured) {
                shouldRun = true;
            }

            let {
                module
            } = await (<any>this).preProcessArguments(argv);
            
            if (module && !shouldRun) {
                let targetCommand = module.default;
                let secLvl:SecurityLevel = SecurityLevel.Player;

                if (targetCommand && targetCommand.prototype instanceof Command) {
                    if (contextExtension.message.member) {

                        secLvl = SecurityService.getUserSecurityLevel(contextExtension.message, contextExtension.realmSettings);

                        let reqLvl = module.minimumSecurityLevel | 0;

                        if ( reqLvl <= secLvl) {
                            shouldRun = true;
                        }
                    }
                }
            }

            if (shouldRun) {
                return await super.execute(argv,contextExtension, cwd);
            } else {
                throw new ExpectedError ("You do not have permission to run this command");
            }

        } else {
            throw new Error ("executeWithSecurity must be called using a DiscordCommandContext object");
        }
    }
}

