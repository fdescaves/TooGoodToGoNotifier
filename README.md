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

- Search for available favorite baskets happens every day every X seconds. **Interval** value must be between 1 and 59. Search is also restricted between the configured range with **StartTime** and **EndTime** parameters.

```json
  "SchedulerOptions": {
    "Interval" : 10,
    "StartTime": "07:00:00",
    "EndTime": "20:00:00"
  }
```

## Notifications rules

- Only your favorite baskets are notified
- Once a basket has been notified, it won't be notified again unless it's seen as out of stock
