
using UnityEngine;

namespace ElProfesorKudo.Firebase.FirebaseIntegration
{
    [CreateAssetMenu(fileName = "Android_IOS_Firebase_BuildSettings", menuName = "ElProfesorKudo/Build Settings/Firebase Build Settings")]
    public class Android_IOS_Firebase_BuildSettings : ScriptableObject
    {
        [Header("===============================COMMON===============================")]
        // COMMUN - iOS & ANDROID
        [Header("Common Settings")]

        [Tooltip("Name of the GameObject in your Unity scene that will receive native callbacks (e.g., from Swift or Java).")]
        public string GameObjectReceiverName = "GameObjectReceiver";

        [Tooltip("If enabled, the GameObjectReceiverName will be injected into native bridge files (e.g., GoogleSignInBridge.swift).")]
        public bool AllowUpdateGameObjectReceiverName = true;

        [Tooltip("If enabled, the client ID fields will be automatically trimmed of leading/trailing whitespace before use.")]
        public bool AllowAutoTrimClientIDs = true;

        [Space(10)]
        [Header("===============================IOS=================================")]
        // iOS - Google Sign-In
        [Header("iOS - Google Sign-In")]

        [Tooltip("Your iOS Web Client ID from Firebase/Google Cloud Console. Used for Google Sign-In (Firebase Auth).")]
        public string WebClientID = "";

        [Tooltip("If enabled, WebClientID will be injected into GoogleSignInBridge.swift.")]
        public bool AllowUpdateSwiftWebClientID = true;

        [Tooltip("iOS-specific GID Client ID from Firebase. Required for Google Sign-In configuration in Info.plist.")]
        public string GIDClientID = "";

        [Tooltip("If enabled, GIDClientID and URL schemes will be inserted into Info.plist.")]
        public bool AllowUpdateInfoPlist = true;


        // iOS - Firebase & Build Integration
        [Header("iOS - Firebase & Xcode Build")]

        [Tooltip("If enabled, FIRApp.configure() will be injected into UnityAppController.mm to initialize Firebase at runtime.")]
        public bool AllowInjectFirebaseInit = true;

        [Tooltip("If enabled, the Podfile will be overwritten with a custom one generated from this plugin.")]
        public bool AllowOverridePodfile = true;

        [Tooltip("If enabled, specific Xcode project settings (e.g., Swift bridging paths) will be added automatically.")]
        public bool AllowApplyXcodeProjectSettings = true;

        [Tooltip("If enabled, generates a 'run_pods_install.sh' script inside the Xcode project to help with pod installation.")]
        public bool AllowGenerateShellScript = true;

        [Tooltip("If enabled, automatically runs the generated pod install script after Unity finishes building the iOS project.")]
        public bool AllowAutoRunShellScript = false;

        // iOS - Push Notification
        [Header("iOS - Push Notification")]
        [Tooltip("Name of the sound file (with extension) located resources to be used for custom notifications.")]
        public string IOSNotificationSoundName = "alert.wav";
        [Tooltip("Enable to use a custom notification sound for iOS notifications.")]
        public bool UseCustomIOSNotificationSound = true;

        [Space(10)]
        [Header("===============================ANDROID===============================")]

        // ANDROID - Manifest
        [Header("Android - Manifest")]

        [Tooltip("If enabled, the Android Data will be injected into AndroidManifest.xml as <meta-data>.")]
        public bool AllowInjectManifestData = true;

        // ANDROID - Google Sign-In
        [Header("Android - Google Sign-In")]
        [Tooltip("If enabled, the Android Data will be injected into AndroidManifest.xml for Android")]
        public bool AllowInjectManifestDataAndroid = true;

        [Tooltip("Your Android Web Client ID from Firebase/Google Cloud Console. Used for Google Sign-In.")]
        public string AndroidWebClientID = "";

