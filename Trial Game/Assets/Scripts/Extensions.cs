using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void RemoveLast(Queue q)
    {
        object first = q.Peek();
        object current = null;
        while (true)
        {
            current = q.Dequeue();
            if (q.Peek() == first)
            {
                break;
            }
            q.Enqueue(current);
        }
    }

    public static T Dequeue<T>(this List<T> list)
    {
            T r = list[0];
            list.Remove(r);
            return r;
    }

    public static T Pop<T>(this List<T> list)
    {
        T r = list[list.Count - 1];
        list.Remove(r);
        return r;
    }

    public static bool IsEmpty(this Queue q)
    {
        return q.Count == 0;
    }

    public static Vector2 V2(this Vector3 v)
    {
        return v;
    }

    public static Vector3 Copy(this Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}
