# Development notes

- Target framework: .NET 8; the installed .NET 10 SDK can build it while the app targets .NET 8.
- Local dev runs the API on port 5255 and Blazor Server on port 5216.
- The default AI provider is mock; no secrets are committed.
- Database file is generated locally and ignored by Git.
- Data is synthetic. Add reviewed knowledge content and real authentication before considering real farmer records.

## Two-minute project introduction

“KrishiMitra is a .NET 8 learning MVP for small farm record keeping and basic agricultural information. A Kannada-first Blazor Server UI calls an ASP.NET Core API. EF Core and SQLite persist a farm profile, farm activities, expenses, reminders and the demo chat. The AI flow is behind an `IAiAssistant` interface: the default provider is predictable and offline, while an OpenAI-compatible HTTP provider can be configured server-side. Before a question reaches the assistant, a small keyword service retrieves up to three educational documents. The app is deliberately a single-user demo; it does not claim to provide secure identity, authoritative agronomy advice or live data.”

## Five-minute technical walkthrough

1. Show the dashboard, then edit the farm profile. Explain that the Web app uses `FarmApiClient`; it does not query SQLite directly.
2. Add an activity with an expense and a date. Explain the API validation and SQLite persistence. Filtering and totals use the selected range; totals are summed in .NET because SQLite's EF provider maps decimals to text.
3. Create, complete and filter a reminder. It is an in-app record only; no notification service runs.
4. Ask the assistant about record keeping. Trace `POST /api/ai/chat` through `IKnowledgeRetriever`, retrieved context and `IAiAssistant`. Show that Mock is the default and that returned source names come from retrieved demo records.
5. Explain boundaries: no login, no multi-tenant access, no sensitive data, no definitive diagnosis or chemical dosage, and no live external information. Explain the next production work: identity and authorization, reviewed regional content, secure key storage, deployment/observability, and broader API integration tests.

## Likely interview questions

- Why split Core, Infrastructure, API and Web? To keep persistence/provider details out of UI and business contracts, while avoiding microservices.
- Why use an AI interface and a mock? It keeps offline development deterministic, avoids a live service in tests, and lets dependency injection choose a provider.
- How does the knowledge lookup work? Lowercase/tokenize the question, score title hits above content hits, and return up to three documents.
- Why no vector database? The corpus is tiny and keyword retrieval is easier to explain and validate; it misses synonyms and cross-language matches.
- How are secrets kept off the client? The HTTP call and `AI__ApiKey` configuration exist only in the API process.
- What are the main limits? Single-user demo, unreviewed educational content, small test suite, and no production deployment/security controls.

Beginner AI/LLM questions to practice:

- What is an LLM, and what does the system message contribute?
- What makes this a small retrieval-augmented generation flow?
- Why are retrieved documents called untrusted context?
- What is a hallucination, and how does the app limit its impact?
- Why does Mock return `IsDemo=true`?
- Why are secrets configured on the API instead of the Blazor UI?
- Why are live-model requests excluded from automated tests?

## Challenges addressed

- NuGet restore needs network access on first setup; packages are not vendored.
- SQLite decimal aggregation is not supported in the same way as a numeric SQL column, so the API sums the already date-filtered decimal rows in .NET.
- Kannada and English are both accepted by chat and retrieval has Kannada seed documents. The rest of the interface currently uses Kannada-first labels; an English UI toggle is future work.
