# TooGoodToGoNotifier

TooGoodToGoNotifier is a .NET 6 console application that send email notifications when any of your favorite baskets are available in the TooGoodToGo mobile app. The idea behind this was to overcome the lack of notification functionality when a basket is available. It's designed to be used on a device that is constantly running in order to avoid the tedious authentication procedure that requires human interaction (for now).

## Configuration

Open the [appsettings.json](src/appsettings.json) configuration file:

- Add your TooGoodToGo account's email

```json
  "TooGoodToGoApiOptions": {
      "AccountEmail": "TooGoodToGoEmailAccount"
    }
```

- Add the email recipients

```json
  "TooGoodToGoNotifierOptions": {
    "Recipients": ["recipientEmail", "anotherRecipientEmail"]
  }
```

- Configure a mail server

```json
  "EmailNotifierOptions": {
    "SmtpServer": "smtpServer",
    "SmtpPort": 465,
    "useSsl": true,
    "SmtpUserName": "smtpUserName",
    "SmtpPassword": "smtpPassword"
  }
```

- Search for available favorite baskets happens every day every X seconds. **Interval** value must be between 1 and 59. Search is also restricted between the configured range with **StartTime** and **EndTime** parameters to avoid scanning when businesses are closed. Keep in mind that TooGoodToGo's services may flag you as a bot and serve a reCAPTCHA if too many requests are sent during a short amount of time.

```json
  "NotifierOptions": {
    "Interval" : 20,
    "StartTime": "07:00:00",
    "EndTime": "20:00:00"
  }
```

## How it works

At startup TooGoodToGoNotifier must authenticate to the TooGoodToGo services using your credentials. Since TooGoodToGo use a passwordless authentication, you will receive an email from them in your inbox and you must navigate to the given link in a navigator. **DO NOT** click the link directly on your phone with the TooGoodToGo app installed otherwise the authentication will fail. When any of your favorite baskets are seen as available, an email will be sent to any recipients you may have configured in the `Recipients` array of the `TooGoodToGoNotifierOptions` section.

## Notifications rules

- Only your favorite baskets will be notified
- Once a basket has been notified, it won't be notified again unless it's seen as out of stock
