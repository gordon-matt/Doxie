[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

# Doxie - Help Generator for .NET Assemblies

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
8.1 Change the footer text
8.2 Change the URL in the GitHub Ribbon
8.3 Use a different Bootstrap theme (see: https://cdnjs.com/libraries/bootswatch)

> **NOTE** Regarding step 5: For .NET Core assemblies you need to ensure that all related assemblies are present in the same location as the ones you want to generate pages from. Otherwise the resulting documentation will contain error messages caused by **FileNotFoundException**s. It can be a pain to figure out what assemblies you need to copy, so there's a simple trick you can use to make this very easy:

1. Add a new project to your solution and call it something like **DoxieDummy** or whatever prefer.
2. Reference all the projects in the solution that you want documentation for.
3. Now the important part: Edit the **.csproj** file for this dummy project and add the following: `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>`. This will ensure ALL assemblies are copied locally to the output directory. Below is a screenshot example:

![Dummy Project](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Dummy.PNG)

4. Now all you have to do is pass that directory path to one of the "Help File Generators" and tell it which of those assemblies should be documented.

## License

This project is licensed under the [MIT license](LICENSE.txt).

## Donate
If you find this project helpful, consider buying me a cup of coffee.  :-)

#### PayPal:

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

#### Crypto:
- **Bitcoin**: 1EeDfbcqoEaz6bbcWsymwPbYv4uyEaZ3Lp
- **Ethereum**: 0x277552efd6ea9ca9052a249e781abf1719ea9414
- **Litecoin**: LRUP8hukWGXRrcPK6Tm7iUp9vPvnNNt3uz

