import { Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class ApiOptions {

    private _bigmomApiUrl: string;
    public get bigmomApiUrl() {
        return this._bigmomApiUrl;
    }

    private _directoryApiUrl: string;
    public get directoryApiUrl() {
        return this._directoryApiUrl;
    }

    constructor() {
        const apiOptions = (window as any)['__me']['api'];
        this._bigmomApiUrl = apiOptions.bigmomApiUrl;
        this._directoryApiUrl = apiOptions.directoryApiUrl;
    }

}