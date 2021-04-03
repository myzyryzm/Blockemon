using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeControl;
using UnityEngine.Networking;
using Blockemon.Messages;

namespace Blockemon
{
    public class BlockemonManager : MonoBehaviour
    {
        public List<Blockemon> blockemons = new List<Blockemon>();
        IEnumerator getBlockemonFromServer;

        void Awake()
        {
            Message.AddListener<BlockemonServerRequest>(OnBlockemonServerRequest);
        }

        void OnDestroy()
        {
            Message.RemoveListener<BlockemonServerRequest>(OnBlockemonServerRequest);
        }

        /// <summary>
        /// Runs when the BlockemonServerRequest message is sent
        /// </summary>
        /// <param name="request"></param>
        void OnBlockemonServerRequest(BlockemonServerRequest request)
        {
            getBlockemonFromServer = GetBlockemonFromServer();
            StartCoroutine(getBlockemonFromServer);
        }

        IEnumerator GetBlockemonFromServer()
        {
            // todo: connect with backend (most likely node) to call api method that queries the blockchain

            yield break;
            // string url = "http://localhost:8000/api/blockemon?user=";
            // UnityWebRequest request = UnityWebRequest.Get(url);
            // yield return request.SendWebRequest();
            // if(request.isNetworkError || request.isHttpError)
            // {
            //     Debug.LogError("ERROR");
            // }
            // else 
            // {
            //     Debug.Log("GET BLOCKEMON SUCCESS");
            //     currentSong = Instantiate(sicSongPrefab);
            //     currentSong.FromJson(request.downloadHandler.text);
            // }
            // public static Interval_get FromJson(string jsonString)
            // {
            //     return JsonUtility.FromJson<Interval_get>(jsonString);
            // }
        }
    }
}