        // Android - Apple Sign-In
        [Header("Android - Apple Sign-In")]
        [Tooltip("If enabled, the Android Data will be injected into AndroidManifest.xml for iOS")]
        public bool AllowInjectManifestDataIos = true;
        [Tooltip("Your Apple redirect URI (e.g. hosted on Firebase). Used to receive the authorization code from Apple after sign-in.")]
        public string AndroidAppleRedirectUri = "";

        [Tooltip("Host part of the redirect URI, e.g. myapp.firebaseapp.com")]
        public string AndroidRedirectHost = "";

        [Tooltip("Path part of the redirect URI, e.g. /redirect.html")]
        public string AndroidRedirectPath = "/redirect.html";

        [Tooltip("Custom scheme used for deep linking after OAuth redirect.")]
        public string AndroidCustomScheme = "unitydl";

        [Tooltip("Custom host for the deep link scheme.")]
        public string AndroidCustomHost = "";

        [Header("Android - Apple Sign-In - Configuration Sensitive Info")]
        [Header("READ ME BEFORE PUSH TO PRODUCTION !!!!!!!!!")]
        [Multiline(12)] public string ReadMe;

        [Tooltip("The Client ID registered for your app in Apple Developer Console, used for Apple Sign-In.")]
        public string AppleClientID = "";

        [Tooltip("The URL of the Cloud Function endpoint to exchange the Apple authorization code for tokens.")]
        public string AppleCloudFunctionUrl = "";


        // Android - Push Notification
        [Header("Android - Push Notification")]
        [Tooltip("Name of the sound file (without extension) located in res/raw/ to be used for custom notifications.")]
        public string AndroidNotificationSoundName = "alert";

        private void OnValidate()
        {
            if (AllowAutoTrimClientIDs) // Check if the trimming option is enabled
            {
                GIDClientID = CleanWhitespace(GIDClientID, nameof(GIDClientID));
                WebClientID = CleanWhitespace(WebClientID, nameof(WebClientID));
                AndroidWebClientID = CleanWhitespace(AndroidWebClientID, nameof(AndroidWebClientID));
                AndroidAppleRedirectUri = CleanWhitespace(AndroidAppleRedirectUri, nameof(AndroidAppleRedirectUri));
                AndroidRedirectHost = CleanWhitespace(AndroidRedirectHost, nameof(AndroidRedirectHost));
                AndroidRedirectPath = CleanWhitespace(AndroidRedirectPath, nameof(AndroidRedirectPath));
                AndroidCustomScheme = CleanWhitespace(AndroidCustomScheme, nameof(AndroidCustomScheme));
                AndroidCustomHost = CleanWhitespace(AndroidCustomHost, nameof(AndroidCustomHost));
                AppleClientID = CleanWhitespace(AppleClientID, nameof(AppleClientID));
                AppleCloudFunctionUrl = CleanWhitespace(AppleCloudFunctionUrl, nameof(AppleCloudFunctionUrl));
                SetReadMeText();
            }

        }

        private static string CleanWhitespace(string input, string fieldName)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string cleaned = input.Replace(" ", "");

            if (input != cleaned)
            {
                Debug.LogWarning(fieldName + " has been cleaned to remove all whitespace. Original: '" + input + "' -> Cleaned: '" + cleaned + "'");
            }

            return cleaned;
        }

        private void SetReadMeText()
        {
            ReadMe =
@"////////////       IMPORTANT         \\\\\\\\\\\\\\\\\
- Hardcoding the Client ID and Cloud Function URL in AndroidManifest.xml 
is NOT recommended, as these sensitive values can be exposed publicly.
- This is for TESTING ONLY.
- I’m not responsible for any leaks or misuse.
- For better security, fetch these values from a backend service at runtime.
- If a backend isn’t possible, use the SensitiveValueSet() method 
in FirebaseAppleSignInAndroid to set them in memory during execution,
or hardcoded it in to the aar file to minimizing exposure.
- Follow these guidelines to keep your app and users safe.";
        }
    }
}