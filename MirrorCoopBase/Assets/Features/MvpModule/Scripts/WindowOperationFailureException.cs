using System;

namespace Features.MvpModule {
    public class WindowOperationFailureException : Exception {
        public WindowOperationFailureException(Type windowType, string operationName, WindowOperationFailureCause cause) : base(
            GenerateMessage(windowType, operationName, cause)) { }

        public WindowOperationFailureException(Type windowType, string operationName, WindowOperationFailureCause cause, Exception innerException) : base(
            GenerateMessage(windowType, operationName, cause) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type windowType, string operationName, WindowOperationFailureCause cause) =>
            $"Cannot perfom {operationName} operation for window of type {windowType}, because {cause}";
    }
}