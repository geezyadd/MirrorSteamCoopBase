using System;

namespace Features.MvpModule {
    public class PresenterNotFoundException : Exception {
        public PresenterNotFoundException(Type windowType, Type presenterType) : base(GenerateMessage(windowType, presenterType)) { }
        public PresenterNotFoundException(Type windowType, Type presenterType, Exception innerException) : base(GenerateMessage(windowType, presenterType) + $"\n Exception text: {innerException}") { }
        public PresenterNotFoundException(Type presenterType) : base(GenerateMessage(presenterType)) { }
        public PresenterNotFoundException(Type presenterType, Exception innerException) : base(GenerateMessage(presenterType) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type presenterType) =>
            $"Presenter of type {presenterType} could not be found. Possible causes of failure:" +
            $"\n1. Presenters may not be collected from window. You may call method CollectPresenters or enable collecting if not found in other method calls" +
            $"\n2. Window may not have View of type {presenterType.ToString().Replace("Presenter", "")}" +
            $"\n3. Naming conventions may not be followed. Make sure that:" +
            $"      - Presenter is called as base type of view you are searching for + \"Presenter\" at the end." +
            $"      - Presenter and view are located in same assembly.";

        private static string GenerateMessage(Type windowType, Type presenterType) =>
            $"Presenter of type {presenterType} could not be found in window of type {windowType}. Possible causes of failure:" +
            $"\n1. Presenters may not be collected from window {windowType}. You may call method CollectPresenters or enable collecting if not found in other method calls" +
            $"\n2. Window may not have View of type {presenterType.ToString().Replace("Presenter", "")}" +
            $"\n3. Naming conventions may not be followed. Make sure that:" +
            $"      - Presenter is called as base type of view you are searching but \"View Base\" is replaced by \"Presenter\" at the end." +
            $"      - Presenter and view are located in same assembly.";
    }
}