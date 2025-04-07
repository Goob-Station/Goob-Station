// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.NodeContainer;
using Content.Server.Power.Components;
using Content.Server.Power.NodeGroups;
using Content.Server.Tools;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Server.Power.EntitySystems
{
    [UsedImplicitly]
    public sealed class CableMultitoolSystem : EntitySystem
    {
        [Dependency] private readonly ToolSystem _toolSystem = default!;
        [Dependency] private readonly PowerNetSystem _pnSystem = default!;
        [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CableComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
            SubscribeLocalEvent<CableComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        }

        private void OnAfterInteractUsing(EntityUid uid, CableComponent component, AfterInteractUsingEvent args)
        {
            if (args.Handled || args.Target == null || !args.CanReach || !_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality))
                return;

            var markup = FormattedMessage.FromMarkupOrThrow(GenerateCableMarkup(uid));
            _examineSystem.SendExamineTooltip(args.User, uid, markup, false, false);
            args.Handled = true;
        }

        private void OnGetExamineVerbs(EntityUid uid, CableComponent component, GetVerbsEvent<ExamineVerb> args)
        {
            // Must be in details range to try this.
            // Theoretically there should be a separate range at which a multitool works, but this does just fine.
            if (_examineSystem.IsInDetailsRange(args.User, args.Target))
            {
                var held = args.Using;

                // Pulsing is hardcoded here because I don't think it needs to be more complex than that right now.
                // Update if I'm wrong.
                var enabled = held != null && _toolSystem.HasQuality(held.Value, SharedToolSystem.PulseQuality);
                var verb = new ExamineVerb
                {
                    Disabled = !enabled,
                    Message = Loc.GetString("cable-multitool-system-verb-tooltip"),
                    Text = Loc.GetString("cable-multitool-system-verb-name"),
                    Category = VerbCategory.Examine,
                    Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/zap.svg.192dpi.png")),
                    Act = () =>
                    {
                        var markup = FormattedMessage.FromMarkupOrThrow(GenerateCableMarkup(uid));
                        _examineSystem.SendExamineTooltip(args.User, uid, markup, false, false);
                    }
                };

                args.Verbs.Add(verb);
            }
        }

        private string GenerateCableMarkup(EntityUid uid, NodeContainerComponent? nodeContainer = null)
        {
            if (!Resolve(uid, ref nodeContainer))
                return Loc.GetString("cable-multitool-system-internal-error-missing-component");

            foreach (var node in nodeContainer.Nodes)
            {
                if (!(node.Value.NodeGroup is IBasePowerNet))
                    continue;
                var p = (IBasePowerNet) node.Value.NodeGroup;
                var ps = _pnSystem.GetNetworkStatistics(p.NetworkNode);

                float storageRatio = ps.InStorageCurrent / Math.Max(ps.InStorageMax, 1.0f);
                float outStorageRatio = ps.OutStorageCurrent / Math.Max(ps.OutStorageMax, 1.0f);
                return Loc.GetString("cable-multitool-system-statistics",
                    ("supplyc", ps.SupplyCurrent),
                    ("supplyb", ps.SupplyBatteries),
                    ("supplym", ps.SupplyTheoretical),
                    ("consumption", ps.Consumption),
                    ("storagec", ps.InStorageCurrent),
                    ("storager", storageRatio),
                    ("storagem", ps.InStorageMax),
                    ("storageoc", ps.OutStorageCurrent),
                    ("storageor", outStorageRatio),
                    ("storageom", ps.OutStorageMax)
                );
            }
            return Loc.GetString("cable-multitool-system-internal-error-no-power-node");
        }
    }
}