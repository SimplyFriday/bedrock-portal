import { Message } from 'discord.js';
import { RealmSettings } from '../Models/realm-settings';

export abstract class SecurityService {

    public static getUserSecurityLevel(message: Message, realmSettings: RealmSettings):SecurityLevel {
        let secLvl = -1;
        
        if (message.member) {
            message.member.roles.cache.forEach(role => {
                if (role.name.toLowerCase() === realmSettings.serverAdminRoleName.toLowerCase()) {
                    secLvl = SecurityLevel.Admin
                } else if (role.name.toLowerCase() === realmSettings.serverModeratorRoleName.toLowerCase()
                    && secLvl < SecurityLevel.Admin) {
                        secLvl = SecurityLevel.Moderator;
                } else if (role.name.toLowerCase() === realmSettings.serverPlayerRoleName.toLowerCase()
                    && secLvl < SecurityLevel.Player) {
                        secLvl = SecurityLevel.Player;
                }
            });
        }

        return secLvl;
    }
}

export enum SecurityLevel {
    Player = 0,
    Moderator = 1,
    Admin = 2
}