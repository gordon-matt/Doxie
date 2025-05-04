## NOTICE

The Aurelia version will no longer be maintained and will be removed in future.

## BLAZOR VERSION

Doxie now has a Blazor version (Web Assembly). The demo mentioned below is now the new Blazor version instead of the Aurelia one. Instructions for the Blazor version as follows:

1. Change **img/logo.png** to your own logo (keep same file name, else modify **NavMenu.razor**)
2. Change the `href` for the **Fork me on GitHub** link in **MainLayout.razor**
3. Change the `<title>` and `<base>` elements in **wwwroot/index.html**
4. Make any other changes you wish (such as Bootstrap theme).
5. Publish to folder
6. Ignore the **web.config** and just add all files from **wwwroot** directly to your project's **gh-pages** branch
7. Make sure you have the **.gitattributes** and **.nojekyll** files present
8. Push to Git

That's it.

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

<img src="https://github.com/gordon-matt/Doxie/blob/master/_Misc/logos/Doxie.png" alt="Logo" width="250" />

# Doxie - Help Generator for .NET Assemblies

Built with:

<img src="https://github.com/gordon-matt/Doxie/blob/master/_Misc/logos/Aurelia.png" alt="Built with Aurelia" width="250" />

![Index Page](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Index.png)

![Assembly Page](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Assembly.png)

## Demo

You can find a working demo I deployed to **gh-pages** for my Extenso project, here: https://gordon-matt.github.io/Extenso/.

## Instructions

1. Install [NodeJS](https://nodejs.org/en/download/)
2. Install JSPM globally: `npm install -g jspm`
3. Clone/download this project
4. Restore JSPM packages: `jspm install`
> **NOTE:** Do this from the root directory of the "Doxie" project (not the solution root)
5. Run one of the help file generators (either WinForms or Console version)
6. Grab the generated **assemblies.json** and place it in **Doxie/js**
7. Run Doxie from a web server (example: IIS)
8. Before deploying, modify the site as you like. Some suggestions:
   1. Change the footer text
   2. Change the URL in the GitHub Ribbon
   3. Use a different Bootstrap theme (see: https://cdnjs.com/libraries/bootswatch)

> **NOTE** Regarding step 5: For .NET Core assemblies you need to ensure that all related assemblies are present in the same location as the ones you want to generate pages from. Otherwise the resulting documentation will contain error messages caused by **FileNotFoundException**s. It can be a pain to figure out what assemblies you need to copy, so there's a simple trick you can use to make this very easy:

1. Add a new project to your solution and call it something like **DoxieDummy** or whatever prefer.
2. Reference all the projects in the solution that you want documentation for.
3. Now the important part: Edit the **.csproj** file for this dummy project and add the following: `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>`. This will ensure ALL assemblies are copied locally to the output directory. Below is a screenshot example:

![Dummy Project](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Dummy.PNG)

4. Now all you have to do is pass that directory path to one of the "Help File Generators" and tell it which of those assemblies should be documented.

## Bundling

The default setup can be a bit slow. You can improve this by enabling JSPM bundling. I have added the necessary configuration in **packages.json** and **gulpfile.js**. All you need to do is run `gulp bundle`.
> **NOTE:** Gulp tends to complain about the `baseURL` in the **config.js** being set to `location.pathname`. You can temporarily (or permanently if you wish) change this to the hardcoded path you need. In many cases this can be simply `/` and for GitHub pages it tends to be `/[Your Project Name]/`. The Gulp task should run fine then.

> Further Note: You can also swap out JSPM for webpack if you prefer. See Aurelia's documentation for how to do that. And lastly, I will at some point find time to review all the Aurelia packages and see if there are some that can be removed from **packages.json** to further improve performance. No promises on when.

## License

This project is licensed under the [MIT license](LICENSE.txt).

## Credits

The code and XSD schema for reading the XML comments files comes from an old project named Jolt.NET. The original source code can be found here: https://jolt.codeplex.com and RedGate have created their own fork on GitHub here: https://github.com/red-gate/JoltNet-core, which had an important bug fix in it.

As for the UI and the overall idea, I was inspired by [AutoHelp](https://github.com/RaynaldM/autohelp) but totally reworked it to use Aurelia and I also decided it's better to generate a JSON file to read from instead of relying on MVC controller actions to acquire the data. This way, it's easy to use in GitHub pages.

## Donate
If you find this project helpful, consider buying me a cup of coffee.  :-)

#### PayPal:

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

#### Crypto:
- **Bitcoin**: 1EeDfbcqoEaz6bbcWsymwPbYv4uyEaZ3Lp
- **Ethereum**: 0x277552efd6ea9ca9052a249e781abf1719ea9414
- **Litecoin**: LRUP8hukWGXRrcPK6Tm7iUp9vPvnNNt3uz

