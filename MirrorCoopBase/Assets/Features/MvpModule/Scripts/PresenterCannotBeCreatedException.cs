using System;

namespace Features.MvpModule {
    public class PresenterCannotBeCreatedException : Exception {
        public PresenterCannotBeCreatedException(Type viewType) : base(GenerateMessage(viewType)) { }
        public PresenterCannotBeCreatedException(Type viewType, Exception innerException) : base(GenerateMessage(viewType) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type viewType) =>
            $"Presenter for view of type {viewType} cannot be found. Make sure you followed namming conventions:" +
            $"   \n- Presenter is called as base type of view you are searching but \"View Base\" is replaced by \"Presenter\" at the end. For current case presenter must be called {viewType.BaseType.ToString().Replace("ViewBase", "Presenter")}" +
            $"   \n- Presenter and view are located in same assembly.";
    }
}