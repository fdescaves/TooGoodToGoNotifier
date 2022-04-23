# TooGoodToGoNotifier

TooGoodToGoNotifier is a .NET 6 console application that send email notifications when any of your favourite baskets are available in the TooGoodToGo mobile app. The idea was to overcome the lack of notification functionality when a basket is available. This application is designed to be used on a device that is constantly running in order to avoid the tedious authentication procedure that requires human interaction.

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
  "NotifierOptions": {
    "Recipients": ["recipientEmail", "anotherRecipientEmail"]
  }
```

- Configure the email server used to send notifications

```json
  "EmailNotifierOptions": {
    "SmtpServer": "smtpServer",
    "SmtpPort": 465,
    "useSsl": true,
    "SmtpUserName": "smtpUserName",
    "SmtpPassword": "smtpPassword"
  }
```

- Scanning for favorite baskets happens every day every **20 seconds** by default. This value is configured using the **ScanningInterval** option and it's value must be between 1 and 59 seconds. Scanning is also restricted between the configured range with **StartTime** and **EndTime** options to avoid scanning when businesses are closed. Keep in mind that TooGoodToGo's services may flag you as a bot and serve a reCAPTCHA if too many requests are sent during a short period of time.

```json
  "NotifierOptions": {
    "ScanningInterval" : 20,
    "StartTime": "08:00:00",
    "EndTime": "20:00:00",
  }
```

## How it works

At startup TooGoodToGoNotifier must authenticate to the TooGoodToGo services using your credentials. Since TooGoodToGo use a passwordless authentication, you will receive an email from them in your inbox and you must navigate to the given link in a browser. **DO NOT** click the link directly on your phone with the TooGoodToGo app installed otherwise the authentication will fail.
Once the application is authenticated, it will check for available favourite baskets every 20 seconds by default. If a favourite basket is seen as available, an email notification will be sent to every recipient in the `Recipients` array of the `NotifierOptions` section. Once a basket has been notified, it won't be notified again unless it's seen as out of stock.
