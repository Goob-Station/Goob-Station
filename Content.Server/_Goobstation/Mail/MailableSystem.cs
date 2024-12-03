using Content.Shared.Clothing;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Mailable
{
    public sealed class  MailableSystem : EntitySystem{

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MailableComponent, ClothingGotEquippedEvent>(OnEquipped);
            SubscribeLocalEvent<MailableComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        }
        private void OnEquipped(EntityUid uid, MailableComponent component, ref ClothingGotEquippedEvent args)
        {
            EnsureComp<MailableComponent>(args.Wearer);
        }

        private void OnUnequipped(EntityUid uid, MailableComponent component, ref ClothingGotUnequippedEvent args)
        {
            RemComp<MailableComponent>(args.Wearer);
        }
    }
}
