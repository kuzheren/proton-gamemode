using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proton.Callbacks.Manager;

public class ProtonCallbacks : MonoBehaviour
{
    private void OnEnable()
    {
        ProtonCallbacksManager.AddCallbacksTarget(this);
    }
    private void OnDisable()
    {
        ProtonCallbacksManager.RemoveCallbacksTarget(this);
    }
}
