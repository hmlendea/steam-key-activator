[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Build Status](https://github.com/hmlendea/steam-key-activator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-key-activator/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/steam-key-activator)](https://github.com/hmlendea/steam-key-activator/releases/latest)

## Running in background as a service

**Note:** The following instructions only apply for *Linux* distributions using *systemd*.

Create the following service file: /usr/lib/systemd/system/steam-key-activator.service
```
[Unit]
Description=Steam Key Activator
After=network.target

[Service]
WorkingDirectory=[ABSOLUTE_PATH_TO_SERVICE_DIRECTORY]
ExecStart=[ABSOLUTE_PATH_TO_SERVICE_DIRECTORY]/SteamKeyActivator
User=[YOUR_USERNAME]

[Install]
WantedBy=multi-user.target
```

Create the following timer file: /lib/systemd/system/steam-key-activator.timer
```
[Unit]
Description=Periodically activates a key on Steam

[Timer]
OnBootSec=5min
OnUnitActiveSec=10min

[Install]
WantedBy=timers.target
```

Values that you might want to change:
 - *OnBootSec*: the delay before the service is started after the OS is booted
 - *OnUnitActiveSec*: how often the service will be triggered

In the above example, the service will start 5 minutes after boot, and then again once every 50 minutes.