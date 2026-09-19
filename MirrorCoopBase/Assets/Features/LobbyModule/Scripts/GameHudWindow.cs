using System;
using Features.MvpModule;
using Zenject;

namespace Features.LobbyModule.Scripts {
    public sealed class GameHudWindow : FocusableWindowBehaviour, IInitializable, IDisposable {
        public GameHudWindow(
            IWindowsFactory windowsFactory,
            IWindowsComponentsFinderService windowsComponentsFinderService,
            IWindowsService windowsService,
            IFocusablesService focusablesService)
            : base(windowsFactory, windowsComponentsFinderService, windowsService, focusablesService) { }
    }
}
