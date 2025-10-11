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
using OsuParsers.Extensions;

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
        
        
        public int Rows
        {
            get
            {
                if (GeneralSection.Mode != Ruleset.Mania)
                    return 0;
            
                var lastNote = HitObjects.LastOrDefault(h => h is ManiaNote) as ManiaNote;
                return lastNote?.RowIndex.Value + 1 ?? 0;
            }
        }
        
        public int OrgKeys
        {
            get
            {
                if (GeneralSection.Mode != Ruleset.Mania)
                    return 0;
                return (int)DifficultySection.CircleSize;
            }
        }
        
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
        
        public List<int> getEndTimeList()
        {
            if (GeneralSection.Mode != Ruleset.Mania)
                throw new InvalidOperationException("当前模式不是Mania模式，无法执行此操作");
            
            var uniqueEndTimes = new HashSet<int>();
            foreach (var note in HitObjects)
            {
                uniqueEndTimes.Add(note.EndTime);
            }
            
            return uniqueEndTimes.OrderBy(t => t).ToList();
        }
        
        public (Matrix, List<int>) getMTXandTimeAxis()
        {
            if (GeneralSection.Mode != Ruleset.Mania)
                throw new InvalidOperationException("当前模式不是Mania模式，无法执行此操作");

            // 获取所有唯一的时间点并排序
            var uniqueStartTimes = HitObjects.Select(n => n.StartTime).Distinct().OrderBy(t => t).ToList();
    
            int timeCount = uniqueStartTimes.Count;
            int keyCount = OrgKeys;

            // 创建矩阵
            var MTX = new Matrix(timeCount, keyCount);
            
            // 填充矩阵
            for (int i = 0; i < HitObjects.Count; i++)
            {
                var note = HitObjects[i];
                int timeIndex = uniqueStartTimes.IndexOf(note.StartTime);
                int colIndex = note.AsManiaNote().ColIndex.Value;
        
                // 如果该位置已经有值，则需要特殊处理（例如Long Note）
                if (MTX[timeIndex, colIndex] != Matrix.Empty)
                {
                    // 可能需要处理重叠音符的情况
                }
        
                MTX[timeIndex, colIndex] = i;
            }

            return (MTX, uniqueStartTimes);
        }

        public (Matrix, List<int>) getExpandHoldBodyMTXandTimeAxis()
        {
            if (GeneralSection.Mode != Ruleset.Mania)
                throw new InvalidOperationException("当前模式不是Mania模式，无法执行此操作");
            (Matrix m, List<int> t) = getMTXandTimeAxis();
            return ExpandHoldBody(m, t);
        }
        
        private (Matrix, List<int>) ExpandHoldBody(Matrix matrix, List<int> timeAxis)
        {
            var allTimes = new HashSet<int>(timeAxis);
            foreach (var endTime in getEndTimeList())
            {
                allTimes.Add(endTime);
            }
    
            var newTimeAxis = allTimes.OrderBy(t => t).ToList();
            
            var newMatrix = new Matrix(newTimeAxis.Count, matrix.Cols);
            
            int oldTimeIndex = 0;
            int newTimeIndex = 0;
    
            while (newTimeIndex < newTimeAxis.Count && oldTimeIndex < timeAxis.Count)
            {
                if (newTimeAxis[newTimeIndex] == timeAxis[oldTimeIndex])
                {
                    // 复制原有行数据
                    for (int col = 0; col < matrix.Cols; col++)
                    {
                        newMatrix[newTimeIndex, col] = matrix[oldTimeIndex, col];
                    }
                    oldTimeIndex++;
                    newTimeIndex++;
                }
                else if (newTimeAxis[newTimeIndex] < timeAxis[oldTimeIndex])
                {
                    // 插入新行，填充为Empty
                    for (int col = 0; col < matrix.Cols; col++)
                    {
                        newMatrix[newTimeIndex, col] = Matrix.Empty;
                    }
                    newTimeIndex++;
                }
                else
                {
                    oldTimeIndex++;
                }
            }
    
            // 如果还有剩余的新时间点，填充为Empty
            while (newTimeIndex < newTimeAxis.Count)
            {
                for (int col = 0; col < matrix.Cols; col++)
                {
                    newMatrix[newTimeIndex, col] = Matrix.Empty;
                }
                newTimeIndex++;
            }
            
            // 填充HoldBody部分
            foreach (var note in HitObjects)
            {
                if (note.EndTime > note.StartTime)
                {
                    int startIndex = newTimeAxis.IndexOf(note.StartTime);
                    int endIndex = newTimeAxis.IndexOf(note.EndTime);
                    int colIndex = note.AsManiaNote().ColIndex.Value;

                    // 在起始位置到结束位置之间填充-7表示HoldBody（包含结束位置）
                    for (int i = startIndex + 1; i <= endIndex; i++)
                    {
                        // 只有当当前位置为空时才填充HoldBody标记
                        if (newMatrix[i, colIndex] == Matrix.Empty)
                        {
                            newMatrix[i, colIndex] = Matrix.HoldBody;
                        }
                    }
                }
            }
    
            return (newMatrix, newTimeAxis);
        }
        
    }

}
