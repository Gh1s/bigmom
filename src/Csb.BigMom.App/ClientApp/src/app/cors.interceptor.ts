import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, from } from "rxjs";
import { ApiOptions } from "./api-options";


@Injectable()
export class CorsInterceptor implements HttpInterceptor {

    constructor(private apiOptions: ApiOptions) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return from(this.handle(req, next));
    }

    async handle(req: HttpRequest<any>, next: HttpHandler) {
        let corsReq: HttpRequest<any>;
        if (req.url.startsWith(this.apiOptions.bigmomApiUrl) || req.url.startsWith(this.apiOptions.directoryApiUrl)) {
            corsReq = req.clone({
                headers: new HttpHeaders().set('Access-Control-Allow-Origin', '*')
            });
        } else {
            corsReq = req;
        }

        return next.handle(corsReq).toPromise();
    }

}
