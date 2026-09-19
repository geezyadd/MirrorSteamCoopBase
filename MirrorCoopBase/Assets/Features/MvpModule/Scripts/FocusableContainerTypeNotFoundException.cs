using System;

namespace Features.MvpModule {
    public class FocusableContainerTypeNotFoundException : Exception {
        public FocusableContainerTypeNotFoundException(Type containerType, string operationName) : base(
            GenerateMessage(containerType, operationName)) { }

        private static string GenerateMessage(Type containerType, string operationName) =>
            $"Cannot perform {operationName} operation, as focusable container of type {containerType} was not present in the dictionary." +
            " Focus service may not be configured properly, make sure that all required containers have been passed to the service during initialization" +
            " or added to the dictionary manually";
    }
}