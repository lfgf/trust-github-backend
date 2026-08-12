# Kojinx Trust Module: GitHub Backend

[![NuGet version](https://img.shields.io/nuget/v/Kojinx.Trust.GitHub.svg)](https://www.nuget.org/packages/Kojinx.Trust.GitHub)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

This is an official Trust Module for the **Kojinx** ecosystem. 

## 🛡️ What is a "Trust Module"?

Kojinx operates under a **surgical trust** architecture. Rather than open-sourcing the entire proprietary monolith, we exclusively extract the exact code paths (pure functions) that interact with sensitive user data, API keys, or OAuth tokens. 

These pure functions are packaged into isolated, public, and fully auditable libraries called **Trust Modules**. The proprietary core system imports these packages to perform sensitive actions. 

### Why is this open source?
We believe that developers should never have to blindly trust an application with their third-party credentials. By making this module open source, anyone can audit the exact code that negotiates, encrypts, and handles their GitHub OAuth tokens within the Kojinx platform.

You can verify that:
- We do not over-request or abuse GitHub scopes.
- We do not store your tokens in plaintext.
- The module strictly performs the OAuth exchange and immediately encrypts the token before passing it back to the Kojinx core.

## 📦 What does this module do?

This library encapsulates the backend logic for Kojinx's GitHub integration. It provides services to:
1. Exchange an OAuth callback code for an access token (`exchangeCodeForToken`).
2. Fetch the user's GitHub profile and email securely (`fetchProfile`, `fetchEmail`).
3. Fetch user repositories (`fetchRepositories`).
4. Generate and validate OAuth state parameters to prevent CSRF (`generateState`, `validateState`).
5. Securely encrypt/decrypt tokens using .NET's `IDataProtector`.

All infrastructure dependencies (like the database, HTTP client, or Data Protection keys) are injected via strictly defined interfaces. The module itself has zero side effects and contains no telemetry or logging.

## 🚀 How to use and test

If you are auditing this code or wish to run the module's tests locally:

1. Clone this repository.
2. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed.
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the test suite (if tests are included in the repository):
   ```bash
   dotnet test
   ```

Because this module uses dependency injection for all external boundaries (like `IHttpClientFactory` and `ITokenEncryptor`), you can easily write unit tests by mocking these interfaces without needing a real database or a live GitHub API.

## 🤝 Contributing

While this library is primarily extracted for auditing purposes, we welcome issues and pull requests if you spot security vulnerabilities, inefficiencies, or bugs in how we handle GitHub API interactions.

## 📄 License

This project is licensed under the MIT License.

<!-- trigger 4 -->