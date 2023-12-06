using System.Numerics;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;

namespace Content.Server.TestHorn;

public sealed class TestHornSystem : EntitySystem
{
    [Dependency] private ThrowingSystem _throwingSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TestHornComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TestHornComponent, UseInHandEvent>(UseItem);
    }

    private void OnStartup(EntityUid uid, TestHornComponent component, ComponentStartup args)
    {
    }

    private void UseItem(EntityUid uid, TestHornComponent component, UseInHandEvent args)
    {
        _throwingSystem.TryThrow(args.User, new Vector2(5.0f, 0.0f), 20, uid, 0);
    }

}
