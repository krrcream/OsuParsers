using System;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Encoders;
using OsuParsers.Enums;

namespace OsuParsers.Beatmaps
{
    public class Beatmap
    {
        public const int LATEST_OSZ_VERSION = 14;
        
        public int Version { get; set; } = LATEST_OSZ_VERSION;
        public BeatmapGeneralSection GeneralSection { get; set; } = new BeatmapGeneralSection();
        public BeatmapEditorSection EditorSection { get; set; } = new BeatmapEditorSection();
        public BeatmapMetadataSection MetadataSection { get; set; } = new BeatmapMetadataSection();
        public BeatmapDifficultySection DifficultySection { get; set; } = new BeatmapDifficultySection();
        public BeatmapEventsSection EventsSection { get; set; } = new BeatmapEventsSection();
        public BeatmapColoursSection ColoursSection { get; set; } = new BeatmapColoursSection();
        public List<TimingPoint> TimingPoints { get; set; } = new List<TimingPoint>();
        public List<HitObject> HitObjects { get; set; } = new List<HitObject>();
        public string OriginalFilePath { get; set; } = "";
        public List<BPMEvent> BPMEvents { get; set; } = new List<BPMEvent>();
        public double MainBPM { get; set; } = 120 ;
        public double MaxBPM { get; set; } = 120 ;
        public double MinBPM { get; set; } = 120 ;
        
        
        /// <summary>
        /// Returns nearest beat length from the given offset.
        /// </summary>
        /// <param name="offset">Time in song. Should be in milliseconds.</param>
        /// <returns></returns>
        public double BeatLengthAt(int offset)
        {
            if (TimingPoints.Count == 0)
                return 0;

            int timingPoint = 0;
            int samplePoint = 0;

            for (int i = 0; i < TimingPoints.Count; i++)
            {
                if (TimingPoints[i].Offset <= offset)
                {
                    if (TimingPoints[i].Inherited)
                        samplePoint = i;
                    else
                        timingPoint = i;
                }
            }

            double multiplier = 1;

            if (samplePoint > timingPoint && TimingPoints[samplePoint].BeatLength < 0)
                multiplier = MathHelper.CalculateBpmMultiplier(TimingPoints[samplePoint]);

            return TimingPoints[timingPoint].BeatLength * multiplier;
        }

        /// <summary>
        /// Saves this <see cref="Beatmap"/> to the specified path.
        /// </summary>
        public void Save(string path)
        {
            File.WriteAllLines(path, BeatmapEncoder.Encode(this));
        }

        public void SetCircleSizeAndMoveNote(int circleSize)
        {
            if (GeneralSection.Mode != Ruleset.Mania)
            {
                return; // 提前退出方法
            }
            foreach (ManiaNote note in HitObjects)
            {
                note.NoteCircleSize = circleSize;
            }
        }

        public (int[,], List<int>) getMTXandTimeAxis()
        {
            if (GeneralSection.Mode != Ruleset.Mania)
                throw new InvalidOperationException("当前模式不是Mania模式，无法执行此操作");
    
            List<ManiaNote> ManiaObjects = HitObjects.OfType<ManiaNote>().ToList();
            int[,] MTX = new int[ManiaObjects.Last().RowIndex.Value + 1, (int)GeneralSection.CirclesCount];
    
            // 填充MTX矩阵
            for (int i = 0; i < ManiaObjects.Count; i++)
            {
                var obj = ManiaObjects[i];
                MTX[obj.RowIndex.Value, obj.ColIndex.Value] = i; 
            }
    
            // 创建每行第一个对象的StartTime列表
            List<int> firstObjectStartTimes = new List<int>();
            var groupedByRow = ManiaObjects.GroupBy(obj => obj.RowIndex.Value)
                .OrderBy(g => g.Key);
    
            foreach (var group in groupedByRow)
            {
                var firstObjectInRow = group.OrderBy(obj => obj.StartTime).First();
                firstObjectStartTimes.Add(firstObjectInRow.StartTime);
            }
    
            return (MTX, firstObjectStartTimes);
        }
    }

}
