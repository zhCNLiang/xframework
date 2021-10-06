using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LuaUpdate : MonoSingleton<LuaUpdate>
{
    private List<Action> updateActions = new List<Action>();
    private List<Action> lateUpdateActions = new List<Action>();
    private List<Action> fixedUpdateActions = new List<Action>();

    public void AddUpdate(Action update)
    {
        updateActions.Add(update);
    }

    public void RemoveUpdate(Action update)
    {
        updateActions.Remove(update);
    }

    public void AddLateUpdate(Action lateUpdate)
    {
        lateUpdateActions.Add(lateUpdate);
    }

    public void RemoveLateUpdate(Action lateUpdate)
    {
        lateUpdateActions.Remove(lateUpdate);
    }

    public void AddFixedUpdate(Action fixedUpdate)
    {
        fixedUpdateActions.Add(fixedUpdate);
    }

    public void RemoveFixedUpdate(Action fixedUpdate)
    {
        fixedUpdateActions.Remove(fixedUpdate);
    }

    void Update()
    {
        foreach(var update in updateActions)
        {
            update.Invoke();
        }
    }

    void LateUpdate()
    {
        foreach(var lateUpdate in lateUpdateActions)
        {
            lateUpdate.Invoke();
        }
    }

    void FixedUpdate()
    {
        foreach(var fixedUpdate in fixedUpdateActions)
        {
            fixedUpdate.Invoke();
        }
    }
}
