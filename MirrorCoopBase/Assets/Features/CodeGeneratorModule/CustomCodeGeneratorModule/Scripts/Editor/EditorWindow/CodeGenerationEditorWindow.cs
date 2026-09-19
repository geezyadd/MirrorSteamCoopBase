using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core;
using UnityEditor;
using UnityEngine.UIElements;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.EditorWindow {
    public class CodeGenerationEditorWindow : UnityEditor.EditorWindow {
        private readonly Dictionary<Type, IToogleVisualElement> _generateCodeMap = new();
        private readonly Dictionary<string, IToogleVisualElement> _subGenerateCodeMap = new();
        private readonly Dictionary<Type, List<string>> _generatorWithSubGenerators = new();
        private bool _isAllSelected;

        [MenuItem("Tools/Generations/Code generation %g")]
        private static void Init() {
            CodeGenerationEditorWindow window = (CodeGenerationEditorWindow)GetWindow(typeof(CodeGenerationEditorWindow));
            window.Show();
        }

        private void CreateGUI() =>
            RebuildUI();

        private void RebuildUI() {
            rootVisualElement.Clear();
            _generateCodeMap.Clear();
            _subGenerateCodeMap.Clear();
            _isAllSelected = false;

            Button button = CreateToggleAllButton();
            rootVisualElement.Add(button);
            
            ScrollView scrollView = new(ScrollViewMode.Vertical) {
                style = {
                    maxHeight = 600, flexGrow = 1, flexShrink = 0
                }
            };

            IEnumerable<Type> generatorTypes = TypeCache.GetTypesDerivedFrom<ICodeGenerator>().Where(x => !x.IsAbstract && x.GetCustomAttribute<GeneratorAttribute>() != null).OrderBy(x => x.GetCustomAttribute<GeneratorAttribute>().GeneratorOrder);

            foreach (Type value in generatorTypes) {
                CodeGenerationToggleVisualElement codeGenerationToggleVisualElement = CreateNewToggleGenerationOption();
                codeGenerationToggleVisualElement.Init(value.Name, false);
                scrollView.Add(codeGenerationToggleVisualElement);
                _generateCodeMap.Add(value, codeGenerationToggleVisualElement);
            }
            
            IEnumerable<Type> generatorWithSubGeneratorsTypes = TypeCache.GetTypesDerivedFrom<ICodeGeneratorWithSubGenerators>().Where(x => !x.IsAbstract && x.GetCustomAttribute<GeneratorAttribute>() != null).OrderBy(x => x.GetCustomAttribute<GeneratorAttribute>().GeneratorOrder);

            foreach (Type value in generatorWithSubGeneratorsTypes) {
                ICodeGeneratorWithSubGenerators codeGenerator = (ICodeGeneratorWithSubGenerators)Activator.CreateInstance(value);
                List<string> subGeneratorNames = codeGenerator.GetSubGeneratorNames();

                CodeGenerationLabelWithButtonVisualElement codeGenerationToggleWithButtonVisualElement = CreateNewLabelGenerationOption();
                codeGenerationToggleWithButtonVisualElement.Init(value.Name);
                scrollView.Add(codeGenerationToggleWithButtonVisualElement);

                List<CodeGenerationSubToggleVisualElement> child = new();
                foreach (string subGeneratorName in subGeneratorNames) {
                    CodeGenerationSubToggleVisualElement codeGenerationSubToggleVisualElement = CreateNewToggleSubGenerationOption();
                    codeGenerationSubToggleVisualElement.Init(subGeneratorName, false);
                    child.Add(codeGenerationSubToggleVisualElement);
                    scrollView.Add(codeGenerationSubToggleVisualElement);
                    _subGenerateCodeMap.Add(subGeneratorName, codeGenerationSubToggleVisualElement);

                    if (_generatorWithSubGenerators.TryGetValue(value, out List<string> generator)) {
                        if(!generator.Contains(subGeneratorName))
                            generator.Add(subGeneratorName);
                    }
                    else
                        _generatorWithSubGenerators.Add(value, new List<string> {
                            subGeneratorName
                        });
                }
                    
                codeGenerationToggleWithButtonVisualElement.InitializeChild(child);
            }
            
            rootVisualElement.Add(scrollView);
            
            Button clearSelectedButton = CreateGenerateSelectedButton();
            rootVisualElement.Add(clearSelectedButton);
        }

        private Button CreateGenerateSelectedButton() {
            Button button = new() {
                text = "Generate Selected Code"
            };

            button.clickable.clicked += GenerateSelected;
            return button;
        }

        private void GenerateSelected() {
            if (!EditorUtility.DisplayDialog("Generate Selected Code", "Do you really want to Generate Selected Code?", "Yes", "No"))
                return;

            List<string> subDataToGenerate = new();
            foreach ((string key, IToogleVisualElement toogleVisualElement) in _subGenerateCodeMap)
                if (toogleVisualElement.IsToggled)
                    subDataToGenerate.Add(key);

            foreach (KeyValuePair<Type, List<string>> generatorWithSubGenerator in _generatorWithSubGenerators) {
                List<string> saveData = generatorWithSubGenerator.Value.Where(v => subDataToGenerate.Contains(v)).ToList();
                if (saveData.Count != 0)
                    CodeGeneration.GenerateBySubGenerator(generatorWithSubGenerator.Key, saveData);
            }

            List<Type> dataToGenerate = new();
            foreach ((Type key, IToogleVisualElement toogleVisualElement) in _generateCodeMap)
                if (toogleVisualElement.IsToggled)
                    dataToGenerate.Add(key);

            dataToGenerate.ForEach(saveData => CodeGeneration.GenerateByGenerator(saveData));
            RebuildUI();
        }

        private Button CreateToggleAllButton() {
            Button button = new() {
                text = "Toggle All"
            };

            button.clickable.clicked += ToggleAllSelected;
            return button;
        }

        private void ToggleAllSelected() {
            _isAllSelected = !_isAllSelected;

            foreach (IToogleVisualElement toggleClearSaves in _generateCodeMap.Values)
                toggleClearSaves.IsToggled = _isAllSelected;

            foreach (IToogleVisualElement toggleClearSaves in _subGenerateCodeMap.Values)
                toggleClearSaves.IsToggled = _isAllSelected;
        }

        private CodeGenerationToggleVisualElement CreateNewToggleGenerationOption() {
            CodeGenerationToggleVisualElement codeGenerationToggleVisualElement = new();
            return codeGenerationToggleVisualElement;
        }

        private CodeGenerationSubToggleVisualElement CreateNewToggleSubGenerationOption() {
            CodeGenerationSubToggleVisualElement codeGenerationToggleVisualElement = new();
            return codeGenerationToggleVisualElement;
        }

        private CodeGenerationLabelWithButtonVisualElement CreateNewLabelGenerationOption() {
            CodeGenerationLabelWithButtonVisualElement codeGenerationToggleWithButtonVisualElement = new();
            return codeGenerationToggleWithButtonVisualElement;
        }
    }
}