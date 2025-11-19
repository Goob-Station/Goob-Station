// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Content.Tools
{
    public sealed class TypeTagPreserver : IEmitter
    {
        public TypeTagPreserver(IEmitter emitter)
        {
            Emitter = emitter;
        }

        private IEmitter Emitter { get; }

        public void Emit(ParsingEvent @event)
        {
            if (@event is MappingStart mapping)
            {
                @event = new MappingStart(mapping.Anchor, mapping.Tag, false, mapping.Style, mapping.Start, mapping.End);
            }

            Emitter.Emit(@event);
        }
    }
}
