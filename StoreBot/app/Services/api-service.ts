import axios from 'axios';
import { resolveProjectReferencePath } from 'typescript';
import { ConfigHelper } from './config-helper';
import * as crypto from "crypto";

export class ApiService {
    public AddPoints (discordId:string, points:number) {
        try {
            let cf = new ConfigHelper();
            axios.post (cf.PortalUri + "/Api/AddPoints", {secret:cf.PortalApiSecret, points:points, discordId:discordId} ).then();
        } catch (e) {
            // todo log or something
        }
    }

    public LinkAccount (discordId:string):boolean {
        try {
            let cf = new ConfigHelper();

            let linkPlain =  discordId + cf.PortalApiSecret; 
            
            let hash = crypto.createHash('sha512');
            hash.update(linkPlain);

            let linkToken = hash.digest('base64');

            axios.get (cf.PortalUri + "/Api/LinkAccount?linkToken=" + linkToken + "&discordId=" + discordId ).then( response => {
                if (response.status === 200) {
                    return true;
                } else {
                    return false;
                }
            });
        } catch (e) {
            // todo log or something    
        }

        return false;
    }
}