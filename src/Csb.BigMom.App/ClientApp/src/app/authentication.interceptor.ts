import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, from } from "rxjs";
import { ApiOptions } from "./api-options";
import { TokenProvider } from "./token-provider";


@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

    constructor(private tokenProvider: TokenProvider, private apiOptions: ApiOptions) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return from(this.handle(req, next));
    }

    async handle(req: HttpRequest<any>, next: HttpHandler) {
        let authReq: HttpRequest<any>;
        if (req.url.startsWith(this.apiOptions.bigmomApiUrl) || req.url.startsWith(this.apiOptions.directoryApiUrl)) {
            const accessToken = await this.tokenProvider.getAccessToken();
            authReq = req.clone({
                headers: new HttpHeaders().set('Authorization', `Bearer ${accessToken}`)
            });
        } else {
            authReq = req;
        }

        return next.handle(authReq).toPromise();
    }

}
