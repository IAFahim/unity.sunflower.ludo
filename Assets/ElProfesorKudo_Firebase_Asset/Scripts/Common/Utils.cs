using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ElProfesorKudo.Firebase.Common
{
    public static class Utils
    {
        public static IEnumerator WaitForTaskWithTimeout(Task task, float timeoutSeconds, Callback onTimeout)
        {
            float timer = 0f;

            while (!task.IsCompleted && timer < timeoutSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!task.IsCompleted)
            {
                onTimeout?.Invoke();
            }
        }
        public static void ExecuteConditionalAction(bool condition, Callback action, string skippedLogMessage)
        {
            if (condition)
            {
                action?.Invoke();
            }
            else
            {
                Debug.Log(skippedLogMessage);
            }
        }

        public static Sprite ConvertTexture2DToSprite(this Texture2D texture2D)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        }
        public static IEnumerator DownloadImageFromWeb(string urlPicture, Callback<Sprite> spriteCallBack)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(urlPicture))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    CustomLogger.LogWarning("Error get request " + urlPicture + " " + www.error);
                    spriteCallBack(null);
                }
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    spriteCallBack(ConvertTexture2DToSprite(texture));
                }
            }
        }

        public static IEnumerator TempCoroutine(Callback callback, int frameWait = 1)
        {
            for (int i = 0; i <= frameWait; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            callback();
        }
    }
}
