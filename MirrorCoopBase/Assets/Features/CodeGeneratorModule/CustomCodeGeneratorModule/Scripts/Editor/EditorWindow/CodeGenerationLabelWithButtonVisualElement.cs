using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.EditorWindow {
    public class CodeGenerationLabelWithButtonVisualElement : VisualElement {
        private List<CodeGenerationSubToggleVisualElement> _child;
        private bool _isToogledAll;
        private const string TOGGLE_CLEAR_SAVES_TOGGLE = "LabelWithButtonGenerateCode";

        #region Boilerplate for Unity

        public new class UxmlFactory : UxmlFactory<CodeGenerationToggleVisualElement> { }

        public CodeGenerationLabelWithButtonVisualElement() { }

        #endregion

        private Label Label => this.Q<Label>("label");
        private Button Button => this.Q<Button>("button");

        public CodeGenerationLabelWithButtonVisualElement(SerializedObject property, string label) =>
            Init(property, label);

        public void Init(SerializedObject property, string label) {
            VisualTreeAsset asset = CodeGenerationTools.GetVisualTreeAsset(TOGGLE_CLEAR_SAVES_TOGGLE);

            asset.CloneTree(this);

            Label.text = label;
            Label.Bind(property);
            Button.Bind(property);
        }

        public void InitializeChild(List<CodeGenerationSubToggleVisualElement> child) {
            _child = child;
            Button.RegisterCallback<ClickEvent>(OnButtonClicked);
        }

        private void OnButtonClicked(ClickEvent clickEvent) {
            _isToogledAll = !_isToogledAll;
            foreach (CodeGenerationSubToggleVisualElement codeGenerationSubToggleVisualElement in _child)
                codeGenerationSubToggleVisualElement.IsToggled = _isToogledAll;
        }

        public void Init(string label) {
            VisualTreeAsset asset = CodeGenerationTools.GetVisualTreeAsset(TOGGLE_CLEAR_SAVES_TOGGLE);
        
            asset.CloneTree(this);

            Label.text = label;
        }
    }
}