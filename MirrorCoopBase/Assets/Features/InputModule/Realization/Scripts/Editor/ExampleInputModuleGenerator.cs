using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core;
using IncrementalSourceGenerator.Utils;
using UnityEditor;
using UnityEngine.InputSystem;
using ICodeGenerator = Features.CodeGeneratorModule.CustomCodeGeneratorModule.Scripts.Editor.Core.ICodeGenerator;
using Tools = IncrementalSourceGenerator.Utils.Tools;

namespace Features.InputModule.Realization.Scripts.Editor {
    [Generator]
    public class ExampleInputModuleGenerator : ICodeGenerator {
        private readonly string _namespace = GENERATED_FILES_PATH.Replace("Assets/", "").Replace("/", ".");
        private const string INPUT_ACTIONS_NAME = "InputActions";
        private const string INPUT_SERVICE = "InputService";
        private const string I_INPUT_SERVICE = "IInputService";
        private const string GENERATED_FILES_PATH = "Assets/Features/InputModule/Realization/Scripts/Generated";

        public void Execute(GeneratorContext context) {
            context.OverrideFolderPath(GENERATED_FILES_PATH);
            context.AddCode(Tools.BuildFileName(I_INPUT_SERVICE), Tools.GetSourceText(InterfaceInputSystemSource));
            context.AddCode(Tools.BuildFileName(INPUT_SERVICE), Tools.GetSourceText(InputSystemSource));
        }

        private string InterfaceInputSystemSource(StringBuilder builder, StringWriter writer, IndentedTextWriter text) {
            InputActionAsset inputActionAsset = GetInputActionAsset(INPUT_ACTIONS_NAME);
            if (inputActionAsset == null)
                return string.Empty;

            List<string> mapNames = inputActionAsset.actionMaps.Select(m => m.name.Replace(" ", "")).ToList();
            List<InputAction> actions = new();
            foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
                actions.AddRange(actionMap.actions);

            text.AddUsing("Features.InputModule.Core.Scripts");

            text.AddNamespace(_namespace);
            {
                text.OpenBlock();
                text.Write("public interface " + I_INPUT_SERVICE);
                {
                    text.OpenBlock();
                    foreach (InputAction action in actions)
                        if (action.expectedControlType == "Vector2")
                            AddPublicVariable(text, "InputVector2Actions", action.name.Replace(" ", ""), true);
                        else
                            AddPublicVariable(text, "InputDefaultActions", action.name.Replace(" ", ""), true);

                    AddEnable(text, INPUT_ACTIONS_NAME, true);
                    AddDisable(text, INPUT_ACTIONS_NAME, true);

                    foreach (string mapName in mapNames) {
                        AddEnableMap(text, INPUT_ACTIONS_NAME, mapName, true);
                        AddDisableMap(text, INPUT_ACTIONS_NAME, mapName, true);
                    }

                    text.CloseBlock();
                }

                text.CloseBlock();
            }

            return writer.ToString();
        }

        private string InputSystemSource(StringBuilder builder, StringWriter writer, IndentedTextWriter text) {
            InputActionAsset inputActionAsset = GetInputActionAsset(INPUT_ACTIONS_NAME);
            if (inputActionAsset == null)
                return string.Empty;

            List<string> mapNames = inputActionAsset.actionMaps.Select(m => m.name.Replace(" ", "")).ToList();
            List<InputAction> actions = new();
            List<string> actionsNames = new();
            foreach (InputActionMap actionMap in inputActionAsset.actionMaps) {
                actions.AddRange(actionMap.actions);
                actionsNames.AddRange(actionMap.actions.Select(a => a.name.Replace(" ", "")));
            }

            text.AddUsing("UnityEngine");
            text.AddUsing("UnityEngine.InputSystem");
            text.AddUsing("Features.InputModule.Core.Scripts");
            text.AddUsing("Zenject");
            text.AddUsing("System");

            text.AddNamespace(_namespace);
            {
                text.OpenBlock();
                text.AddPublicClass(INPUT_SERVICE);
                AddInterfaces(text, mapNames);
                {
                    text.OpenBlock();

                    AddPrivateVariable(text, INPUT_ACTIONS_NAME, INPUT_ACTIONS_NAME);

                    foreach (InputAction action in actions)
                        if (action.expectedControlType == "Vector2")
                            AddPublicVariable(text, "InputVector2Actions", action.name.Replace(" ", ""));
                        else
                            AddPublicVariable(text, "InputDefaultActions", action.name.Replace(" ", ""));

                    AddConstructor(text, INPUT_SERVICE, INPUT_ACTIONS_NAME);
                    
                    AddInitialize(text);
                    AddDispose(text);

                    AddEnable(text, INPUT_ACTIONS_NAME);
                    AddDisable(text, INPUT_ACTIONS_NAME);

                    AddSetControlsCallback(text, INPUT_ACTIONS_NAME, mapNames);
                    AddRemoveControlsCallback(text, INPUT_ACTIONS_NAME, mapNames);

                    foreach (string mapName in mapNames) {
                        AddEnableMap(text, INPUT_ACTIONS_NAME, mapName);
                        AddDisableMap(text, INPUT_ACTIONS_NAME, mapName);
                    }

                    foreach (InputAction action in actions)
                        AddOnMethod(text, action);

                    text.CloseBlock();
                }

                text.CloseBlock();
            }

            return writer.ToString();
        }

