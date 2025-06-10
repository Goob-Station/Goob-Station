// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Clickable;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Tests.Client
{
    [TestFixture]
    public sealed class ClickMapTest
    {
        [Test]
        public void TestBasic()
        {
            var img = new Image<Rgba32>(2, 2)
            {
                [0, 0] = new(0, 0, 0, 0f),
                [1, 0] = new(0, 0, 0, 1f),
                [0, 1] = new(0, 0, 0, 1f),
                [1, 1] = new(0, 0, 0, 0f)
            };

            var clickMap = ClickMapManager.ClickMap.FromImage(img, 0.5f);

            Assert.That(clickMap.IsOccluded(0, 0), Is.False);
            Assert.That(clickMap.IsOccluded(1, 0), Is.True);
            Assert.That(clickMap.IsOccluded(0, 1), Is.True);
            Assert.That(clickMap.IsOccluded(1, 1), Is.False);
        }

        [Test]
        public void TestThreshold()
        {
            var img = new Image<Rgba32>(2, 2)
            {
                [0, 0] = new(0, 0, 0, 0f),
                [1, 0] = new(0, 0, 0, 0.25f),
                [0, 1] = new(0, 0, 0, 0.75f),
                [1, 1] = new(0, 0, 0, 1f)
            };

            var clickMap = ClickMapManager.ClickMap.FromImage(img, 0.5f);

            Assert.That(clickMap.IsOccluded(0, 0), Is.False);
            Assert.That(clickMap.IsOccluded(1, 0), Is.False);
            Assert.That(clickMap.IsOccluded(0, 1), Is.True);
            Assert.That(clickMap.IsOccluded(1, 1), Is.True);
        }
    }
}