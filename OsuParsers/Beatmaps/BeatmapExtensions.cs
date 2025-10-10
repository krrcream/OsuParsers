// 创建新的扩展类文件

using System;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Enums;

namespace OsuParsers.Beatmaps
{
    /// <summary>
    /// Beatmap扩展类，用于提供额外的操作方法
    /// </summary>
    public static class BeatmapExtensions
    {
        /// <summary>
        /// 替换指定索引处的HitObject为具有新EndTime的音符
        /// </summary>
        /// <param name="beatmap">要操作的Beatmap实例</param>
        /// <param name="index">要替换的音符索引</param>
        /// <param name="newEndTime">新的结束时间</param>
        public static void ReplaceHitObjectWithNewEndTime(this Beatmap beatmap, int index, int newEndTime)
        {
            if (beatmap.GeneralSection.Mode != Ruleset.Mania)
                throw new InvalidOperationException("当前模式不是Mania模式，无法执行此操作");
    
            if (index < 0 || index >= beatmap.HitObjects.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");
    
            var hitObject = beatmap.HitObjects[index];
            if (hitObject is ManiaNote maniaNote)
            {
                var newNote = maniaNote.CreateNoteWithNewEndTime(newEndTime);
                beatmap.HitObjects[index] = newNote;
        
                // 初始化新音符的必要属性
                if (newNote.ColIndex == null && maniaNote.ColIndex.HasValue)
                {
                    // 保持原来的列索引
                    newNote.ColIndex = maniaNote.ColIndex.Value;
                }
        
                // 确保NoteCircleSize被正确设置
                if (!newNote.NoteCircleSize.HasValue && beatmap.DifficultySection.CircleSize > 0)
                {
                    newNote.NoteCircleSize = (int)beatmap.DifficultySection.CircleSize;
                }
            }
            else
            {
                throw new InvalidOperationException("指定索引处的对象不是ManiaNote类型");
            }
        }
    }
}