using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    #region Queue Extensions -----------------------------------------------------------------------------
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

    public static bool IsEmpty<T>(this Queue<T> q)
    {
        if (q == null)
            return true;
        return q.Count == 0;
    }
    #endregion -------------------------------------------------------------------------------------------

    #region List Extensions ------------------------------------------------------------------------------
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

    public static T GetAndRemove<T>(this List<T> list, int index)
    {
        T obj = list[index];
        list.RemoveAt(index);
        return obj;
    }
    #endregion -----------------------------------------------------------------------------------------

    #region Vector Extensions --------------------------------------------------------------------------
    public static Vector2 V2(this Vector3 v)
    {
        return v;
    }

    public static double GridDistance(this Vector3 v, Vector3 distanceTo)
    {
        double disX = Mathf.Abs(distanceTo.x - v.x);
        double disY = Mathf.Abs(distanceTo.y - v.y);
        double totalDis = disX + disY;

        return totalDis;
    }

    public static double GridDistance(this Vector2 v, Vector3 distanceTo)
    {
        double disX = Mathf.Abs(distanceTo.x - v.x);
        double disY = Mathf.Abs(distanceTo.y - v.y);
        double totalDis = disX + disY;

        return totalDis;
    }

    public static Vector3 Copy(this Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Enums.PathDirection ToPathDirection(this Vector2 vector)
    {
        if (vector == Vector2.down)
            return Enums.PathDirection.Down;
        else if (vector == Vector2.up)
            return Enums.PathDirection.Up;
        else if (vector == Vector2.left)
            return Enums.PathDirection.Left;
        else if (vector == Vector2.right)
            return Enums.PathDirection.Right;
        else
            return Enums.PathDirection.Start;
    }
    #endregion -----------------------------------------------------------------------------------------

    #region AudioSource Extensions ---------------------------------------------------------------------
    public static void Play(this AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
    #endregion -----------------------------------------------------------------------------------------
}
