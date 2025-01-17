# FG.MiddlewareCollection

[![NuGet Version](https://img.shields.io/nuget/v/FG.MiddlewareCollection.svg)](https://www.nuget.org/packages/FG.MiddlewareCollection/)

## Overview

FG.MiddlewareCollection is a .NET library providing a robust collection of middleware for enhancing your ASP.NET Core application's request pipeline. It simplifies the integration of common functionalities like logging, error handling, rate limiting, and more.

## Features

- Advanced request and error logging
- Configurable CORS and rate limiting
- Circuit breaker for fault tolerance
- IP whitelisting and blacklisting
- Middleware for response compression, XSS protection, and health checks

## Installation

Install the package via NuGet:

```powershell
Install-Package FG.MiddlewareCollection
```

## Usage

Here are some examples demonstrating how to use the middleware in your ASP.NET Core application:

### Advanced Logging Middleware

```csharp
app.UseAdvancedLogging();
```

### Error Logging Middleware

```csharp
app.UseErrorLogging();
```

### Health Check Middleware

```csharp
app.UseHealthCheck();
```

### Response Compression Middleware

```csharp
app.UseResponseCompression();
```

### IP Blacklisting Middleware

```csharp
var blacklistedIPs = new[] { "192.168.1.1", "10.0.0.1" };
app.UseIPBlacklisting(services, blacklistedIPs);
```

### IP Whitelisting Middleware

```csharp
var whitelistedIPs = new[] { "192.168.1.2", "10.0.0.2" };
app.UseIPWhitelisting(services, whitelistedIPs);
```

### Circuit Breaker Middleware

#### Using Options

```csharp
var circuitBreakerOptions = new CircuitBreakerMiddlewareOptions
{
    Threshold = 5,
    OpenTime = TimeSpan.FromSeconds(30)
};
app.UseCircuitBreaker(services, circuitBreakerOptions);
```

#### Using Configuration

```csharp
app.UseCircuitBreaker(services, configuration);
```

### Rate Limiting Middleware

#### Simple Configuration

```csharp
app.UseRateLimiting(services, rateLimitPerMinute: 100, limitExceedErrorMessage: "Too many requests. Try again later.");
```

#### Using Configuration

```csharp
app.UseRateLimiting(services, configuration);
```

#### Using Options

```csharp
var rateLimitingOptions = new RateLimitingMiddlewareOptions
{
    RequestsPerMinute = 200,
    LimitExceedErrorMessage = "You have exceeded the rate limit."
};
app.UseRateLimiting(services, rateLimitingOptions);
```

### Cross-Origin Resource Sharing (CORS) Middleware

#### With Custom Options

```csharp
var corsOptions = new CorsMiddlewareOptions
{
    AllowOrigin = "*" ,
    AllowMethods = "GET, POST",
    AllowHeaders = "Content-Type, Authorization"
};
app.UseCors(services, corsOptions);
```

#### Using Configuration

```csharp
app.UseCors(services, configuration);
```

### XSS Protection Middleware

```csharp
app.UseXssProtection();
```

## Contributing

We welcome contributions! Please check the [contributing guidelines](https://docs.github.com/en/contributing) for more details.

## License

This project is licensed under the MIT License. See the [LICENSE](https://opensource.org/license/mit) file for details.