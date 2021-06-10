export class RealmSettings {
    serverIsConfigured:boolean = true;
    serverPlayerRoleName:string = "Player";
    serverModeratorRoleName:string = "Moderator";
    serverAdminRoleName:string = "Admin";

    static serverSettings:string[] = ['serverIsConfigured', 'serverModeratorRoleName', 'serverAdminRoleName'];
}