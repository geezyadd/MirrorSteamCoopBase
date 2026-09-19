using System;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratorAttribute : Attribute {
        public int GeneratorOrder { get; }

        public GeneratorAttribute(int generatorOrder = 0) {
            GeneratorOrder = generatorOrder;
        }
    }
}