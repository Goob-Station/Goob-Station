// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Shared.DeviceLinking.Events
{
    public sealed class PortDisconnectedEvent : EntityEventArgs
    {
        public readonly string Port;

        public PortDisconnectedEvent(string port)
        {
            Port = port;
        }
    }
}