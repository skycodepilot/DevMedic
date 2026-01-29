# ADR 003: Ephemeral Data Persistence (In-Container SQLite)

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context
Standard Docker practices suggest using Volumes to persist data on the host machine. However, DevMedic's "Black Hole" philosophy requires that data can be easily and permanently "burned" (deleted) to ensure long-term privacy.

## Decision
We will use **SQLite** as the database engine, and the `.db` file will reside **strictly inside the container's internal filesystem** without a host-mounted volume.
1.  **Auto-Migration:** The database is initialized via EF Core `EnsureCreated()` on startup.
2.  **State Management:** Data persists through container restarts (`docker stop/start`).
3.  **The Burn:** Data is permanently wiped when the container is removed (`docker compose down`).

## Consequences
* **Plausible Deniability:** Removing the container leaves no trace of the activity logs on the host hard drive.
* **Data Fragility:** Use of `docker compose down` results in total data loss. 
* **Manual Portability:** Users wishing to keep data must manually extract the file using `docker cp`.