        private void AddPrivateVariable(IndentedTextWriter text, string variableType, string variableName) {
            string muffinInputActionsName = variableName.Remove(0, 1).Insert(0, variableName[0].ToString().ToLower());
            text.WriteLine($"private {variableType} _{muffinInputActionsName};");
        }

        private void AddPublicVariable(IndentedTextWriter text, string variableType, string variableName, bool isInterface = false) {
            if (isInterface)
                text.WriteLine($"public {variableType} {variableName} {{ get; set; }}");
            else
                text.WriteLine($"public {variableType} {variableName} {{ get; set; }} = new();");
        }

        private void AddConstructor(IndentedTextWriter text, string className, string variableClassName) {
            text.Write($"public {className}");
            text.AddOpenRoundBrace();
            string muffinInputActionsName = variableClassName;
            muffinInputActionsName = muffinInputActionsName.Remove(0, 1).Insert(0, muffinInputActionsName[0].ToString().ToLower());
            text.Write($"{variableClassName} {muffinInputActionsName}");
            text.AddCloseRoundBrace();
            text.OpenBlock();
            text.WriteLine($"_{muffinInputActionsName} = {muffinInputActionsName};");
            text.CloseBlock();
        }
        
        private void AddInitialize(IndentedTextWriter text) {
            text.WriteLine("public void Initialize() =>");
            text.WriteLine("    Enable();");
        }

        private void AddDispose(IndentedTextWriter text) {
            text.WriteLine("public void Dispose() =>");
            text.WriteLine("    Disable();");
        }

        private void AddInterfaces(IndentedTextWriter text, List<string> allMapNames) {
            string allInterfaces = string.Empty;
            foreach (string mapAction in allMapNames)
                allInterfaces += INPUT_ACTIONS_NAME + ".I" + mapAction + "Actions, ";

            allInterfaces += I_INPUT_SERVICE + ", IInitializable, IDisposable";
            text.AddInherited(allInterfaces);
        }

        private InputActionAsset GetInputActionAsset(string inputActionsName) {
            string[] findAssets = AssetDatabase.FindAssets(inputActionsName);
            return findAssets.Select(GetAssetByID).First(assetByID => assetByID != null);
        }

        private InputActionAsset GetAssetByID(string assetsGUID) {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetsGUID);
            InputActionAsset loadedAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            return loadedAsset;
        }

        private void AddEnable(IndentedTextWriter text, string inputActionsName, bool isInterface = false) {
            if (isInterface)
                text.WriteLine("public void Enable();");
            else {
                text.WriteLine("public void Enable()");
                text.OpenBlock();
                text.WriteLine($"_{inputActionsName.Remove(0, 1).Insert(0, inputActionsName[0].ToString().ToLower())}.Enable();");
                text.WriteLine("SetControlsCallback();");
                text.CloseBlock();
            }
        }

