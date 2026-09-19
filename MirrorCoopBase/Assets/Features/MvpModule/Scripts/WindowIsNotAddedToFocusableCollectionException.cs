using System;

namespace Features.MvpModule {
    public class WindowIsNotAddedToFocusableCollectionException : Exception {
        public WindowIsNotAddedToFocusableCollectionException(Type windowType) : base(GenerateMessage(windowType)) { }

        private static string GenerateMessage(Type windowType) =>
            $"Window of type {windowType} was not added to the focusable collection. It may cause unexpected behaviour. Please, " +
            $"make sure, that you open and close windows correctly using public API methods of window itself or using WindowsService";
    }
}