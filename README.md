Note Sync
=========

## Introduction ##
Note Sync is a simple Windows service that syncs written notes from your Sony E-Reader device to your Evernote Account. 
Because the notes on the device do not have a modified time stamp, it will always replace the note with the version found on the device.
It will tag new notes with a configurable tag (default: `NoteSync`).

## Building the solution ##
The solution has been built using Microsoft Visual Studio 2017 (Community Edition).
Make sure to restore NuGet packages if your environment is not configured to Restore them automatically when building.

## Debugging the service ##
Set `WindowsService` as the startup project. The service will run in Console Mode.

## Installing the service ##
Open a prompt or PowerShell with administrative rights. run the command `WindowsService install`.
Upon first start, it will create default configuration files in `ProgramData\NoteSync`. Enter valid Evernote API credentials in  the `EvernoteSettings.Settings` file and (re)start the Windows Service.

## Using the Service ##
The service will monitor for attached USB storage devices. It will automatically try to detect whether this is a SONY reader device and starts synchronization.