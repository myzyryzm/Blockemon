using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Blockemon 
{
    public class ModifyTexture : MonoBehaviour
    {
        public Dragon dragonPrefab;
        Dragon dragon;

        void Awake()
        {
            dragon = Instantiate(dragonPrefab.gameObject).GetComponent<Dragon>();
            StartCoroutine(GetTexture());
        }

        IEnumerator GetTexture() {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://ryzm-public.s3-us-west-1.amazonaws.com/T_Dragon.jpg");
            yield return www.SendWebRequest();
            if(www.isNetworkError == true)
            {
                Debug.Log(www.error);
            }
            else 
            {
                Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                dragon.bodyRenderer.material.SetTexture("_MainTex", myTexture);
            }
    }
    }
}

