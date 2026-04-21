import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Barcha /api/* so'rovlarga withCredentials: true qo'shadi.
 * Bu browser ga HttpOnly cookie ni avtomatik jo'natishni buyuradi.
 * Token localStorage/memory da saqlanmaydi — faqat HttpOnly cookie da.
 */
@Injectable()
export class CredentialsInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req.clone({ withCredentials: true }));
  }
}
