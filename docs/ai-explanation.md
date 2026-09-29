# AI integration for a .NET developer

## What happens in this app

An LLM is a text model that predicts a continuation from a prompt. The app builds a short request with a system role (its safety and behavior rules), farm crop/location context, any keyword-matched knowledge snippets, and the farmer's question. The API sends that request from the server. The browser never sees the provider key.

The application first searches a small SQLite knowledge collection by normalized words. It scores a title hit more strongly than a content hit and returns at most three documents. This is a simple retrieval-augmented generation (RAG) flow: retrieve context, then ask a language model to use it. Keyword matching is easy to inspect and test, and is sufficient for this small educational corpus; it will miss synonyms and many Kannada-to-English matches.

## Mock and configured providers

`MockAiAssistant` returns a deterministic response containing retrieved demo content (or a no-match explanation) and marks the answer as demo. This makes the app useful offline and prevents the UI from implying a live model is active.

`OpenAiCompatibleAssistant` uses `HttpClient` from the factory and the configured `AI:BaseUrl`, `AI:Model`, and `AI:ApiKey`. Its system prompt says to use simple language, state uncertainty, avoid unsupported citations and live-data claims, not provide unverified chemical quantities, and refer serious crop problems to qualified specialists. User and retrieved text cannot override that role. Retrieved documents are explicitly untrusted.

## Limits

LLMs can hallucinate: produce plausible but incorrect claims. Retrieval can also be incomplete or outdated. Current knowledge documents are generic educational examples, not authoritative agronomy references, and no live weather, market or government source is connected. The AI does not diagnose with certainty, prescribe input quantities, or change farm records. Even with a model configured, a farmer should consult a qualified local agricultural officer for serious crop issues.
