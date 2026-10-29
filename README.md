[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

<img src="https://github.com/gordon-matt/Doxie/blob/master/_Misc/logos/Doxie.png" alt="Logo" width="250" />

# Doxie - Help Generator for .NET Assemblies

![Index Page](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Index.png)

![Assembly Page](https://github.com/gordon-matt/Doxie/blob/master/_Misc/Assembly.png)

## Demo

You can find a working demo I deployed to **gh-pages** for my Extenso project, here: https://gordon-matt.github.io/Extenso/.

## Instructions

1. Change **img/logo.png** to your own logo (keep same file name, else modify **NavMenu.razor**)
2. Change the `href` for the **GitHub** link in **NavMenu.razor**
3. Change the `<title>` element in **wwwroot/index.html**. The base path stays `/` locally and becomes `/{repository}/` on GitHub Pages.
4. Make any other changes you wish (such as Bootstrap theme).
5. Run one of the help file generators (either WinForms or Console version)
6. Grab the generated **assemblies.json** and place it in **Doxie.BlazorClient/wwwroot/js**
7. Publish to folder
8. Ignore the **web.config** and just add all files from **wwwroot** directly to your project's **gh-pages** branch
9. Make sure you have the **.gitattributes** and **.nojekyll** files present
10. Push to Git

> **NOTE** Regarding step 5: The WinForms generator can open a `.sln` or `.slnx` and select the class libraries for you. If a project has not been built, or has no XML documentation file, it offers to build the solution in Release with documentation turned on. You can still point it at a folder of assemblies instead.
>
> Related assemblies have to be available while the pages are generated, or the result contains **FileNotFoundException** messages. A solution scan uses each project's output directory and its NuGet packages. If you use the console generator, or you would rather gather everything into one folder, add a small project that references the libraries you want to document and set `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` in that project. Build it, then point the generator at that output folder and choose which assemblies to include.

## License

This project is licensed under the [MIT license](LICENSE.txt).

## Credits

The code and XSD schema for reading the XML comments files comes from an old project named Jolt.NET. The original source code can be found here: https://jolt.codeplex.com and RedGate have created their own fork on GitHub here: https://github.com/red-gate/JoltNet-core, which had an important bug fix in it.

As for the UI and the overall idea, I was inspired by [AutoHelp](https://github.com/RaynaldM/autohelp). I also decided it's better to generate a JSON file to read from instead of relying on MVC controller actions to acquire the data. This way, it's easy to use in GitHub pages.

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />
