using System;

namespace Features.MvpModule {
    public class WindowIsNotOpenedException : Exception {
        public WindowIsNotOpenedException(Type windowType, string operationName) : base(GenerateMessage(windowType, operationName)) { }

        private static string GenerateMessage(Type windowType, string operationName) {
            return $"Cannot perform {operationName} operation, as window of type {windowType} is not opened. For correct " +
                   $"usage of operation {operationName}, you need to call Open method in {windowType} for correct initialization";
        }
    }
}