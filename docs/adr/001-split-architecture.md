# ADR 001: Split Architecture (Native Agent + Containerized API)

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context
The DevMedic application must intercept low-level hardware input (keystrokes and mouse movements) to determine developer activity. However, storing and processing this data needs to be isolated from the Host OS to prevent accidental data leakage and ensure a clean "burn" (deletion) process.

## Decision
We will implement a **Split Architecture**:
1.  **DevMedic.Agent (Native):** A .NET 9 console application running natively on the Host OS. This is required because hardware input hooks (via `SharpHook`) require direct access to the OS input stack (X11/Wayland/Windows), which is difficult or insecure to pass into a container.
2.  **DevMedic.Api (Docker):** A .NET 9 Web API running inside a locked-down Docker container. This acts as the "Black Hole" where data is classified and stored.

## Consequences
* **Hardware Access:** Maintains low-level hook capabilities without compromising the storage layer.
* **Environmental Requirements:** Users must have the .NET 9 SDK (for the Agent) and Docker (for the API) installed.
* **Security Isolation:** The API is network-isolated (bound to 127.0.0.1), while the Agent remains the only component interacting with the Host OS.