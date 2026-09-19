using System;

namespace Features.MvpModule {
    public class ViewIsNotSuitableException : Exception {
        public ViewIsNotSuitableException(Type viewType) : base(GenerateMessage(viewType)) { }
        public ViewIsNotSuitableException(Type viewType, Exception innerException) : base(GenerateMessage(viewType) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type viewType) =>
            $"View of type {viewType} is not suitable. Make sure you followed next conventions: " +
            $"\n - View you are trying to use inherits from base class, name of which ends with \"ViewBase\"" +
            $"\n - ViewBase must inherit ViewBehaviour class";
    }
}