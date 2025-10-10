using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Enums.Beatmaps;
using System.Numerics;

namespace OsuParsers.Beatmaps.Objects.Mania
{
    /// <summary>
    /// Mania音符扩展类，用于创建和转换音符对象
    /// </summary>
    public static class ManiaNoteExtensions
    {
        /// <summary>
        /// 基于当前音符创建具有新 EndTime 的音符对象，必须音符的EndTime大于StartTime34ms(OD10的300判定，避免生成过短的HoldNote)
        /// </summary>
        /// <param name="existingNote">当前音符对象</param>
        /// <param name="newEndTime">新的结束时间</param>
        /// <returns>新的音符对象</returns>
        public static ManiaNote CreateNoteWithNewEndTime(this ManiaNote existingNote, int newEndTime)
        {
            if (newEndTime > existingNote.StartTime + 34)
            {
                return new ManiaHoldNote(existingNote.Position, existingNote.StartTime, newEndTime,
                    existingNote.HitSound, existingNote.Extras, existingNote.IsNewCombo, existingNote.ComboOffset);
            }
            else 
            {
                return new ManiaNote(existingNote.Position, existingNote.StartTime, existingNote.StartTime,
                    existingNote.HitSound, existingNote.Extras, existingNote.IsNewCombo, existingNote.ComboOffset);
            }
        }
    }
}