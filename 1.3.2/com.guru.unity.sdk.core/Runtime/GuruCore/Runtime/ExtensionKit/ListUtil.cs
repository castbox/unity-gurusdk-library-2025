using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class ListUtil
{
    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return list == null || list.Count == 0;
    }

    public static T GetElement<T>(this IList<T> list, int index)
    {
        if (list == null)
            return default(T);

        if (index < 0 || index >= list.Count)
            return default(T);

        return list[index];
    }

    #region Functional OP

    //Map
    public static List<U> MapList<T, U>(this IList<T> list, Func<T, U> handler)
    {
        List<U> result = new List<U>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            U e = handler(list[i]);
            result.Add(e);
        }

        return result;
    }

    public static void MapList<T>(this IList<T> list, Action<T> handler)
    {
        for (int i = 0; i < list.Count; i++)
        {
            handler(list[i]);
        }
    }

    //Filter
    public static List<T> FilterList<T>(this IList<T> list, Predicate<T> pred)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            T e = list[i];
            if (pred(e))
                result.Add(e);
        }

        return result;
    }

    //Fold 1
    public static T FoldList<T>(this IList<T> list, Func<T, T, T> func)
    {
        if (list.Count == 0)
            return default(T);

        T acc = list[0];
        for (int i = 1; i < list.Count; i++)
            acc = func(acc, list[i]);
        return acc;
    }

    //Fold 2
    public static S FoldList<T, S>(this IList<T> list, S acc, Func<S, T, S> func)
    {
        for (int i = 0; i < list.Count; i++)
            acc = func(acc, list[i]);
        return acc;
    }

    public static void ForEach<T>(this IList<T> list, Action<T> handler)
    {
        for (int i = 0; i < list.Count; i++)
        {
            handler(list[i]);
        }
    }

    #endregion

    #region Create OP

    //Generate list contains [start, end)
    public static List<int> CreateIntList(int start, int end)
    {
        List<int> result = new List<int>(end - start);
        for (int n = start; n < end; n++)
            result.Add(n);
        return result;
    }

    public static List<int> CreateIntList(int value, int start, int end)
    {
        List<int> result = new List<int>(end - start);
        for (int i = 0; i < end - start; ++i)
        {
            result.Add(value);
        }

        return result;
    }

    #endregion

    #region Collection OP

    //intersection: l1 intersect l2
    public static List<T> IntersectList<T>(IList<T> l1, IList<T> l2)
    {
        Predicate<T> pred = (T e) => { return l2.Contains(e); };
        List<T> result = FilterList(l1, pred);
        return result;
    }

    //subtraction: l1 - l2
    public static List<T> SubtractList<T>(List<T> l1, IList<T> l2)
    {
        Predicate<T> pred = (T e) => { return !l2.Contains(e); };
        List<T> result = FilterList(l1, pred);
        return result;
    }

    //Remove the elements which another list doesn't contain
    public static void RemoveDifferenceSet<T>(this IList<T> superList, IList<T> subList)
    {
        for (int i = 0; i < superList.Count; i++)
        {
            if (!IsContainElement(subList, superList[i]))
            {
                superList.RemoveAt(i);
                i--;
            }
        }
    }

    #endregion

    #region Find OP

    //Find all indexes for a particular element
    public static List<int> FindAllIndexes<T>(this IList<T> list, T element) where T : IComparable<T>
    {
        List<int> result = FindAllIndexes(list, (T e) => element.CompareTo(e) == 0);
        return result;
    }

    //Find all indexes for a predicate
    public static List<int> FindAllIndexes<T>(this IList<T> list, Predicate<T> pred)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
                result.Add(i);
        }

        return result;
    }

    //Find the index for a predicate 有扩展了
    public static int Find<T>(this IList<T> list, Predicate<T> pred)
    {
        int result = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
            {
                result = i;
                break;
            }
        }

        return result;
    }

    //Find
    public static T FindFirstOrDefault<T>(this IList<T> list, Predicate<T> pred)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
            {
                return list[i];
            }
        }

        return default(T);
    }

    //Find last
    public static T FindLastOrDefault<T>(this IList<T> list, Predicate<T> pred)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (pred(list[i]))
            {
                return list[i];
            }
        }

        return default(T);
    }

    #endregion

    //If all elements are the same
    public static bool IsAllElementsSame<T>(this IList<T> list) where T : IComparable<T>
    {
        bool result = false;
        if (list.Count > 0)
        {
            Predicate<T> pred = (T t) => { return t.CompareTo(list[0]) == 0; };
            result = IsAllElementsSatisfied(list, pred);
        }

        return result;
    }

    //If all elements satisfy a predicate
    public static bool IsAllElementsSatisfied<T>(this IList<T> list, Predicate<T> pred)
    {
        bool result = true;
        for (int i = 0; i < list.Count; i++)
        {
            if (!pred(list[i]))
            {
                result = false;
                break;
            }
        }

        return result;
    }

    //If any element satisfy a predicate
    public static bool IsAnyElementSatisfied<T>(this IList<T> list, Predicate<T> pred)
    {
        bool result = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //Fill the list with an single element
    public static void FillElements<T>(this IList<T> list, T element)
    {
        for (int i = 0; i < list.Count; i++)
            list[i] = element;
    }

    //Count the number of one element
    public static int GetElementCount<T>(this IList<T> list, T element) where T : IComparable<T>
    {
        int count = GetElementCount<T>(list, (T e) => { return element.CompareTo(e) == 0; });
        return count;
    }

    //Count the number for predicate
    public static int GetElementCount<T>(this IList<T> list, Predicate<T> pred)
    {
        int count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
                ++count;
        }

        return count;
    }

    //If a list contains a particular element
    public static bool IsContainElement<T>(this IList<T> list, T element)
    {
        bool result = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(element))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //If a list contains all elements in another list
    public static bool IsContainAllElements<T>(this IList<T> superList, IList<T> subList)
    {
        bool result = IsAllElementsSatisfied(subList, (T e) => { return superList.Contains(e); });
        return result;
    }

    //If two lists are the same
    public static bool IsEqualLists<T>(this IList<T> l1, IList<T> l2) where T : IComparable<T>
    {
        bool result = false;
        if (l1.Count == l2.Count)
        {
            result = true;
            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i].CompareTo(l2[i]) != 0)
                {
                    result = false;
                    break;
                }
            }
        }

        return result;
    }

    //Count element number for a predicate
    public static int CountElements<T>(this IList<T> list, Predicate<T> pred)
    {
        int result = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
                ++result;
        }

        return result;
    }

    public static void AddElementsToList<T>(IList<T> listDst, IList<T> listSrc)
    {
        if (listSrc.IsNullOrEmpty())
            return;

        for (int i = 0; i < listSrc.Count; i++)
        {
            listDst.Add(listSrc[i]);
        }
    }

    public static void RemoveAllNull<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null)
                list.RemoveAt(i);
        }
    }

    public static T RandomOne<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogError("list is null or empty");
            return default(T);
        }

        return list[new Random().Next(0, list.Count)];
    }

    public static T Random<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static T RandomOne<T>(this IList<T> list, int seed)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogError("list is null or empty");
            return default(T);
        }

        return list[new Random(seed).Next(0, list.Count)];
    }

    /// <summary>
    /// 遇到规避项选后一个，时间固定，避免无对象抽或者超时
    /// 弊端是非均匀随机，看情况使用
    /// </summary>
    public static T RandomOneAvoid<T>(this IList<T> list, T avoid, T fallback)
    {
        if (list == null || list.Count == 0)
            return fallback;

        var rIndex = new Random().Next(0, list.Count);
        if (Equals(list[rIndex], avoid))
            rIndex = (rIndex + 1) % list.Count;

        return list[rIndex];
    }

    public static T FirstOneOrDef<T>(this IList<T> list)
    {
        return list != null && list.Count > 0 ? list[0] : default;
    }

    public static T LastOneOrDef<T>(this IList<T> list)
    {
        return list != null && list.Count > 0 ? list[list.Count - 1] : default;
    }

    public static T FindMaxItem<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list == null || list.Count == 0)
            return default;
        var ret = list[0];
        for (var i = 1; i < list.Count; i++)
        {
            if (comparison(ret, list[i]) < 0)
                ret = list[i];
        }

        return ret;
    }

    public static T FindMinItem<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list == null || list.Count == 0)
            return default;
        var ret = list[0];
        for (var i = 1; i < list.Count; i++)
        {
            if (comparison(ret, list[i]) > 0)
                ret = list[i];
        }

        return ret;
    }
}