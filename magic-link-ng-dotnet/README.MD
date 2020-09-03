# MagicLink-ng-dotnet
This sample show how to use MagicLink APIs in an Angular Web App through the direct call to the TBServer services.

As this approach requires to handle cookies returned by some of the APIs, it is not possible to invoke them directly from the Angular front-end, due to the CORS restrictions. A NET Core back-end is then used as a wrapper for the calls.