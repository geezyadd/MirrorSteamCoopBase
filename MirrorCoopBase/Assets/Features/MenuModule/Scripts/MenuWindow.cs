using System;
using Features.MvpModule;
using Zenject;

namespace Features.MenuModule.Scripts {
    public sealed class MenuWindow : FocusableWindowBehaviour, IInitializable, IDisposable {
        public MenuWindow(
            IWindowsFactory windowsFactory,
            IWindowsComponentsFinderService windowsComponentsFinderService,
            IWindowsService windowsService,
            IFocusablesService focusablesService)
            : base(windowsFactory, windowsComponentsFinderService, windowsService, focusablesService) { }
    }
}
