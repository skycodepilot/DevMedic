# DevMedic 🩺

**DevMedic** is a distributed "Heart Monitor" for developers. It tracks your cognitive state (Coding, Browsing, Idle) based on your input patterns (Keystrokes vs. Mouse usage) and logs it to a persistent database.

It is built as a **Privacy-First** application:
1.  **The Agent** runs locally on your machine (Linux/Pop!_OS) to sense input.
2.  **The API** runs in an isolated Docker container to record and analyze data.
3.  **The Data** stays on your machine in a local SQLite database.

---

## 🏗 Architecture

The system uses a **Split Architecture** to balance access with isolation:

* **DevMedic.Agent (Host OS):** A C# Console Application that runs natively. It uses `SharpHook` to listen to global low-level input events. It aggregates activity into 1-second "Pulses" and fires them to the API.
* **DevMedic.Api (Docker):** A .NET 9 Web API running in a container. It acts as the "Brain," receiving pulses, classifying the activity (e.g., "Coding" vs. "Browsing"), and saving the history to a SQLite database via Entity Framework Core.

---

## 🚀 Prerequisites

* **.NET 9 SDK** (For running the Agent)
* **Docker** (For running the API)
* **Linux Environment** (Agent currently optimized for Linux/X11/Wayland via SharpHook)

---

## 🛠️ Setup & Run

### 1. Start the API (The Brain)
The API runs in Docker. You must run this first so the Agent has someone to talk to.

```bash
# From the solution root
docker build -t devmedic-api .
docker run -p 5000:8080 devmedic-api
```

*The API is now listening on http://localhost:5000/api/pulse*

### 2. Start the Agent (The Sensor)
Open a new terminal. The Agent must run natively to access input hooks.

```bash
cd DevMedic.Agent
dotnet run
```

*You should see a visual "EKG" bar in the terminal reacting to your typing.*

---

## 🧠 Features

### Real-Time Pattern Recognition
The API analyzes the ratio of Keystrokes to Mouse Events to classify your state:
* **CODING 💻:** High keyboard activity, low mouse usage.
* **BROWSING 🌐:** High mouse usage, low keyboard activity.
* **IDLE/THINKING 🤔:** Low but present activity.
* **AWAY 💤:** Zero activity (Agent sends one "Away" signal, then stays silent to save bandwidth).

### Persistence
* Data is saved to a **SQLite** database (`devmedic.db`) located inside the API container.
* Uses **Entity Framework Core** for data access and automatic schema creation.

### Visualization
* **Agent Console:** Shows real-time ASCII bar graphs of Key/Mouse intensity.
* **API Console:** Logs received packets and classification decisions.

---

## 🧪 Testing

1.  Run both the API and Agent.
2.  **Type frantically:** Watch the Agent bar fill up with `#` and the API log "CODING".
3.  **Browse the web:** Move the mouse in circles. Watch the Agent bar fill with `.` and the API log "BROWSING".
4.  **Stop everything:** Watch the Agent show a flatline and the API log "AWAY", then go silent.

---

## 📂 Project Structure

* `DevMedic.Agent/`: The client-side sensor app.
* `DevMedic.Api/`: The server-side recording API.
* `Dockerfile`: Multi-stage build script for the API.
* `.dockerignore`: Ensures clean build contexts.

---

## 🛡️ Privacy Note
DevMedic counts **events** (e.g., "Key Pressed"). It **DOES NOT** record **which** key was pressed. Your passwords and code content never leave the `SharpHook` buffer and are never sent to the API.