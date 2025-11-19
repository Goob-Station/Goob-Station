<!--
SPDX-FileCopyrightText: 2025 Space Station 14 Contributors

SPDX-License-Identifier: MIT-WIZARDS
-->

<p align="center"> <img alt="Goob Station 14" width="880" height="300" src="https://github.com/Goob-Station/Goob-Station/blob/master/Resources/Textures/Logo/logo.png" /></p>

This is a fork from the primary repo for Space Station 14 called Goob Station. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server this is the content pack for Goob Station.

If you want to host or create content for SS14, go to the [Space Station 14 repository](https://github.com/space-wizards/space-station-14) as it contains both RobustToolbox and the content pack for development of new content packs and is the base for your fork.

## Links

[Goob Station Discord Server](https://discord.gg/goobstation) | [Goob Station Development Discord Server](https://discord.gg/zXk2cyhzPN) | [Goob Station Forum](https://forums.goobstation.com/) | [Goob Station Website](https://goobstation.com)

## Documentation/Wiki

The Goob Station [docs site](https://docs.goobstation.com/) has documentation on GS14's content, engine, game design, and more. It also has lots of resources for new contributors to the project.

## Contributing

We are happy to accept contributions from anybody. Get in [Development Discord Server](https://discord.gg/zXk2cyhzPN) if you want to help. Feel free to check the [list of issues](https://github.com/Goob-Station/Goob-Station/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help either!
While following the [Space Station 14 contribution guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html) is not mandatory for Goob Station, we recommend reviewing them for best practices.

We are not currently accepting translations of the game on our main repository. If you would like to translate the game into another language consider creating a fork or contributing to a fork.

## Building

1. Clone this repo.
2. Run `RUN_THIS.py` to init submodules and download the engine.
3. Compile the solution.

[More detailed instructions on building the project.](https://docs.goobstation.com/en/general-development/setup.html)

## License

Each file in this codebase includes a license header that clearly defines its terms. Most unique files in this repository are licensed under the Mozilla Public License (MPL). Files originating from upstream (the Space Station 14 codebase) are licensed under MIT. Any dual or multiple licenses are explicitly indicated with two or more license lines in the file header. Please refer to the headers for the exact licensing terms.

Most media assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.

### MPL for Dummies, Forks, and people looking to port our content.

The Mozilla Public License (MPL) is a “file-level” license, which means it only applies to the specific files it covers. Unlike AGPL, which requires the entire project using your code to also be AGPL (strong copyleft), MPL allows your code to be included in any codebase (open or closed source) as long as the MPL license stays with that file.

If you distribute binaries of an MPL-covered file, you must make that file’s source code available. You do **not** need to release the source code of the rest of the project. This makes it much easier to combine MPL-licensed files with other projects without affecting their overall licensing.

Most MPL-licensed files can also be relicensed under stronger copyleft licenses, such as GPL or AGPL, if desired.
**Exception:** files licensed under `MPL-2.0-no-copyleft-exception` cannot be ported to heavier copyleft licenses.
