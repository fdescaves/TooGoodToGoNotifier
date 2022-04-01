# TooGoodToGoNotifier

TooGoodToGoNotifier is a .NET 6 console application that send email notifications when any of your favorite baskets are available in the TooGoodToGo mobile application.

## How to use it

Follow the configuration instructions explained in the section below. Then, start the TooGoodToGoNotifier application. To function, the application must authenticate to the TooGoodToGo services using your credentials. Therefore, you will receive an email from TooGoodToGo in your inbox and you must click the link to complete the authentication. Please note that TooGoodToGoNotifier must be used with the same account that you use to mark your favorite baskets.
When any of your favorite baskets are available, an email will be sent to any recipients you may have configured in the `Recipients` array of the `NotifierOptions` section.

## Configuration

Open the [appsettings.json](src/appsettings.json) configuration file:

- Add your TooGoodToGo account's email

```json
  "TooGoodToGoApiOptions": {
      "AccountEmail": "[TOOGOODTOGO-ACCOUNT-EMAIL]"
    }
```

- Add the mail recipients who will receive the notifications

```json
  "NotifierOptions": {
    "Recipients": ["FOO@BAR.COM", "BAR@FOO.COM"]
  }
```

- Configure a mail server

```json
  "EmailNotifierOptions": {
    "SmtpServer": "[SMTP-SERVER]",
    "SmtpPort": 465,
    "useSsl": true,
    "SmtpUserName": "[SMTP-USERNAME]",
    "SmtpPassword": "[SMTP-PASSWORD]"
  }
```

- Search for available favorite baskets happens every day every X seconds. **Interval** value must be between 1 and 59. Search is also restricted between the configured range with **StartTime** and **EndTime** parameters. Keep in mind that TooGoodToGo services may throttle you if too many requests are sent.

```json
  "NotifierOptions": {
    "Interval" : 10,
    "StartTime": "07:00:00",
    "EndTime": "20:00:00"
  }
```

## Notifications rules

- Only your favorite baskets will be notified
- Once a basket has been notified, it won't be notified again unless it's seen as out of stock
