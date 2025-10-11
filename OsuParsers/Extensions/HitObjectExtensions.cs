using System;
using System.Collections.Generic;
using System.Linq;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Objects.Mania;

namespace OsuParsers.Extensions;

public static class HitObjectExtensions
{
    /// <summary>
    /// 安全地更新 HitObject，保持引用一致性
    /// </summary>
    public static void UpdateHitObject<T>(this List<HitObject> hitObjects, int index, T newObject) where T : HitObject
    {
        if (index >= 0 && index < hitObjects.Count)
        {
            hitObjects[index] = newObject;
        }
    }
    
    /// <summary>
    /// 通过条件查找并更新 HitObject
    /// </summary>
    public static void UpdateHitObject<T>(this List<HitObject> hitObjects, Predicate<HitObject> predicate, T newObject) where T : HitObject
    {
        int index = hitObjects.FindIndex(predicate);
        if (index >= 0)
        {
            hitObjects[index] = newObject;
        }
    }
    
    /// <summary>
    /// 获取指定类型的对象在 HitObjects 中的索引
    /// </summary>
    public static int GetPhysicalIndex<T>(this List<HitObject> hitObjects, T typedObject) where T : HitObject
    {
        return hitObjects.IndexOf(typedObject);
    }
    
   
    public static ManiaNote AsManiaNote(this HitObject hitObject)
    {
        return hitObject as ManiaNote;
    }

    public static ManiaNote ToManiaNote(this HitObject hitObject)
    {
        return (ManiaNote)hitObject;
    }
    
    public static List<ManiaNote> AsManiaNotes(this List<HitObject> hitObjects)
    {
        return hitObjects.Cast<ManiaNote>().Where(note => note != null).ToList();
    }

    public static List<ManiaNote> ToManiaNotes(this List<HitObject> hitObjects)
    {
        return hitObjects.Select(obj => (ManiaNote)obj).ToList();
    }
    
}