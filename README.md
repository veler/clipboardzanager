# ClipboardZanager

ClipboardZanager is a clipboard manager for ``Windows 10`` initiated by Etienne BAUDOUX in 2010. It is designed to keep a history of what the user copies in Windows and let him or her reuse it later. Since August 28th, 2017, ClipboardZanager is open source and is developed with the help of contributors.

The main features of ClipboardZanager are :
* Clipboard conservation.
* Synchronization with the personal user Cloud storage service account (supports ``OneDrive`` and ``DropBox``).
* User's data is encrypted. The user can also decide to ignore all copied data from a specific application.
* Full integration to Windows 10.

[Release notes](https://github.com/veler/clipboardzanager/blob/master/RELEASENOTES.md)

[More info here](http://clipboardzanager.velersoftware.com)

![clipboardzanager](http://medias.velersoftware.com/images/clipboardzanager/1.png)

# Requirements to run ClipboardZanager

* [Windows 10](https://www.microsoft.com/en-us/software-download/windows10) (Anniversary Update (14393) or later) edition Home, Pro, Education, Entreprise, S.

# Setup a development environment

## Requirements

### For Windows

You will need the following tools :
* Windows 10
* .Net 4.6
* Visual Studio 2017 with ``Windows development`` and ``Visual Studio extension development toolset`` and ``Windows 10 SDK (10.0.14393.0)``
* ``AutoRunCustomTool`` extension (that you can find in repository/Tools/AutoRunCustomTool.vsix)
* ``DesktopBridgeDebuggingProject`` extension (that you can find in repository/Tools/DesktopBridgeDebuggingProject.vsix)

### For Android

You will need the following tools :
* Java 8
* Android Studio
* Android SDK to support version ``21`` to ``25``

### For iOS

Well, I need to start the project at least.

## Versioning & Passwords

### Versioning

The application version use the following pattern :

```
year.month.day.buildCountInTheDay
```

Each time that the ``Windows Desktop app is built (with Visual Studio)``, the desktop and Android version number are updated.
The Android app is not built by Visual Studio. The version number is updated but the project still need to be built from Android Studio.

### Passwords & AppKey

The user's data is encrypted with a private key. This key is generated thanks to the private ``OneDrive appkey``, ``DropBox appkey``, and ``assembly version`` number (that changes at each build).

## Build the project

Before building the project, you will need a ``OneDrive appkey`` and ``DropBox appkey`` to be able to generate correctly the private keys used by ClipboardZanager to encrypt data.
You will need to :
* Create an application on the [OneDrive developers portal](https://dev.onedrive.com/) with the following permissions :
  * ``Files.ReadWrite.AppFolder``
  * ``User.Read``
* Create an application on the [DropBox developers portal](https://www.dropbox.com/developers) with the following permissions :
  * ``AppFolder``

Once done, you will have to rename the ``passwords-sample.txt`` file to ``passwords.txt`` and complete it with the ``appkey`` and ``redirection URI`` provided by OneDrive and DropBox.

In general, the security process can be improved and simplified.

Once done, open the solution in Visual Studio. Set ``DesktopBridgeDebuggingProject`` as the ``StartUp Project`` and ``Rebuild the Solution``. The private keys will be updated in the Windows Desktop project and Android project at the following locations :
* For the Windows Desktop app : ``ClipboardZanager\Sources\ClipboardZanager\Properties\Passwords.cs``
* For the Android app : ``Android\app\src\main\res\values\passwords.xml``

Those two files and ``passwords.txt`` are git ignored.
You can then open Android Studio and build the smartphone app.

## Run the project on Windows

There is two ways to debug :
* Running ``Windows Store\DesktopBridgeDebuggingProject`` in ``Debug`` mode. But on some machine dependencies are not well detected.
* The other solution that works everywhere but that is less practical consist in deploying the ``Windows Store\ClipboardZanager`` project in ``Debug`` mode and then use the ``Debug/Other Debug Targets/Debug Installed App Package`` to debug the ``ClipboardZanager`` package.

## Unit tests

On Windows, when we run all the unit tests, ``PasteBarWindowViewModel_Search`` fails, but success when we run it independently. It will be fix soon.

# Contribute

Feel free to contribute to this project in any way : adding feature, opening issues, translating.

# License

[![WTFPL](http://www.wtfpl.net/wp-content/uploads/2012/12/wtfpl-badge-1.png)](http://www.wtfpl.net/)

```
            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
                    Version 2, December 2004

 Copyright (C) 2004 Sam Hocevar <sam@hocevar.net>

 Everyone is permitted to copy and distribute verbatim or modified
 copies of this license document, and changing it is allowed as long
 as the name is changed.

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION

  0. You just DO WHAT THE FUCK YOU WANT TO.
```