using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._Lavaland.Mobs;

[RegisterComponent]
public sealed partial class HierophantChaserComponent : Component
{
    [DataField] public float Speed = 3f;
}
