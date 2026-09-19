using System.Collections.Generic;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core {
    public sealed class GeneratorContext {
        private readonly List<CodeText> _codeList = new();

        internal IReadOnlyList<CodeText> codeList =>
            _codeList;
        internal string overrideFolderPath { get; private set; }

        public void AddCode(string fileName, string text) {
            _codeList.Add(new CodeText {
                fileName = fileName, text = text
            });
        }

        public void OverrideFolderPath(string path) {
            overrideFolderPath = path;
        }
    }
}