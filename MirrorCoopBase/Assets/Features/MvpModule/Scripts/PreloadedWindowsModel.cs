using System.Collections.Generic;

namespace Features.MvpModule {
    public class PreloadedWindowsModel {
        public Dictionary<string, MonoWindowInstance> PreloadedWindows { get; } = new();
    }
}