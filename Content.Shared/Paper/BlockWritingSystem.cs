// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SpeltIncorrectyl <66873282+SpeltIncorrectyl@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Paper;

/// <summary>
/// A system that prevents those with the IlliterateComponent from writing on paper.
/// Has no effect on reading ability.
/// </summary>
public sealed class BlockWritingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlockWritingComponent, PaperWriteAttemptEvent>(OnPaperWriteAttempt);
    }

    private void OnPaperWriteAttempt(Entity<BlockWritingComponent> entity, ref PaperWriteAttemptEvent args)
    {
        args.FailReason = entity.Comp.FailWriteMessage;
        args.Cancelled = true;
    }
}