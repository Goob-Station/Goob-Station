using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Popups;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/nuclearreactor.dm

public abstract class SharedNuclearReactorSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    protected static readonly int _gridWidth = NuclearReactorComponent.ReactorGridWidth;
    protected static readonly int _gridHeight = NuclearReactorComponent.ReactorGridHeight;

    public override void Initialize()
    {
        base.Initialize();

        // BUI event
        SubscribeLocalEvent<NuclearReactorComponent, ReactorEjectItemMessage>(OnEjectItemMessage);
    }

    private void OnEjectItemMessage(EntityUid uid, NuclearReactorComponent component, ReactorEjectItemMessage args)
    {
        if (component.PartSlot.Item == null)
            return;

        _slotsSystem.TryEjectToHands(uid, component.PartSlot, args.Actor);
    }

    protected bool ReactorTryGetSlot(EntityUid uid, string slotID, out ItemSlot? itemSlot) => _slotsSystem.TryGetSlot(uid, slotID, out itemSlot);

    protected static ReactorPartComponent?[,] SelectPrefab(string select) => select switch
    {
        "normal" => NuclearReactorPrefabs.Normal,
        "debug" => NuclearReactorPrefabs.Debug,
        "meltdown" => NuclearReactorPrefabs.Meltdown,
        "alignment" => NuclearReactorPrefabs.Alignment,
        "arachne" => NuclearReactorPrefabs.Arachne,
        "lens" => NuclearReactorPrefabs.Lens,
        _ => NuclearReactorPrefabs.Empty,
    };

    public void UpdateGridVisual(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                var gridComp = comp.ComponentGrid[x, y];
                var vector = new Vector2i(x, y);

                if (gridComp == null)
                {
                    comp.VisualData.Remove(vector);
                }
                else
                {
                    var data = new ReactorCapVisualData { cap = gridComp.IconStateCap, color = _proto.Index(gridComp.Material).Color };
                    if (!comp.VisualData.TryAdd(vector, data))
                        comp.VisualData[vector] = data;
                }
            }
        }
        Dirty(ent);

        // Sanity check to make sure there is actually an appearance component (nullpointer hell)
        if (!_entityManager.HasComponent<AppearanceComponent>(uid))
            return;

        // The data being set doesn't really matter, it just has to trigger AppearanceChangeEvent and the client will handle the rest
        if (!_appearance.TryGetData(uid, ReactorCapVisuals.Sprite, out bool prevValue))
            _appearance.SetData(uid, ReactorCapVisuals.Sprite, true);
        _appearance.SetData(uid, ReactorCapVisuals.Sprite, !prevValue);
    }

    protected void UpdateTempIndicators(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        if (comp.Temperature >= comp.ReactorOverheatTemp)
        {
            if(!comp.IsSmoking)
            {
                comp.IsSmoking = true;
                _appearance.SetData(uid, ReactorVisuals.Smoke, true);
                _popupSystem.PopupEntity(Loc.GetString("reactor-smoke-start", ("owner", uid)), uid, PopupType.MediumCaution);
            }
            if (comp.Temperature >= comp.ReactorFireTemp && !comp.IsBurning)
            {
                comp.IsBurning = true;
                _appearance.SetData(uid, ReactorVisuals.Fire, true);
                _popupSystem.PopupEntity(Loc.GetString("reactor-fire-start", ("owner", uid)), uid, PopupType.MediumCaution);
            }
            else if (comp.Temperature < comp.ReactorFireTemp && comp.IsBurning)
            {
                comp.IsBurning = false;
                _appearance.SetData(uid, ReactorVisuals.Fire, false);
                _popupSystem.PopupEntity(Loc.GetString("reactor-fire-stop", ("owner", uid)), uid, PopupType.Medium);
            }
        }
        else
        {
            if(comp.IsSmoking)
            {
                comp.IsSmoking = false;
                _appearance.SetData(uid, ReactorVisuals.Smoke, false);
                _popupSystem.PopupEntity(Loc.GetString("reactor-smoke-stop", ("owner", uid)), uid, PopupType.Medium);
            }
        }
    }

    protected static bool AdjustControlRods(NuclearReactorComponent comp, float change) { 
        var newSet = Math.Clamp(comp.ControlRodInsertion + change, 0, 2);
        if (comp.ControlRodInsertion != newSet)
        {
            comp.ControlRodInsertion = newSet;
            return true;
        }
        return false; 
    }
}

public static class NuclearReactorPrefabs
{
    private static readonly ReactorPartComponent c = BaseReactorComponents.ControlRod;
    private static readonly ReactorPartComponent f = BaseReactorComponents.FuelRod;
    private static readonly ReactorPartComponent g = BaseReactorComponents.GasChannel;
    private static readonly ReactorPartComponent h = BaseReactorComponents.HeatExchanger;

    public static readonly ReactorPartComponent?[,] Empty =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Normal =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            g, h, g, h, g, h, g
        },
        {
            h, null, c, null, c, null, h
        },
        {
            g, h, g, h, g, h, g
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Debug =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            g, h, g, h, g, h, g
        },
        {
            h, f, c, f, c, f, h
        },
        {
            g, h, g, h, g, h, g
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Meltdown =
    {
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
    };

    public static readonly ReactorPartComponent?[,] Alignment =
    {
        {
            null, null, null, null, null, null, c
        },
        {
            null, null, null, null, null, c, null
        },
        {
            null, null, null, null, c, null, null
        },
        {
            null, null, null, c, null, null, null
        },
        {
            null, null, c, null, c, null, null
        },
        {
            null, c, null, null, null, c, null
        },
        {
            c, null, null, null, null, null, c
        }
    };

    public static readonly ReactorPartComponent?[,] Arachne =
    {
        {
            g, h, g, h, g, h, g
        },
        {
            h, f, c, f, c, f, h
        },
        {
            g, c, c, g, c, c, g
        },
        {
            h, f, g, c, g, f, h
        },
        {
            g, c, c, g, c, c, g
        },
        {
            h, f, c, f, c, f, h
        },
        {
            g, h, g, h, g, h, g
        }
    };

    public static readonly ReactorPartComponent?[,] Lens =
    {
        {
            g, h, g, h, g, h, g
        },
        {
            h, f, c, g, c, f, h
        },
        {
            g, c, c, h, c, c, g
        },
        {
            h, g, h, f, h, g, h
        },
        {
            g, c, c, h, c, c, g
        },
        {
            h, f, c, g, c, f, h
        },
        {
            g, h, g, h, g, h, g
        }
    };
}
