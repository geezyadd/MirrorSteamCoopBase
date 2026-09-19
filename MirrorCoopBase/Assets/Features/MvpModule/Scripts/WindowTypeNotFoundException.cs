using System;

namespace Features.MvpModule {
    public class WindowTypeNotFoundException : Exception {
        public WindowTypeNotFoundException(Type windowType, string operationName) : base(
            GenerateMessage(windowType, operationName)) { }
        public WindowTypeNotFoundException(Type windowType, string operationName, Exception innerException) : base(
            GenerateMessage(windowType, operationName) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type windowType, string operationName) =>
            $"Cannot perform {operationName} operation, as window of type {windowType} was not present in the dictionary." +
            " Windows service may not be configured properly, make sure that all required windows have been passed to the service during initialization" +
            " or added to the dictionary manually";
    }
}