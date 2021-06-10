import AppConfig from '../config.json'

export class ConfigHelper
{
    public DiscordToken:string;
    public PortalApiSecret:string;
    public PortalUri:string;
    public PointsChannels:string[];

    constructor ()
    {
        this.DiscordToken = (<any> AppConfig).DiscordToken;
        this.PortalApiSecret = (<any> AppConfig).DiscordToken;
        this.PortalUri = (<any> AppConfig).DiscordToken;
        this.PointsChannels = (<any> AppConfig).PointsChannels;
    }
}