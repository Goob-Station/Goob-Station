using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Shared._CorvaxGoob.BookOfGreentext;

public sealed class BookOfGreentextSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookOfGreentextComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BookOfGreentextComponent, BookOfGreentextDoAfterEvent>(OnDoAfter);
    }

    private void OnUseInHand(Entity<BookOfGreentextComponent> entity, ref UseInHandEvent ev)
    {
        if (ev.Handled)
            return;

        if (HasComp<CurseOfBookOfGreentextComponent>(ev.User))
        {
            _popup.PopupClient(Loc.GetString("book-of-greentext-already-taken"), ev.User);
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ev.User, entity.Comp.UseDelay, new BookOfGreentextDoAfterEvent(), entity)
        {
            NeedHand = true,
            BreakOnMove = true
        });
    }

    private void OnDoAfter(Entity<BookOfGreentextComponent> entity, ref BookOfGreentextDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Handled)
            return;

        EnsureComp<CurseOfBookOfGreentextComponent>(ev.User, out var curse);

        curse.Book = entity;
        _popup.PopupClient(Loc.GetString("book-of-greentext-use-message"), ev.User);
    }
}
