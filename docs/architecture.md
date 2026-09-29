# Architecture

## Request flow

The interactive Blazor Server UI calls `FarmApiClient`, which sends JSON to the separate ASP.NET Core API. Controllers validate input and coordinate a scoped `FarmDbContext`. EF Core translates queries to SQLite. The API returns response objects and status codes; Blazor renders them with normal encoded Razor output.

## Responsibilities

- **Core** contains the farm, activity, reminder, knowledge and chat records; request records; and the `IAiAssistant` and `IKnowledgeRetriever` contracts.
- **Infrastructure** contains EF Core configuration and migration, keyword retrieval, the mock assistant and the optional HTTP assistant.
- **API** configures dependency injection, migrates the SQLite database on startup and exposes controllers plus Swagger in Development.
- **Web** owns navigation, forms, loading and error states, and the HTTP client. It never receives provider credentials.

## Persistence and migrations

SQLite uses a single synthetic farm plus sample activity and reminder records for a fresh database. Knowledge documents are clearly labeled generic educational demo content. Startup calls `MigrateAsync`; existing databases are retained. Foreign keys cascade from farms to activities and reminders. System timestamps use a UTC value conversion.

## Chat flow

`POST /api/ai/chat` validates the question, loads the demo farm, retrieves up to three keyword matches, stores the user message, calls the configured assistant, stores the answer and returns only sources that were actually retrieved. If no document matches, the assistant is told there is no supporting context. Retrieved content is explicitly treated as untrusted. The assistant cannot change farm records.

The mock provider is selected unless `AI:Provider=OpenAICompatible`. The optional provider uses `IHttpClientFactory`, a 25-second timeout and bounded prompt/answer sizes. API keys and prompt contents are not logged. No retry loop is used.

## Error handling and validation

Controllers return 400 for invalid requests, 404 for absent IDs, 201 for creation and 204 for deletion. Unexpected errors are returned as a generic JSON error. Forms show loading and friendly failure states. Never treat this demo profile as authenticated or private.

## Growth path

Add integration tests using an isolated SQLite database; add a reviewed migration snapshot and migration workflow; introduce authentication and authorization before storing real records; then consider richer retrieval only when a curated corpus justifies it.
