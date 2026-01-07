using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    // [SerializeField] private ShieldController shieldController;

    protected override void Configure(IContainerBuilder builder)
    {
        // builder.RegisterComponent(shieldController);
        builder.Register<IInputManager, InputManager>(Lifetime.Singleton);
    }
}