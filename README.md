![Logo](https://i.ibb.co/Cs1wHfKJ/image-1.png)
# FL Studio Discord RPC (FLRPC)

A simple, yet cool way to show off your FL Studio projects to your friends and others.


## Features

- Secret mode (meaning others can't see what project you're working on)
- Display optional accurate FL Studio version (ex. FL Studio 20.8.4.1873)
- A GUI based easy-to-manage configuration editor ("FLRPC.exe -configfile" to edit and "FLRPC.exe -reset" to delete the current configuration)
- Portable executable without having to have dlls if you want to use the app
- Arguments to use the app from console
- Hidden window so you can forget about the app running and won't accidentally close it (Have to be closed with the task manager)
- Works without user activity, detects if FL Studio is open or not to start/stop the RPC

## Pros
- Very lightweight and resource efficient
- Uses only 4 external packages
- Almost everything is commented, so the code is easily manageable and readable

## Cons
- No integration into actual FL Studio (meaning it must run in the background)
- ~Has very little features (as of now)~ I'm working on it :D

## Screenshots

![RPC Screenshot 1](https://i.imgur.com/XJzzJcm.png)

![RPC Screenshot 2](https://i.imgur.com/viJFFoI.png)

![RPC Screenshot 3](https://i.ibb.co/pr3YtYy9/Screenshot-2025-06-28-141502.png)

![RPC Screenshot 4](https://i.ibb.co/vxMKq8mG/Screenshot-2025-06-28-141627.png)

## Installation

You can either simply run the pre-compiled release, or build it yourself! I currently use .NET Framework 4.8.0 (although you could probably downgrade it if you build the project yourself)
## Feedback

If you have any feedback, reach out to me on discord - @AdrianoTech


## Features that may (or may not) be added in the future

- Actual FL Studio integration

- ~Optional GUI Interface~

- More features

(These are some good ones, to be added!)



## Packages used

[DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp)\
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)\
[Colorful.Console](https://github.com/tomakita/Colorful.Console)\
[Costura.Fody](https://github.com/Fody/Costura)


## License

This project is licensed under the [MIT](https://opensource.org/license/mit/) License - see the [LICENSE](LICENSE) file for details.
