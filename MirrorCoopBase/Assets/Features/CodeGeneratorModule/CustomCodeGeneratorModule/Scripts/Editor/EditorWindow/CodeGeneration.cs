using System;
using System.Collections.Generic;
using System.IO;
using Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core;
using UnityEditor;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.EditorWindow {
    public static class CodeGeneration {
        private const string DEFAULT_FOLDER_PATH = "Assets/UnityCodeGen.Generated";
        private const char SEPARATOR = '/';

        public static void GenerateByGenerator(Type value) {
            if (typeof(ICodeGenerator).IsAssignableFrom(value)) {
                ICodeGenerator generator = (ICodeGenerator)Activator.CreateInstance(value);
                GeneratorContext generatorContext = new();
                generator.Execute(generatorContext);
                GenerateScriptFromContext(generatorContext);
            }
            else if (typeof(ICodeGeneratorWithSubGenerators).IsAssignableFrom(value))
                GenerateBySubGenerator(value);
            else
                throw new Exception($"{value.Name} is not generator");
        }
        
        public static void GenerateBySubGenerator(Type value, List<string> saveData) {
            ICodeGeneratorWithSubGenerators generator = (ICodeGeneratorWithSubGenerators)Activator.CreateInstance(value);
            GeneratorContext generatorContext = new();
            generator.ExecuteSubGeneratorByNames(generatorContext, saveData);
            GenerateScriptFromContext(generatorContext);
        }
        
        public static void GenerateBySubGenerator(Type value) {
            ICodeGeneratorWithSubGenerators generator = (ICodeGeneratorWithSubGenerators)Activator.CreateInstance(value);
            GeneratorContext generatorContext = new();
            generator.ExecuteSubGeneratorByNames(generatorContext, generator.GetSubGeneratorNames());
            GenerateScriptFromContext(generatorContext);
        }
        
        private static void GenerateScriptFromContext(GeneratorContext context) {
            string folderPath = context.overrideFolderPath ?? DEFAULT_FOLDER_PATH;

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (CodeText code in context.codeList) {
                string[] hierarchy = code.fileName.Split(SEPARATOR);
                string path = folderPath;
                for (int i = 0; i < hierarchy.Length; i++) {
                    path += SEPARATOR + hierarchy[i];
                    if (i == hierarchy.Length - 1)
                        break;

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                if (File.Exists(path)) {
                    string text = File.ReadAllText(path);
                    if (text == code.text)
                        continue;
                }

                File.WriteAllText(path, code.text);
            }
            
            EditorUtility.RequestScriptReload();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}