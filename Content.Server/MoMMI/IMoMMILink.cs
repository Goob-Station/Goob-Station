// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.MoMMI
{
    public interface IMoMMILink
    {
        void SendOOCMessage(string sender, string message);
    }
}