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

## Benchmark

Use BenchmarkDotNet for stable throughput and allocation measurements.

Run:

```bash
dotnet run -c Release --project benchmark/Sang.UAParser.Benchmarks.csproj
```

The benchmark project includes these scenarios:

- ParseChrome: desktop browser UA parsing
- ParseOpenHarmony: OpenHarmony mobile UA parsing
- ParseSpider: bot UA parsing
- ParseWholeDataset: full pass over tool/data/uatest.txt

Focus on these numbers:

- Mean: average execution time
- Allocated: memory allocated per operation
- ParseWholeDataset: best indicator for real throughput after parser changes

Example results on Windows 11, .NET 10.0.2, Intel Core i5-13600K:

| Method | Mean | Allocated |
| --- | ---: | ---: |
| ParseChrome | 4.096 us | 1504 B |
| ParseOpenHarmony | 4.902 us | 2224 B |
| ParseSpider | 2.804 us | 752 B |
| ParseWholeDataset | 8.137 ms | 2965332 B |

Notes:

- ParseWholeDataset uses the sample set in tool/data/uatest.txt
- Benchmark results depend on CPU, .NET runtime version, and power plan