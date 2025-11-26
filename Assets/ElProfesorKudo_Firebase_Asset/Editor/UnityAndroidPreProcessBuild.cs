#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Xml;
using UnityEngine;
using ElProfesorKudo.Firebase.FirebaseIntegration;

namespace ElProfesorKudo.Firebase.Editor.Build
{
    public class UnityAndroidPreProcessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // META_DATA_KEY
        private const string GOOGLE_CLIENT_ID = "com.google.android.gms.auth.api.signin.clientid";
        private const string GOOGLE_RECEIVER = "com.elprofesorkudo.googlesigninplugin.receiver";
        private const string NOTIFICATION_ALERT_NAME = "com.elprofesorkudo.notificationplugin.sound_notification_name";
        private const string APPLE_RECEIVER = "com.elprofesorkudo.applesigninplugin.receiver";
        private const string APPLE_REDIRECT_URI = "com.elprofesorkudo.applesigninplugin.redirect_uri";
        private const string APPLE_CLIENT_ID = "com.elprofesorkudo.applesigninplugin.client_id";
        private const string APPLE_CLOUD_FUNCTION_URL = "com.elprofesorkudo.applesigninplugin.cloud_function_url";

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            Debug.Log("[UnityAndroidPreProcessBuild] Injecting Web Client ID before build...");

            Android_IOS_Firebase_BuildSettings settings = GetBuildSettings();
            if (settings == null)
            {
                Debug.LogError(" Android_IOS_Firebase_BuildSettings.asset not found. Please create it and assign the Android Web Client ID.");
                return;
            }

