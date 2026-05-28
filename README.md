# Unofficial Neewer Light Panel Tool

Neewer Light Panel Tool is a standalone Windows WPF utility for controlling Neewer Bluetooth light panels from third-party stream control software such as StreamDeck, Streamer.bot, SAMMI, or any tool that can send an HTTP request.

This is not the official Neewer light controller. The Neewer Bluetooth protocol used here is undocumented and has only been partially reverse engineered by the community. At this level it is functional for common control tasks, but some commands are based on observed behavior, magic bytes, and magic numbers rather than official protocol documentation.

This tool was built for the Neewer RGB660 Pro. It may also work with other Neewer Bluetooth-controlled lights that use the same underlying protocol. Some models do not support every feature:

- CCT-only lights may ignore RGB commands.
- Some lights may not support all scene effects.
- Unsupported scene buttons may simply do nothing.
- The app discovers devices that report `NEEWER` in their Bluetooth name or ID and lets you try connecting to them.

On May 9, 2026, Neewer released their official desktop controller, Neewer Control Center:

https://support.neewer.com/neewer_control_center_detail?menu=1

Use Neewer Control Center if you want the official desktop controller. This app exists because the official controller does not provide third-party HTTP integration for stream tools.

![Application ScreenShot](https://github.com/Teravus/LightPanelControlTool/blob/main/asset/Screenshot1.png?raw=true)

## Basic Workflow

1. Open `NeewerLightPanelTool`.
2. Click `Scan Bluetooth`.
3. Select one or more discovered lights.
4. Click `Connect Selected`.
5. Optionally create a named group from selected lights.
6. Pick the listening IP address.
7. Pick the HTTP port.
8. Click `Start HTTP`.
9. Change RGB, CCT tone, scene, brightness, or power settings in the UI.
10. Copy the generated URL from `StreamDeck Request` into your third-party controller.

## Listening IP

If StreamDeck, Streamer.bot, SAMMI, or your automation tool is running on the same machine as this app, use:

```text
127.0.0.1
```

If the third-party software is running on another machine, choose the IP address of the network interface that the other machine can reach.

To listen on all network interfaces, use:

```text
0.0.0.0
```

Make sure your firewall allows inbound traffic on the selected port if another machine needs to reach the app.

## HTTP Control

The app starts a small local Kestrel HTTP server when you click `Start HTTP`. The generated request URL at the bottom of the window reflects the current selected light or group and the current control settings.

Targets are specified by either:

```text
group=GroupName
```

or:

```text
light=NEEWER-RGB660 PRO-DCBC28
```

Example requests:

```text
http://127.0.0.1:5088/neewerbt_RGBSet?group=Key&r=255&g=0&b=0&brightness=39
http://127.0.0.1:5088/neewerbt_CCTToneSet?group=Key&tone=4500&brightness=39
http://127.0.0.1:5088/neewerbt_SceneSet?group=Key&scenename=Party&brightness=39
http://127.0.0.1:5088/neewerbt_PowerSet?group=Key&power=off
```

## Persistence

Groups, group state, and HTTP listener settings are saved automatically as a convenience state file under local app data. The app loads that last state on startup.

You can also explicitly save and load group configuration from the UI.

## Requirements

- Windows
- .NET 10
- Bluetooth LE support
- Neewer Bluetooth lights powered on and discoverable

## Status

This is a practical integration tool, not a complete official implementation of every Neewer light feature. The RGB660 Pro path is the primary tested path. Other models may work if they use the same command protocol.
