// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Packaging;
using Robust.Packaging;

IPackageLogger logger = new PackageLoggerConsole();

if (!CommandLineArgs.TryParse(args, out var parsed))
{
    logger.Error("Unable to parse args, aborting.");
    return;
}

if (parsed.WipeRelease)
    WipeRelease();
else
{
    // Ensure the release directory exists. Otherwise, the packaging will fail.
    Directory.CreateDirectory("release");
}

if (!parsed.SkipBuild)
    WipeBin();

if (parsed.Client)
{
    await ClientPackaging.PackageClient(parsed.SkipBuild, parsed.Configuration, logger);
}
else
{
    await ServerPackaging.PackageServer(parsed.SkipBuild, parsed.HybridAcz, logger, parsed.Configuration, parsed.Platforms);
}

void WipeBin()
{
    logger.Info("Clearing old build artifacts (if any)...");

    if (Directory.Exists("bin"))
        Directory.Delete("bin", recursive: true);
}

void WipeRelease()
{
    if (Directory.Exists("release"))
    {
        logger.Info("Cleaning old release packages (release/)...");
        Directory.Delete("release", recursive: true);
    }

    Directory.CreateDirectory("release");
}