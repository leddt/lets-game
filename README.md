# Let's Game!
An app to help you plan gaming sessions with your group of friends.

Live version at http://gaming.leddt.com

## Stack
- .NET 5
- ASP.NET Core Razor Pages
- Entity Framework Core
- PostgreSQL
- Bootstrap 4

## Developer setup

- [Install .Net 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- Clone the repo and cd into it
- Run `dotnet restore`
- Run `dotnet build`
- Run `dotnet run -p LetsGame.Web`
- Access the app at `https://localhost:5001`
- Press `Ctrl-C` in your terminal to stop the app

### Database

In development, the app uses [mysticmind/mysticmind-postgresembed](https://github.com/mysticmind/mysticmind-postgresembed) to run a PostgreSQL server.

The binaries are downloaded and cached on first run. The server starts and stops with the app, assuming a graceful shutdown. 

### Third party services and user secrets

The app uses several third party services, for which the credentials are not included in the repo. The app will try to degrade gracefully when configuration settings are missing.

#### Google Authentication

We allow users to login with their Google Account. For this to work you need to register an application with Google and run these commands:

```
dotnet user-secrets set "Authentication:Google:ClientId" "Your Google client id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "Your Google client secret"
```

If those secrets are not set, the Google login feature will be disabled. You can still create an account with an email and password.

#### SendGrid

We use SendGrid to send emails. You need a SendGrid account and an API key authorized to send emails. Note that you can configure the e-mail address from which emails are sent in the `appsettings.json` file.

```
dotnet user-secrets set "SendGrid:ApiKey" "Your SendGrid api key"
```

If it is not set, the email feature will be disabled. Note that this affects user registration, because we require emails to be validated.
When the email feature is disabled, the confirmation link will be displayed in the page, allowing you to click on it to fake-confirm your email address.

#### IGDB

We use IGDB to power the games search feature. You need to have a Twitch account and to register an application on https://dev.twitch.tv.

```
dotnet user-secrets set "igdb:ClientId" "Your Twitch client id"
dotnet user-secrets set "igdb:ClientSecret" "Your Twitch client secret"
```

If this is not configured, the games search feature will always return a single fake result. 

#### IsThereAnyDeal

We are planning to use IsThereAnyDeal in the future but the features are not currently in use. You can still setup an app with them and set your secrets:

```
dotnet user-secrets set "itad:ClientId" "Your ITAD client id"
dotnet user-secrets set "itad:ClientSecret" "Your ITAD client secret"
dotnet user-secrets set "itad:ApiKey" "Your ITAD api key"
```