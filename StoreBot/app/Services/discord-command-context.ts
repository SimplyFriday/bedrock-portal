import { Context, ContextOptions } from 'clime';
import { Message } from 'discord.js';
import { Client } from '@typeit/discord';
import { RealmSettings } from '../Models/realm-settings';

export class DiscordCommandContext extends Context {
    public message:Message;
    public client:Client;
    public realmSettings: RealmSettings;

    constructor (options:ContextOptions, contextExtension:DiscordCommandContextValues) {
        super(options);

        this.message = contextExtension.message;
        this.client = contextExtension.client;
        this.realmSettings = contextExtension.realmSettings
    }
}

export interface DiscordCommandContextValues {
    message:Message;
    client:Client;
    realmSettings: RealmSettings;
}