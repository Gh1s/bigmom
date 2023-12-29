import { Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class TokenProvider {

    async getAccessToken(): Promise<string> {
        const w = (window as any);
        const me = w['__me'];
        const expiresAt = new Date(me.expires_at);
        if (expiresAt < new Date()) {
            const response = await fetch('/me');
            const newMe = await response.json();
            w['__me'] = newMe;
            return newMe.access_token;
        }
        return me.access_token;
    }

    async getCsrfToken(): Promise<string> {
        const w = (window as any);
        const me = w['__me'];
        return me.csrf_token;
    }

}