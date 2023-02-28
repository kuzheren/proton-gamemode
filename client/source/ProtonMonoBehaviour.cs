using System.Collections;
using System.Collections.Generic;
using Proton.Network;
using Proton.Packet.Handler;
using Proton.Structures;
using Proton.Global.States;
using Proton.Callbacks.Manager;
using Proton;
using UnityEngine;

public class ProtonMonoBehaviour : ProtonCallbacks
{
    private void Start()
    {
        gameObject.name = "ProtonHandler";
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(UpdateReceiveCallback());
        StartCoroutine(CheckConnection());
    }
    private IEnumerator UpdateReceiveCallback()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.001f);
            ProtonNetwork.Receive();
        }
    }
    private IEnumerator CheckConnection()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (System.DateTimeOffset.Now.ToUnixTimeSeconds() - ProtonGlobalStates.LastPingTime > 10 && ProtonEngine.IsConnected())
            {
                ProtonEngine.Disconnect();
                ProtonCallbacksManager.InvokeCallback("OnClientError", new object[] {3, "Таймаут соединения. Причина: отсутствие ответа от сервера на протяжении 10 секунд"});
            }
        }
    }
}
