using System;

namespace Features.MvpModule {
    public class PresentersNotCollectedException : Exception {
        public PresentersNotCollectedException(Type windowType) : base(GenerateMessage(windowType)) { }
        public PresentersNotCollectedException(Type windowType, Exception innerException) : base(GenerateMessage(windowType) + $"\n Exception text: {innerException}") { }

        private static string GenerateMessage(Type windowType) =>
            $"Presenters of window of type {windowType} were not collected. You have to call method CollectPresenters or enable " +
            $"collecting if not found in other method calls";
    }
}