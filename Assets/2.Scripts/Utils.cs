using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{

    public static void ChangeLayer(Transform transform, string name)
    {
        ChangeLayersRecursively(transform, name);
    }

    public static void ChangeLayer(Transform transform, int count)
    {
        ChangeLayersRecursively(transform, count);

    }

    public static void ChangeLayersRecursively(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach(Transform child in trans)
        {
            ChangeLayersRecursively(child, name);
        }
    }

    public static void ChangeLayersRecursively(Transform trans, int cnt)
    {
        trans.gameObject.layer = cnt;
        foreach(Transform child in trans)
        {
            ChangeLayersRecursively(child, cnt);
        }
    }

}
