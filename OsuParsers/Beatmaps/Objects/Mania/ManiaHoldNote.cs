using OsuParsers.Enums.Beatmaps;
using System.Numerics;
#nullable disable
namespace OsuParsers.Beatmaps.Objects.Mania
{
    public class ManiaHoldNote : ManiaNote
    {
        public override int HoldLength => EndTime - StartTime;
        
        public ManiaHoldNote(Vector2 position, int startTime, int endTime, HitSoundType hitSound, Extras extras, bool isNewCombo, int comboOffset)
            : base(position, startTime, endTime, hitSound, extras, isNewCombo, comboOffset)
        {
        }
    }
}
