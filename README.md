# LocalMetrics

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?)
[![Build Status](https://github.com/louresb/LocalMetrics/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/louresb/LocalMetrics/actions/workflows/build-and-test.yml)
[![License](https://img.shields.io/badge/license-MIT-F4A261)](https://github.com/louresb/LocalMetrics/blob/main/LICENSE)

**LocalMetrics** is a lightweight, cross-platform monitoring tool that captures real-time system metrics (CPU, memory, disk) from the machine where it runs and displays them in a Blazor-based dashboard.

![video-output-173CA63B-D9B0-4E59-BAFA-C4D25DF924A8-3-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/afc6b02e-7949-4628-a056-78951547da3d)

- Real-time local system metrics  
- AES-encrypted API responses  
- Prometheus-compatible `/metrics` export  
- Runs on Windows, macOS, and Linux  
- UI served via Docker or `dotnet run`  
- No agents, no external setup — just run and monitor  

![Demo GIF of LocalMetrics](./media/demo.gif)

---
## 🚀 Quick Start

<div align="center">
  <table>
    <tr>
      <td width="40%" valign="top">
        <p align="left">
          To get started, first download the latest release and extract the ZIP file.
        </p>
      </td>
      <td width="60%" align="center">
        <img src="https://github.com/user-attachments/assets/913d39f6-5244-4cea-b6ee-62bd2e6feaf4" alt="Download Screen" width="600"/>
      </td>
    </tr>
  </table>
</div>

### Windows / macOS

1. Open the extracted folder.
   
2. Run the backend:
   ```bash
   ./LocalMetrics.Api.exe      # Windows
   ./LocalMetrics.Api          # macOS
   ```
3. From the root folder, run the UI:
   ```bash
   docker compose up
   ```
   Or if running locally:
   ```bash
   dotnet run --project src/LocalMetrics.UI
   ```

Then access: [http://localhost](http://localhost)

---



## API Endpoints

- `GET /metrics` — Prometheus-compatible metrics in plaintext  
- `GET /api/SystemMetrics` — Returns encrypted system metrics  
- `POST /api/SystemMetrics/decrypt` — Decrypts and returns readable system metrics  

Example:
```bash
curl http://localhost:5050/metrics
```
> ⚠ Port 5050 is used by default.

---

## ⚙️ Configuration

```bash
# The API selects a system metrics collector based on the OS (Windows, Linux or macOS).
# Metrics are cached in memory (default: 5 seconds) to reduce system load.

# API responses are encrypted using AES with a key defined in appsettings.json.
# The UI fetches encrypted data every 5 seconds and sends it to the API for decryption.

# When using Docker, nginx proxies:
# - "/"      → Blazor UI
# - "/api/*" → API running on the host via host.docker.internal

# The UI reads the API base URL from appsettings.json (local or Docker-specific).

```

---
## Contributing

Contributions are welcome! If you encounter any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

---

## License

[MIT License](https://github.com/louresb/LocalMetrics/blob/main/LICENSE) © [Bruno Loures](https://github.com/louresb)
