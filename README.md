# TooGoodToGoNotifier

TooGoodToGoNotifier is a .NET Core console app that send notifications when any of your favorite baskets are available in the TooGoodToGo mobile app.

## How to use

Start TooGoodToGoNotifier. In the TooGoodToGo mobile app, using the same account as configured in the [appsettings.json](src/appsettings.json) file, mark any baskets as favorite. Notifications will be sent when any of these baskets are available.

## Configuration

Open the [appsettings.json](src/appsettings.json) configuration file:

- Add your TooGoodToGo credentials

```json
  "ApiOptions": {
    "AuthenticationOptions": {
      "Email": "[TOOGOODTOGO-EMAIL-ACCOUNT]",
      "Password": "[TOOGOODTOGO-PASSWORD-ACCOUNT]"
    }
```

- Configure a mail server and the notifications recipients

```json
  "EmailNotifierOptions": {
    "SmtpServer": "[SMTP-SERVER]",
    "SmtpPort": 465,
    "useSsl": true,
    "SmtpUserName": "[SMTP-USERNAME]",
    "SmtpPassword": "[SMTP-PASSWORD]",
    "Recipients": ["FOO@BAR.COM", "BAR@FOO.COM"]
  }
```

- By default, the app looks for available favorite baskets every day every minute of every hour between 7AM to 7PM. This behavior is configured using a cron expression and can be modified

```json
  "SchedulerOptions": {
    "CronExpression": "*/1 7-19 * * *"
  }
```

## Notifications rules

- Only your favorite baskets are notified
- Once a basket has been notified, it won't be notified again unless it's seen as out of stock
