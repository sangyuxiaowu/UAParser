# Sang.UAParser

A simple user agent parser for .NET.

## Installation

```bash
dotnet add package Sang.UAParser
```

## Usage

```csharp
using Sang.UAParser;

var uaParser = new UAParser();
var ua = uaParser.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

Console.WriteLine(ua.Browser); // Chrome
Console.WriteLine(ua.BrowserVersion); // 58.0.3029.110
Console.WriteLine(ua.OS); // Windows
Console.WriteLine(ua.OSVersion); // 10
Console.WriteLine(ua.DeviceType); // Desktop

```

## Parsing Types

Sang.UAParser supports parsing the following types of information from a user agent string:

- Browser: The web browser that the user agent is using, such as Chrome, Firefox, Safari, etc.
- Browser Version: The version of the web browser.
- OS: The operating system that the user agent is running on, such as Windows, macOS, Linux, etc.
- OS Version: The version of the operating system.
- Device Type: The type of device that the user agent is running on. This can be one of the following: Desktop, Mobile, Spider, Bot, Other.