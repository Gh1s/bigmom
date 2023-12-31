import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));

/*fetch('me')
    .then(response => response.json())
    .then(me => {
        (window as any)['__me'] = me
        platformBrowserDynamic()
            .bootstrapModule(AppModule)
            .catch(err => console.error(err));
    });*/
