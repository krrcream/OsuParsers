using System.Numerics;
using OsuParsers.Beatmaps.Objects.Mania;

namespace OsuParsers.Extensions
{
    /// <summary>
    /// Mania音符扩展类，用于创建和转换音符对象
    /// </summary>
    public static class ManiaNoteExtensions
    {
        /// <summary>
        /// 克隆音符并可选地修改 StartTime、Position.X 和 EndTime
        /// </summary>
        /// <param name="existingNote">原始音符</param>
        /// <param name="StartTime">新的开始时间（不指定则保持原值）</param>
        /// <param name="PositionX">新的列位置position.X</param>
        /// <param name="EndTime">新的结束时间（不指定则保持原值）</param>
        /// <returns>新的音符对象</returns>
        public static ManiaNote CloneNote(this ManiaNote existingNote, 
            int? PositionX = null, 
            int? StartTime = null, 
            int? EndTime = null)
        {
            // 使用原始值作为默认
            var startTime = StartTime ?? existingNote.StartTime;
            var x = PositionX ?? existingNote.Position.X;
            var endTime = EndTime ?? existingNote.EndTime;

            ManiaNote newNote;

            // 判断是否为 Hold Note（EndTime > StartTime + 34ms, 34ms为OD10的300判定，避免生成太短的Hold Note）
            if (endTime > startTime + 34)
            {
                newNote = new ManiaHoldNote(
                    new Vector2(x, existingNote.Position.Y),  // 只修改 X，Y 不变
                    startTime,
                    endTime,
                    existingNote.HitSound,
                    existingNote.Extras,
                    existingNote.IsNewCombo,
                    existingNote.ComboOffset
                );
            }
            else
            {
                newNote = new ManiaNote(
                    new Vector2(x, existingNote.Position.Y),
                    startTime,
                    startTime,  // 短音符的 StartTime 和 EndTime 相同
                    existingNote.HitSound,
                    existingNote.Extras,
                    existingNote.IsNewCombo,
                    existingNote.ComboOffset
                );
            }

            // 克隆 ManiaNote 特有属性
            newNote.BeatLengthOfThisNote = existingNote.BeatLengthOfThisNote;
            newNote.RowIndex = existingNote.RowIndex;
            newNote.OriginalColIndex = existingNote.OriginalColIndex;
            
            // 注意：NoteCircleSize 需要特别处理，因为它会影响 ColIndex 的计算
            // 我们先设置 NoteCircleSize，这样在设置 Position 时 ColIndex 会正确计算
            newNote.NoteCircleSize = existingNote.NoteCircleSize;
            
            // 如果是手动设置了 ColIndex 的情况，需要保持这个状态
            if (existingNote is ManiaNote note && 
                typeof(ManiaNote).GetField("_isColIndexManuallySet", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(existingNote) is bool isManuallySet && isManuallySet)
            {
                // 通过反射设置私有字段
                var isColIndexManuallySetField = typeof(ManiaNote).GetField("_isColIndexManuallySet", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                isColIndexManuallySetField?.SetValue(newNote, true);
                
                // 如果原来有 ColIndex 值，也设置上去
                if (existingNote.ColIndex.HasValue)
                {
                    newNote.ColIndex = existingNote.ColIndex;
                }
            }

            return newNote;
        }        
        
        
        
    }
}