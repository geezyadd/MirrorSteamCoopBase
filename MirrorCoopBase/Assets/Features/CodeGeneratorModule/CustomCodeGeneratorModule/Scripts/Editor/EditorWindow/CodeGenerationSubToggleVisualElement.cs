using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.EditorWindow {
    public class CodeGenerationSubToggleVisualElement : VisualElement, IToogleVisualElement {
        private const string TOGGLE_CLEAR_SAVES_TOGGLE = "SubToggleGenerateCode";

        #region Boilerplate for Unity

        public new class UxmlFactory : UxmlFactory<CodeGenerationToggleVisualElement> { }

        public CodeGenerationSubToggleVisualElement() { }

        #endregion

        private Label Label => this.Q<Label>("label");
        private Toggle Toggle => this.Q<Toggle>("toggle");
    
        public event Action<bool> OnToggleChanged;

        public CodeGenerationSubToggleVisualElement(SerializedObject property, string label, bool isOn) =>
            Init(property, label, isOn);

        public bool IsToggled { get => Toggle.value; set => Toggle.value = value; }

        public void Init(SerializedObject property, string label, bool isOn) {
            VisualTreeAsset asset = CodeGenerationTools.GetVisualTreeAsset(TOGGLE_CLEAR_SAVES_TOGGLE);

            asset.CloneTree(this);

            Label.text = label;
            Toggle.value = isOn;
            Label.Bind(property);
            Toggle.Bind(property);
        
            Toggle.RegisterValueChangedCallback(InvokeOnToggledChanged);
        }

        public void Init(string label, bool isOn) {
            VisualTreeAsset asset = CodeGenerationTools.GetVisualTreeAsset(TOGGLE_CLEAR_SAVES_TOGGLE);
        
            asset.CloneTree(this);

            Label.text = label;
            Toggle.value = isOn;
        
            Toggle.RegisterValueChangedCallback(InvokeOnToggledChanged);
        }

        private void InvokeOnToggledChanged(ChangeEvent<bool> evt) =>
            OnToggleChanged?.Invoke(evt.newValue);
    }
}