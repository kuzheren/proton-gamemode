using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proton.Callbacks.Manager
{
    public static class ProtonCallbacksManager
    {
        public static List<MonoBehaviour> callbackTargets = new List<MonoBehaviour>();

        public static void AddCallbacksTarget(MonoBehaviour target)
        {
            callbackTargets.Add(target);
        }
        public static void RemoveCallbacksTarget(MonoBehaviour target)
        {
            callbackTargets.Remove(target);
        }
        public static void InvokeCallback(string methodName, object[] args=null)
        {
            foreach (var target in callbackTargets)
            {
                var method = target.GetType().GetMethod(methodName);
                if (method != null)
                {
                    if (args == null)
                    {
                        args = new object[] {};
                    }

                    try
                    {
                        method.Invoke(target, args);
                    }
                    catch (System.Reflection.TargetParameterCountException)
                    {
                        throw new System.ArgumentException($"Выполняемая коллбэком функция {methodName} не имеет нужной сигнатуры! Количество аргументов: {args.Length}");
                    }
                }
            }
        }
    }
}