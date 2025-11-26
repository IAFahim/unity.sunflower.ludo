#if UNITY_IOS || UNITY_TVOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace ElProfesorKudo.Firebase.Editor.Build
{
    using ElProfesorKudo.Firebase.FirebaseIntegration;
    using ElProfesorKudo.Firebase.Common;

    public class UnityIOSPostProcessBuild
    {
        // Execution order (999) is important to ensure this script runs after others
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            Debug.Log("Executing iOS PostProcessBuild for GIDClientID and Firebase Init...");

            // Retrieve Android IOS Firebase BuildSettings
            Android_IOS_Firebase_BuildSettings settings = GetAndroidIOSFirebaseBuildSettings();
            if (settings == null)
            {
                Debug.LogError("Error: Android_IOS_Firebase_BuildSettings.asset not found! Please create it via Assets > Create > Build Settings > iOS Build Settings and configure the GID Client ID.");
                return; // Stop execution if settings are not found
            }
            // --- Execute conditional actions for better readability ---

            Utils.ExecuteConditionalAction(settings.AllowUpdateInfoPlist,
                                     () => HandleInfoPlistModifications(pathToBuiltProject, settings.GIDClientID),
                                     "Info.plist modifications skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.AllowInjectFirebaseInit,
                                     () => HandleUnityAppControllerModifications(pathToBuiltProject),
                                     "Firebase App Controller initialization skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.AllowOverridePodfile,
                                     () => HandlePodfileModifications(pathToBuiltProject),
                                     "Podfile override skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.AllowApplyXcodeProjectSettings,
                                     () => HandleXcodeProjectSettings(pathToBuiltProject),
                                     "Xcode project settings modifications skipped as per settings.");

            bool scriptGenerated = false;
            Utils.ExecuteConditionalAction(settings.AllowGenerateShellScript,
                                    () =>
                                    {
                                        GeneratePodInstallScript(pathToBuiltProject);
                                        scriptGenerated = true;
                                    },
                                    "Shell script generation skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.AllowAutoRunShellScript,
                                           () =>
                                           {
                                               if (scriptGenerated)
                                               {
                                                   ExecutePodInstallScript(pathToBuiltProject);
                                               }
                                               else
                                               {
                                                   Debug.LogWarning("Cannot run Shell script automatically: 'Generate Shell Script' is not enabled, so no script was generated to run.");
                                               }
                                           },
                                           "Automatic Shell execution script skipped as per settings.");


            Utils.ExecuteConditionalAction(settings.AllowUpdateSwiftWebClientID,
                                     () => HandleGoogleSignInBridgeModificationsWebClientID(pathToBuiltProject, settings.WebClientID, "GoogleSignInBridge.swift"),
                                     "GoogleSignInBridge.swift Web Client ID modification skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.AllowUpdateGameObjectReceiverName,
                                     () => HandleSwiftSignInBridgeModificationsGameObjectReceiverName(pathToBuiltProject, settings.GameObjectReceiverName, "GoogleSignInBridge.swift"),
                                     "GoogleSignInBridge.swift GameObjectReceiverName modification skipped as per settings.");
            Utils.ExecuteConditionalAction(settings.AllowUpdateGameObjectReceiverName,
                                    () => HandleSwiftSignInBridgeModificationsGameObjectReceiverName(pathToBuiltProject, settings.GameObjectReceiverName, "AppleSignInBridge.swift"),
                                    "AppleSignInBridge.swift GameObjectReceiverName modification skipped as per settings.");

            Utils.ExecuteConditionalAction(settings.UseCustomIOSNotificationSound,
                                    () => CopyAlertSoundToXcode(pathToBuiltProject, settings.IOSNotificationSoundName),
                                    "Copy alert.wav skipped.");


            Debug.Log("PostProcessBuild iOS completed.");
        }


        private static Android_IOS_Firebase_BuildSettings GetAndroidIOSFirebaseBuildSettings()
        {
            // Search for the ScriptableObject by its type
            string[] guids = AssetDatabase.FindAssets("t:Android_IOS_Firebase_BuildSettings");

            if (guids.Length == 0)
            {
                return null;
            }
            else if (guids.Length > 1)
            {
                // Warning message if multiple instances are found
                string warningMessage = "Warning: Multiple 'Android_IOS_Firebase_BuildSettings' instances found in the project. Only one will be used.\n";
                warningMessage += "Please ensure you have only one 'Android_IOS_Firebase_BuildSettings.asset' file.\n";
                warningMessage += "Files found :\n";
                foreach (string guid in guids)
                {
                    warningMessage += "- " + AssetDatabase.GUIDToAssetPath(guid) + "\n";
                }
                Debug.LogWarning(warningMessage);
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Android_IOS_Firebase_BuildSettings>(path);
        }


        private static void HandleGoogleSignInBridgeModificationsWebClientID(string pathToBuiltProject, string webClientID, string swiftClassName)
        {
            string swiftBridgePath = Path.Combine(pathToBuiltProject, swiftClassName);


            if (!File.Exists(swiftBridgePath))
            {
                string[] foundFiles = Directory.GetFiles(pathToBuiltProject, swiftClassName, SearchOption.AllDirectories);
                if (foundFiles.Length > 0)
                {
                    swiftBridgePath = foundFiles[0]; // Prend le premier trouvé
                    Debug.Log(swiftClassName + " found at: " + swiftBridgePath);
                }
                else
                {
                    Debug.LogError(swiftClassName + " not found at expected path: " + swiftBridgePath + " or in subdirectories. Cannot update Web Client ID.");
                    return;
                }
            }

            string content = File.ReadAllText(swiftBridgePath);

            // Regex to find the line with kWebClientID and capture the existing ID.
            // This allows replacing the value regardless of its current format.
            // Using a verbatim string literal with double quotes for internal quotes.
            string pattern = @"private static let kWebClientID = ""([^""]*)""";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            if (regex.IsMatch(content))
            {
                string newContent = regex.Replace(content, "private static let kWebClientID = \"" + webClientID + "\"");
                File.WriteAllText(swiftBridgePath, newContent);
                Debug.Log(swiftClassName + " updated with Web Client ID: " + webClientID);
            }
            else
            {
                Debug.LogError("Could not find 'kWebClientID' definition in " + swiftClassName + ". Please ensure the line 'private static let kWebClientID = \"...\"' exists in the Swift file.");
            }
        }

        private static void HandleSwiftSignInBridgeModificationsGameObjectReceiverName(string pathToBuiltProject, string gameObjectReceiverName, string swiftClassName)
        {
            string swiftBridgePath = Path.Combine(pathToBuiltProject, swiftClassName);

            if (!File.Exists(swiftBridgePath))
            {
                string[] foundFiles = Directory.GetFiles(pathToBuiltProject, swiftClassName, SearchOption.AllDirectories);
                if (foundFiles.Length > 0)
                {
                    swiftBridgePath = foundFiles[0];
                    Debug.Log(swiftClassName + " found at: " + swiftBridgePath);
                }
                else
                {
                    Debug.LogError(swiftClassName + " not found at expected path: " + swiftBridgePath + " or in subdirectories. Cannot update GameObject Receiver Name.");
                    return;
                }
            }

            string content = File.ReadAllText(swiftBridgePath);

            // Regex to find the line with gameObjectReceiverName and capture the existing name.
            // The pattern now looks for the specific variable name.
            // It allows for any characters (including empty) between the quotes.
            string pattern = @"private static let gameObjectReceiverName = ""([^""]*)""";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            if (regex.IsMatch(content))
            {
                string newContent = regex.Replace(content, "private static let gameObjectReceiverName = \"" + gameObjectReceiverName + "\"");
                File.WriteAllText(swiftBridgePath, newContent);
                Debug.Log(swiftClassName + " updated with GameObject Receiver Name: " + gameObjectReceiverName);
            }
            else
            {
                Debug.LogError("Could not find 'gameObjectReceiverName' definition in " + swiftClassName + ". Please ensure the line 'private static let gameObjectReceiverName = \"...\"' exists in the Swift file.");
            }
        }

        private static void HandleInfoPlistModifications(string pathToBuiltProject, string gidClientID)
        {
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;

            // Add the GIDClientID key if it doesn't already exist
            const string GIDClientIDKey = "GIDClientID";
            // Use the value passed as a parameter from the ScriptableObject
            string GIDClientIDValue = gidClientID;

            if (!rootDict.values.ContainsKey(GIDClientIDKey))
            {
                rootDict.SetString(GIDClientIDKey, GIDClientIDValue);
                Debug.Log("GIDClientID added to Info.plist: " + GIDClientIDValue);
            }
            else
            {
                Debug.Log("GIDClientID already present in Info.plist. No modification.");
            }

            plist.WriteToFile(plistPath);
        }

        private static void HandleUnityAppControllerModifications(string pathToBuiltProject)
        {
            string unityAppControllerPath = Path.Combine(pathToBuiltProject, "Classes/UnityAppController.mm");

            if (File.Exists(unityAppControllerPath))
            {
                string content = File.ReadAllText(unityAppControllerPath);

                // 1. Add FirebaseCore/FIRApp.h import
                const string importToFind = "#import \"UnityAppController.h\"";
                const string firebaseImport = "#import <FirebaseCore/FIRApp.h>";

                if (!content.Contains(firebaseImport))
                {
                    // Insert the new import after the existing import
                    content = content.Replace(importToFind, importToFind + "\n" + firebaseImport);
                    Debug.Log("Import '" + firebaseImport + "' added to UnityAppController.mm.");
                }
                else
                {
                    Debug.Log("Import '" + firebaseImport + "' already present in UnityAppController.mm. No modification.");
                }

                // 2. Insert [FIRApp configure]; into application:didFinishLaunchingWithOptions:
                const string methodSignature = "- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions";
                // Indentation is important for readability in the generated file
                const string configureCall = "    [FIRApp configure];";

                // Search for the method signature
                int methodSignatureIndex = content.IndexOf(methodSignature);
                if (methodSignatureIndex != -1)
                {
                    // Search for the opening brace of the method after its signature
                    int braceIndex = content.IndexOf("{", methodSignatureIndex + methodSignature.Length);
                    if (braceIndex != -1)
                    {
                        // Insert [FIRApp configure]; right after the opening brace, if not already there
                        if (!content.Contains(configureCall))
                        {
                            // +1 to insert after the brace
                            content = content.Insert(braceIndex + 1, "\n" + configureCall);
                            Debug.Log("'" + configureCall.Trim() + "' added to UnityAppController.mm.");
                        }
                        else
                        {
                            Debug.Log("'" + configureCall.Trim() + "' already present in UnityAppController.mm. No modification.");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Method '" + methodSignature + "' not found in UnityAppController.mm. Cannot add FirebaseApp.configure().");
                }

                File.WriteAllText(unityAppControllerPath, content);
            }
            else
            {
                Debug.LogError("UnityAppController.mm not found at path: " + unityAppControllerPath);
            }
        }

        private static void HandlePodfileModifications(string pathToBuiltProject)
        {
            string podfilePath = Path.Combine(pathToBuiltProject, "Podfile");
            TextAsset podScriptContent = Resources.Load<TextAsset>("PodScript");
            string podFileContent = "";
            if (podScriptContent != null)
            {
                podFileContent = podScriptContent.text;
                Debug.Log("PodFile found in Resources folder");
            }
            else
            {
                Debug.LogError("PodFiles.txt didn't find in Resources folder");
            }

            string customPodfileContent = podFileContent;
            File.WriteAllText(podfilePath, customPodfileContent.Trim());
            Debug.Log("Podfile generated/updated with custom content.");
        }

        private static void HandleXcodeProjectSettings(string pathToBuiltProject)
        {
            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string unityFrameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();

            if (!string.IsNullOrEmpty(unityFrameworkTargetGuid))
            {
                // Paths to add to Header Search Paths, now with recursive markers (/**)
                string path1 = "$(BUILT_PRODUCTS_DIR)/UnityFramework.framework/Headers/**";
                string path2 = "$(BUILD_DIR)/Intermediates.noindex/Unity-iPhone.build/$(CONFIGURATION)$(EFFECTIVE_PLATFORM_NAME)/UnityFramework.build/Objects-normal/$(ARCHS)/**";

                // Get all build configurations for UnityFramework target
                //var configs = proj.BuildConfigGuidsForTarget(unityFrameworkTargetGuid);
                string[] buildConfigNames = new[] { "Debug", "Release", "ReleaseForProfiling", "ReleaseForRunning" };
                List<string> configs = buildConfigNames
                    .Select(name => proj.BuildConfigByName(unityFrameworkTargetGuid, name))
                    .Where(guid => !string.IsNullOrEmpty(guid))
                    .ToList();
                foreach (string configGuid in configs)
                {
                    // Get current HEADER_SEARCH_PATHS for this configuration
                    string currentHeaderSearchPaths = proj.GetBuildPropertyForConfig(configGuid, "HEADER_SEARCH_PATHS");
                    List<string> pathsToSet = new List<string>();

                    // Add $(inherited) first if not present, to ensure CocoaPods paths are included
                    if (!string.IsNullOrEmpty(currentHeaderSearchPaths) && currentHeaderSearchPaths.Contains("$(inherited)"))
                    {
                        pathsToSet.Add("$(inherited)");
                        // Add existing paths, excluding $(inherited) if it was already processed
                        pathsToSet.AddRange(currentHeaderSearchPaths.Split(' ').Where(p => p != "$(inherited)").Select(p => p.Trim('"')));
                    }
                    else if (!string.IsNullOrEmpty(currentHeaderSearchPaths))
                    {
                        // If $(inherited) was not explicitly found but other paths exist, add them
                        pathsToSet.AddRange(currentHeaderSearchPaths.Split(' ').Select(p => p.Trim('"')));
                    }
                    else
                    {
                        // If no paths exist, ensure $(inherited) is added
                        pathsToSet.Add("$(inherited)");
                    }

                    bool changed = false;

                    // Add path1 if not already in the list
                    if (!pathsToSet.Contains(path1))
                    {
                        pathsToSet.Add(path1);
                        changed = true;
                    }

                    // Add path2 if not already in the list
                    if (!pathsToSet.Contains(path2))
                    {
                        pathsToSet.Add(path2);
                        changed = true;
                    }

                    if (changed)
                    {
                        // Reconstruct the string, quoting paths that contain spaces or variables
                        string newHeaderSearchPaths = string.Join(" ", pathsToSet.Select(p => p.Contains(" ") || p.Contains("$") ? $"\"{p}\"" : p));
                        proj.SetBuildPropertyForConfig(configGuid, "HEADER_SEARCH_PATHS", newHeaderSearchPaths);
                        Debug.Log("Header Search Paths updated for config " + configGuid + " in UnityFramework target.");

                    }
                }
                proj.WriteToFile(projPath);
            }
            else
            {
                Debug.LogError("UnityFramework target not found in Xcode project. Cannot modify Header Search Paths.");
            }
        }

        private static void GeneratePodInstallScript(string pathToBuiltProject)
        {
            string scriptFileName = "run_pods_install.sh";
            string scriptPath = Path.Combine(pathToBuiltProject, scriptFileName);

            TextAsset shellScriptFile = Resources.Load<TextAsset>("ShellScript");
            string shellScriptContent = "";
            if (shellScriptFile != null)
            {
                shellScriptContent = shellScriptFile.text;
                Debug.Log("Shell script found in Resources folder");
            }
            else
            {
                Debug.LogError("ShellScript.txt didn't find in Resources folder");
            }
            // Shell script content, with dynamic build path
            string scriptContent = shellScriptContent;
            File.WriteAllText(scriptPath, scriptContent);

            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("/bin/bash", "-c \"chmod +x \\\"" + scriptPath + "\\\"\"");
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                if (proc.ExitCode == 0)
                {
                    Debug.Log("Script d'installation des pods '" + scriptFileName + "' generated and made executable in the Xcode build folder.");
                }
                else
                {
                    Debug.LogError("Error making script '" + scriptFileName + "' executable:\n" + error);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception when executing chmod for the script: " + e.Message);
            }
        }

        private static void ExecutePodInstallScript(string pathToBuiltProject)
        {
            string scriptFileName = "run_pods_install.sh";
            string scriptPath = Path.Combine(pathToBuiltProject, scriptFileName);

            if (!File.Exists(scriptPath))
            {
                Debug.LogError("Cannot execute pod install script: '" + scriptFileName + "' not found at " + scriptPath + ". Ensure 'Generate Pod Install Shell Script' is enabled and the script was successfully generated.");
                return;
            }

            Debug.Log("Attempting to automatically run pod install script: " + scriptPath);

            try
            {
                // Start a new process to execute the shell script
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("open", "-a Terminal.app \"" + scriptPath + "\"");

                procStartInfo.UseShellExecute = true; // Use shell execute for 'open' command
                procStartInfo.CreateNoWindow = false; // Allow Terminal window to be created

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                Debug.Log("Pod install script launched successfully. Check a new Terminal window for output.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception when attempting to run pod install script: " + e.Message);
            }
        }

        private static void CopyAlertSoundToXcode(string pathToBuiltProject, string soundNameWithExtension)
        {
            string sourcePath = Path.Combine(Application.dataPath, "ElProfesorKudo_Firebase_Asset/Resources", soundNameWithExtension);
            if (!File.Exists(sourcePath))
            {
                Debug.LogError("Source sound file not found: " + sourcePath);
                return;
            }

            string destFolder = Path.Combine(pathToBuiltProject, "Unity-iPhone/Sound");
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string destPath = Path.Combine(destFolder, soundNameWithExtension);
            File.Copy(sourcePath, destPath, true);
            Debug.Log("Copied " + soundNameWithExtension + " to Xcode folder: " + destPath);

            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string targetGuid = proj.GetUnityMainTargetGuid();

            string fileGuid = proj.AddFile(destPath, "Sound/" + soundNameWithExtension);
            proj.AddFileToBuild(targetGuid, fileGuid);

            proj.WriteToFile(projPath);
            Debug.Log(soundNameWithExtension + " added to Xcode project build.");
        }
    }
}
#endif
