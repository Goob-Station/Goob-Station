// SPDX-FileCopyrightText: 2023 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chat.Systems;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class UnblockableSpeechSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<UnblockableSpeechComponent, CheckIgnoreSpeechBlockerEvent>(OnCheck);
        }

        private void OnCheck(EntityUid uid, UnblockableSpeechComponent component, CheckIgnoreSpeechBlockerEvent args)
        {
            args.IgnoreBlocker = true;
        }
    }
}