import 'jquery';
import { HttpClient } from 'aurelia-http-client';
import { PLATFORM } from 'aurelia-pal';

export class App {
    async configureRouter(config, router) {
        config.title = 'Aurelia';
		config.map([
			{ moduleId: PLATFORM.moduleName('/aurelia-app/index'), route: ['', 'index'], name: 'index', nav: true, title: 'Home' },
			{ moduleId: PLATFORM.moduleName('/aurelia-app/flickr'), route: 'flickr', name: 'flickr', nav: true, title: 'Flickr' }
		]);
        this.router = router;
    }
}