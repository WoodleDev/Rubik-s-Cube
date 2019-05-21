using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension {

    //https://stackoverflow.com/a/139841
    public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
    (Dictionary<TKey, TValue> original) where TValue : ICloneable
    {
    Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                            original.Comparer);
    foreach (KeyValuePair<TKey, TValue> entry in original)
    {
        ret.Add(entry.Key, (TValue) entry.Value.Clone());
    }
    return ret;
    }

    //https://stackoverflow.com/a/1082938
    public static int mod(int x, int m) {
    return (x%m + m)%m;
    }
}
