# ADR 002: Privacy-First Data Anonymization

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context
Storing raw identifiers like "Jasons-Laptop" in a database creates a privacy risk. If the database file is ever moved or inspected, it could be used to link activity directly to a specific individual. We need a way to distinguish between different devices without storing Personally Identifiable Information (PII).

## Decision
The Agent will implement **Source Anonymization** before data transmission:
1.  **Hashed Identifiers:** The machine name is captured and processed through a one-way cryptographic hash (**SHA256**).
2.  **Truncation:** Only the first 8 characters of the hex string are sent to the API (e.g., `Device-4A7F92B1`).
3.  **Event Aggregation:** The Agent only sends counts of events (integers), never the specific keys pressed or coordinates of mouse movements.

## Consequences
* **Anonymity by Design:** The database is inherently anonymized. Even if the DB is compromised, reversing the hash to find the original computer name is computationally difficult.
* **Device Persistence:** If a user renames their computer, the system will treat it as a new device.
* **Limited Scope:** This ensures the data is useful for productivity trends but useless for surveillance.