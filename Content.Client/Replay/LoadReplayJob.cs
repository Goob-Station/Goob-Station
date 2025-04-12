// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Content.Client.Replay.UI.Loading;
using Robust.Client.Replays.Loading;

namespace Content.Client.Replay;

public sealed class ContentLoadReplayJob : LoadReplayJob
{
    private readonly LoadingScreen<bool> _screen;

    public ContentLoadReplayJob(
        float maxTime,
        IReplayFileReader fileReader,
        IReplayLoadManager loadMan,
        LoadingScreen<bool> screen)
        : base(maxTime, fileReader, loadMan)
    {
        _screen = screen;
    }

    protected override async Task Yield(float value, float maxValue, LoadingState state, bool force)
    {
        var header = Loc.GetString("replay-loading", ("cur", (int)state + 1), ("total", 5));
        var subText = Loc.GetString(state switch
        {
            LoadingState.ReadingFiles => "replay-loading-reading",
            LoadingState.ProcessingFiles => "replay-loading-processing",
            LoadingState.Spawning => "replay-loading-spawning",
            LoadingState.Initializing => "replay-loading-initializing",
            _ => "replay-loading-starting",
        });
        _screen.UpdateProgress(value, maxValue, header, subText);

        await base.Yield(value, maxValue, state, force);
    }
}