        private void AddDisable(IndentedTextWriter text, string inputActionsName, bool isInterface = false) {
            if (isInterface)
                text.WriteLine("public void Disable();");
            else {
                text.WriteLine("public void Disable()");
                text.OpenBlock();
                text.WriteLine($"_{inputActionsName.Remove(0, 1).Insert(0, inputActionsName[0].ToString().ToLower())}.Disable();");
                text.WriteLine("RemoveControlsCallback();");
                text.CloseBlock();
            }
        }

        private void AddSetControlsCallback(IndentedTextWriter text, string muffinInputActionsName, List<string> allMapNames) {
            text.WriteLine("private void SetControlsCallback()");
            text.OpenBlock();
            foreach (string mapName in allMapNames)
                text.WriteLine(
                    $"_{muffinInputActionsName.Remove(0, 1).Insert(0, muffinInputActionsName[0].ToString().ToLower())}.{mapName}.SetCallbacks(this);");

            text.CloseBlock();
        }

        private void AddRemoveControlsCallback(IndentedTextWriter text, string muffinInputActionsName, List<string> allMapNames) {
            text.WriteLine("private void RemoveControlsCallback()");
            text.OpenBlock();
            foreach (string mapName in allMapNames)
                text.WriteLine(
                    $"_{muffinInputActionsName.Remove(0, 1).Insert(0, muffinInputActionsName[0].ToString().ToLower())}.{mapName}.RemoveCallbacks(this);");

            text.CloseBlock();
        }

        private void AddEnableMap(IndentedTextWriter text, string inputActionsName, string mapName, bool isInterface = false) {
            if (isInterface)
                text.WriteLine($"public void Enable{mapName}();");
            else {
                text.WriteLine($"public void Enable{mapName}()");
                text.OpenBlock();
                string inputActionsNameVariable = inputActionsName.Remove(0, 1).Insert(0, inputActionsName[0].ToString().ToLower());
                text.WriteLine($"_{inputActionsNameVariable}.{mapName}.Enable();");
                text.WriteLine($"_{inputActionsNameVariable}.{mapName}.SetCallbacks(this);");
                text.CloseBlock();
            }
        }

        private void AddDisableMap(IndentedTextWriter text, string inputActionsName, string mapName, bool isInterface = false) {
            if (isInterface)
                text.WriteLine($"public void Disable{mapName}();");
            else {
                text.WriteLine($"public void Disable{mapName}()");
                text.OpenBlock();
                string inputActionsNameVariable = inputActionsName.Remove(0, 1).Insert(0, inputActionsName[0].ToString().ToLower());
                text.WriteLine($"_{inputActionsNameVariable}.{mapName}.Disable();");
                text.WriteLine($"_{inputActionsNameVariable}.{mapName}.RemoveCallbacks(this);");
                text.CloseBlock();
            }
        }

        private void AddOnMethod(IndentedTextWriter text, InputAction action) {
            string actionName = action.name.Replace(" ", "");
            text.WriteLine("public void On" + actionName + "(InputAction.CallbackContext context)");
            text.OpenBlock();

            text.AddIfWithBrace();
            text.Write("context.started");
            text.AddCloseRoundBrace();
            text.WriteLine(actionName + ".Started?.Invoke();");

            text.AddIfWithBrace();
            text.Write("context.performed");
            text.AddCloseRoundBrace();
            text.WriteLine(actionName + ".Performed?.Invoke();");

            text.AddIfWithBrace();
            text.Write("context.canceled");
            text.AddCloseRoundBrace();
            text.WriteLine(actionName + ".Canceled?.Invoke();");

            if (action.expectedControlType == "Vector2") {
                text.AddIfWithBrace();
                text.Write("context.started");
                text.AddCloseRoundBrace();
                text.WriteLine(actionName + ".VectorChangedStarted?.Invoke(context.ReadValue<Vector2>());");

                text.AddIfWithBrace();
                text.Write("context.performed");
                text.AddCloseRoundBrace();
                text.WriteLine(actionName + ".VectorChangedPerformed?.Invoke(context.ReadValue<Vector2>());");

                text.AddIfWithBrace();
                text.Write("context.canceled");
                text.AddCloseRoundBrace();
                text.WriteLine(actionName + ".VectorChangedCanceled?.Invoke(context.ReadValue<Vector2>());");
            }

            text.CloseBlock();
        }
    }
}