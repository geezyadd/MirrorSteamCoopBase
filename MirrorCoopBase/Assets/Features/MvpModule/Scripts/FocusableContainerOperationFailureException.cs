using System;

namespace Features.MvpModule {
    public class FocusableContainerOperationFailureException : Exception {
        public FocusableContainerOperationFailureException(Type containerType, string operationName, FocusableContainerOperationFailureCause cause) : base(
            GenerateMessage(containerType, operationName, cause)) { }

        private static string GenerateMessage(Type containerType, string operationName, FocusableContainerOperationFailureCause cause) =>
            $"Cannot perfom {operationName} operation for focusable container of type {containerType}, because {cause}";
    }
}