            if (!settings.AllowInjectManifestData)
            {
                Debug.Log("Injection skipped: setting disabled in Android_IOS_Firebase_BuildSettings.");
                return;
            }
            string manifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("AndroidManifest.xml not found at: " + manifestPath);
                return;
            }

            InjectAndroidDataIntoManifest(manifestPath, settings.AndroidWebClientID, settings.GameObjectReceiverName, settings.AndroidNotificationSoundName, settings.AndroidAppleRedirectUri, settings.AllowInjectManifestDataAndroid, settings.AllowInjectManifestDataIos, settings.AppleClientID, settings.AppleCloudFunctionUrl);
            if (settings.AllowInjectManifestDataIos)
            {
                InjectAppleIntentFiltersIntoManifest(manifestPath, settings.AndroidRedirectHost, settings.AndroidRedirectPath, settings.AndroidCustomScheme, settings.AndroidCustomHost);
            }

        }

        private static Android_IOS_Firebase_BuildSettings GetBuildSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:Android_IOS_Firebase_BuildSettings");

            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Android_IOS_Firebase_BuildSettings>(path);
        }


        private static void InjectAndroidDataIntoManifest(string manifestPath, string androidWebClientID, string receiverName, string soundNotificationsName, string appleRedirectUri = null, bool allowWriteDataAndroid = true, bool allowWriteApple = true, string appleClientId = "", string appleCloudFunctionUrl = "")
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load(manifestPath);

            XmlNode applicationNode = manifest.SelectSingleNode("/manifest/application");
            if (applicationNode == null)
            {
                Debug.LogError("<application> tag not found in AndroidManifest.xml.");
                return;
            }

            string androidNamespaceUri = manifest.DocumentElement.GetAttribute("xmlns:android");
            if (string.IsNullOrEmpty(androidNamespaceUri))
            {
                androidNamespaceUri = "http://schemas.android.com/apk/res/android";
                manifest.DocumentElement.SetAttribute("xmlns:android", androidNamespaceUri);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(manifest.NameTable);
            nsmgr.AddNamespace("android", androidNamespaceUri);

            // Inject data separately
            if (allowWriteDataAndroid)
            {
                InjectGoogleMetaData(applicationNode, androidNamespaceUri, nsmgr, androidWebClientID, receiverName, soundNotificationsName);
            }
            if (allowWriteApple)
            {
                InjectAppleMetaData(applicationNode, androidNamespaceUri, nsmgr, appleRedirectUri, receiverName, appleClientId, appleCloudFunctionUrl);
            }
            manifest.Save(manifestPath);
            AssetDatabase.Refresh();
            Debug.Log("AndroidManifest updated with Google and Apple metadata.");
        }

        private static void InjectGoogleMetaData(XmlNode applicationNode, string androidNs, XmlNamespaceManager nsmgr, string androidWebClientID, string receiverName, string soundNotificationsName)
        {
            InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                GOOGLE_CLIENT_ID, androidWebClientID);

            InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                GOOGLE_RECEIVER, receiverName);

            InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                NOTIFICATION_ALERT_NAME, soundNotificationsName);
        }

        private static void InjectAppleMetaData(XmlNode applicationNode, string androidNs, XmlNamespaceManager nsmgr, string appleRedirectUri, string receiverName, string appleClientId, string appleCloudFunctionUrl)
        {
            InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                APPLE_RECEIVER, receiverName);

            if (!string.IsNullOrEmpty(appleRedirectUri))
            {
                InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                APPLE_REDIRECT_URI, appleRedirectUri);
            }
            if (!string.IsNullOrEmpty(appleClientId))
            {
                InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                APPLE_CLIENT_ID, appleClientId);
            }
            if (!string.IsNullOrEmpty(appleCloudFunctionUrl))
            {
                InjectOrUpdateMetaData(applicationNode, androidNs, nsmgr,
                APPLE_CLOUD_FUNCTION_URL, appleCloudFunctionUrl);
            }
        }

        private static void InjectOrUpdateMetaData(XmlNode appNode, string ns, XmlNamespaceManager nsmgr, string key, string value)
        {
            XmlNode existing = appNode.SelectSingleNode($"meta-data[@android:name='{key}']", nsmgr);
            if (existing == null)
            {
                XmlElement meta = appNode.OwnerDocument.CreateElement("meta-data");
                meta.SetAttribute("name", ns, key);
                meta.SetAttribute("value", ns, value);
                appNode.AppendChild(meta);
                Debug.Log($"Injected {key}: {value}");
            }
            else
            {
                existing.Attributes["android:value"].Value = value;
                Debug.Log($"Updated {key}: {value}");
            }
        }

        private static void InjectAppleIntentFiltersIntoManifest(string manifestPath, string redirectHost, string redirectPath, string customScheme, string customHost)
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load(manifestPath);

            XmlNode applicationNode = manifest.SelectSingleNode("/manifest/application");
            if (applicationNode == null)
            {
                Debug.LogError("<application> tag not found in AndroidManifest.xml.");
                return;
            }

            string androidNs = "http://schemas.android.com/apk/res/android";

            // Find or create the AppleSignInActivity node
            XmlNode activityNode = null;
            foreach (XmlNode node in applicationNode.ChildNodes)
            {
                if (node.Name == "activity")
                {
                    XmlAttribute nameAttr = node.Attributes?["android:name"];
                    if (nameAttr != null && nameAttr.Value.Contains("AppleSignInActivity"))
                    {
                        activityNode = node;
                        break;
                    }
                }
            }

            if (activityNode == null)
            {
                activityNode = manifest.CreateElement("activity");
                XmlAttribute nameAttr = manifest.CreateAttribute("android", "name", androidNs);
                nameAttr.Value = "com.elprofesorkudo.applesigninplugin.AppleSignInActivity";
                activityNode.Attributes.Append(nameAttr);
                applicationNode.AppendChild(activityNode);
            }

            // Check and inject HTTPS intent-filter (Apple redirect)
            if (!IntentFilterExists(activityNode, androidNs, "https", redirectHost, redirectPath))
            {
                XmlElement httpsIntentFilter = manifest.CreateElement("intent-filter");

                XmlAttribute labelAttr = manifest.CreateAttribute("android", "label", androidNs);
                labelAttr.Value = "Apple Sign In Redirect";
                httpsIntentFilter.Attributes.Append(labelAttr);

                XmlAttribute autoVerifyAttr = manifest.CreateAttribute("android", "autoVerify", androidNs);
                autoVerifyAttr.Value = "false";
                httpsIntentFilter.Attributes.Append(autoVerifyAttr);

                httpsIntentFilter.AppendChild(CreateIntentElement(manifest, "action", "android.intent.action.VIEW", androidNs));
                httpsIntentFilter.AppendChild(CreateIntentElement(manifest, "category", "android.intent.category.DEFAULT", androidNs));
                httpsIntentFilter.AppendChild(CreateIntentElement(manifest, "category", "android.intent.category.BROWSABLE", androidNs));

                XmlElement data1 = manifest.CreateElement("data");
                data1.Attributes.Append(CreateAndroidAttribute(manifest, "scheme", androidNs, "https"));
                data1.Attributes.Append(CreateAndroidAttribute(manifest, "host", androidNs, redirectHost));
                data1.Attributes.Append(CreateAndroidAttribute(manifest, "path", androidNs, redirectPath));
                httpsIntentFilter.AppendChild(data1);

                activityNode.AppendChild(httpsIntentFilter);
            }

            // Check and inject custom scheme intent-filter
            if (!IntentFilterExists(activityNode, androidNs, customScheme, customHost, null))
            {
                XmlElement customIntentFilter = manifest.CreateElement("intent-filter");

                customIntentFilter.AppendChild(CreateIntentElement(manifest, "action", "android.intent.action.VIEW", androidNs));
                customIntentFilter.AppendChild(CreateIntentElement(manifest, "category", "android.intent.category.DEFAULT", androidNs));
                customIntentFilter.AppendChild(CreateIntentElement(manifest, "category", "android.intent.category.BROWSABLE", androidNs));

                XmlElement data2 = manifest.CreateElement("data");
                data2.Attributes.Append(CreateAndroidAttribute(manifest, "scheme", androidNs, customScheme));
                data2.Attributes.Append(CreateAndroidAttribute(manifest, "host", androidNs, customHost));
                customIntentFilter.AppendChild(data2);

                activityNode.AppendChild(customIntentFilter);
            }

            manifest.Save(manifestPath);
            AssetDatabase.Refresh();
            Debug.Log("Apple intent-filters injected into AndroidManifest.xml");
        }

        // Fonction helper pour vérifier l'existence d'un intent-filter avec un data matching
        private static bool IntentFilterExists(XmlNode activityNode, string androidNs, string scheme, string host, string path)
        {
            foreach (XmlNode intentFilter in activityNode.ChildNodes)
            {
                if (intentFilter.Name != "intent-filter")
                    continue;

                foreach (XmlNode dataNode in intentFilter.ChildNodes)
                {
                    if (dataNode.Name != "data")
                        continue;

                    var schemeAttr = dataNode.Attributes?["android:scheme"];
                    var hostAttr = dataNode.Attributes?["android:host"];
                    var pathAttr = dataNode.Attributes?["android:path"];

                    bool schemeMatch = schemeAttr != null && schemeAttr.Value == scheme;
                    bool hostMatch = hostAttr != null && hostAttr.Value == host;
                    bool pathMatch = (path == null) || (pathAttr != null && pathAttr.Value == path);

                    if (schemeMatch && hostMatch && pathMatch)
                        return true;
                }
            }

            return false;
        }

        private static XmlElement CreateIntentElement(XmlDocument doc, string tag, string androidName, string ns)
        {
            XmlElement element = doc.CreateElement(tag);

            XmlAttribute nameAttr = doc.CreateAttribute("android", "name", ns);
            nameAttr.Value = androidName;
            element.Attributes.Append(nameAttr);

            return element;
        }

        private static XmlAttribute CreateAndroidAttribute(XmlDocument doc, string localName, string ns, string value)
        {
            XmlAttribute attr = doc.CreateAttribute("android", localName, ns);
            attr.Value = value;
            return attr;
        }
    }
}
#endif
