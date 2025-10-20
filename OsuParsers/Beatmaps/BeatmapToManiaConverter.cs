using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Beatmaps.Objects.Taiko;
using OsuParsers.Beatmaps.Objects.Catch;
using OsuParsers.Beatmaps.Sections;
using OsuParsers.Beatmaps.Sections.Events;
using OsuParsers.Enums;

namespace OsuParsers.Beatmaps
{
    /// <summary>
    /// 工厂类，用于将其他模式的 Beatmap 转换为 Mania 模式
    /// </summary>
    public static class BeatmapToManiaConverter
    {
        /// <summary>
        /// 将指定的 Beatmap 转换为 Mania 模式
        /// </summary>
        /// <param name="beatmap">要转换的 Beatmap</param>
        /// <param name="targetKeyCount">目标键数（列数）</param>
        /// <returns>转换后的 Beatmap</returns>
        public static Beatmap ConvertToManiaMode(this Beatmap beatmap, int targetKeyCount = 4)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            // 创建新的 Beatmap 副本
            var maniaBeatmap = new Beatmap
            {
                Version = beatmap.Version,
                OriginalFilePath = beatmap.OriginalFilePath,
                // 复制所有 Section 数据
                GeneralSection = CloneGeneralSection(beatmap.GeneralSection),
                EditorSection = CloneEditorSection(beatmap.EditorSection),
                MetadataSection = CloneMetadataSection(beatmap.MetadataSection),
                DifficultySection = CloneDifficultySection(beatmap.DifficultySection),
                EventsSection = CloneEventsSection(beatmap.EventsSection),
                ColoursSection = CloneColoursSection(beatmap.ColoursSection),
                TimingPoints = beatmap.TimingPoints.ToList(),
                BPMEvents = beatmap.BPMEvents.ToList(),
                MainBPM = beatmap.MainBPM,
                MaxBPM = beatmap.MaxBPM,
                MinBPM = beatmap.MinBPM
            };

            // 设置为 Mania 模式
            maniaBeatmap.GeneralSection.Mode = Ruleset.Mania;
            maniaBeatmap.GeneralSection.ModeId = 3;
            
            // 设置键数
            maniaBeatmap.DifficultySection.CircleSize = targetKeyCount;

            // 转换 HitObjects
            maniaBeatmap.HitObjects = ConvertHitObjects(beatmap.HitObjects, targetKeyCount);

            // 初始化 Mania 特有属性
            InitializeManiaProperties(maniaBeatmap, targetKeyCount);

            return maniaBeatmap;
        }

        /// <summary>
        /// 克隆 GeneralSection
        /// </summary>
        private static BeatmapGeneralSection CloneGeneralSection(BeatmapGeneralSection source)
        {
            return new BeatmapGeneralSection
            {
                AudioFilename = source.AudioFilename,
                AudioLeadIn = source.AudioLeadIn,
                PreviewTime = source.PreviewTime,
                Countdown = source.Countdown,
                SampleSet = source.SampleSet,
                StackLeniency = source.StackLeniency,
                Mode = source.Mode,
                ModeId = source.ModeId,
                LetterboxInBreaks = source.LetterboxInBreaks,
                WidescreenStoryboard = source.WidescreenStoryboard,
                StoryFireInFront = source.StoryFireInFront,
                SpecialStyle = source.SpecialStyle,
                EpilepsyWarning = source.EpilepsyWarning,
                UseSkinSprites = source.UseSkinSprites,
                CirclesCount = source.CirclesCount,
                SlidersCount = source.SlidersCount,
                SpinnersCount = source.SpinnersCount,
                Length = source.Length
            };
        }

        /// <summary>
        /// 克隆 EditorSection
        /// </summary>
        private static BeatmapEditorSection CloneEditorSection(BeatmapEditorSection source)
        {
            return new BeatmapEditorSection
            {
                Bookmarks = source.Bookmarks?.ToArray(),
                DistanceSpacing = source.DistanceSpacing,
                BeatDivisor = source.BeatDivisor,
                GridSize = source.GridSize,
                TimelineZoom = source.TimelineZoom
            };
        }

        /// <summary>
        /// 克隆 MetadataSection
        /// </summary>
        private static BeatmapMetadataSection CloneMetadataSection(BeatmapMetadataSection source)
        {
            return new BeatmapMetadataSection
            {
                Title = source.Title,
                TitleUnicode = source.TitleUnicode,
                Artist = source.Artist,
                ArtistUnicode = source.ArtistUnicode,
                Creator = source.Creator,
                Version = source.Version,
                Source = source.Source,
                Tags = source.Tags?.ToArray(),
                BeatmapID = source.BeatmapID,
                BeatmapSetID = source.BeatmapSetID
            };
        }

        /// <summary>
        /// 克隆 DifficultySection
        /// </summary>
        private static BeatmapDifficultySection CloneDifficultySection(BeatmapDifficultySection source)
        {
            return new BeatmapDifficultySection
            {
                HPDrainRate = source.HPDrainRate,
                CircleSize = source.CircleSize,
                OverallDifficulty = source.OverallDifficulty,
                ApproachRate = source.ApproachRate,
                SliderMultiplier = source.SliderMultiplier,
                SliderTickRate = source.SliderTickRate
            };
        }

        /// <summary>
        /// 克隆 EventsSection
        /// </summary>
        private static BeatmapEventsSection CloneEventsSection(BeatmapEventsSection source)
        {
            return new BeatmapEventsSection
            {
                BackgroundImage = source.BackgroundImage,
                Video = source.Video,
                VideoOffset = source.VideoOffset,
                Breaks = source.Breaks.Select(b => new BeatmapBreakEvent(b.StartTime, b.EndTime)).ToList(),
                Storyboard = source.Storyboard // 注意：这里直接引用，如需深拷贝需要额外处理
            };
        }

        /// <summary>
        /// 克隆 ColoursSection
        /// </summary>
        private static BeatmapColoursSection CloneColoursSection(BeatmapColoursSection source)
        {
            return new BeatmapColoursSection
            {
                ComboColours = source.ComboColours.ToList(),
                SliderTrackOverride = source.SliderTrackOverride,
                SliderBorder = source.SliderBorder
            };
        }

        /// <summary>
        /// 转换 HitObjects 到 Mania 模式
        /// </summary>
        private static List<HitObject> ConvertHitObjects(List<HitObject> sourceObjects, int keyCount)
        {
            var convertedObjects = new List<HitObject>();

            // 按时间排序源对象
            var sortedObjects = sourceObjects.OrderBy(h => h.StartTime).ToList();

            // 为每个时间点的对象分配列
            var timeGroups = sortedObjects.GroupBy(h => h.StartTime).ToList();

            foreach (var timeGroup in timeGroups)
            {
                var objectsInTime = timeGroup.ToList();
                int objectCount = objectsInTime.Count;
                
                for (int i = 0; i < objectCount; i++)
                {
                    var obj = objectsInTime[i];
                    // 计算列索引，均匀分布在键数上
                    int columnIndex = i * keyCount / objectCount;
                    // 确保不超出范围
                    columnIndex = Math.Min(columnIndex, keyCount - 1);
                    
                    // 转换对象
                    var maniaObject = ConvertToManiaObject(obj, columnIndex, keyCount);
                    convertedObjects.Add(maniaObject);
                }
            }

            return convertedObjects;
        }

        /// <summary>
        /// 将单个对象转换为 Mania 对象
        /// </summary>
        private static HitObject ConvertToManiaObject(HitObject source, int columnIndex, int keyCount)
        {
            // 获取列的 X 坐标
            int xPosition = GetColumnXPosition(columnIndex, keyCount);
            
            // 创建新的位置向量
            var position = new Vector2(xPosition, 0); // Mania 模式中 Y 坐标通常为 0
            
            HitObject maniaObject;
            
            // 根据原始对象类型进行转换
            // 注意：需要先检查具体的子类型，再检查基类型
            switch (source)
            {
                // 处理旋转类型对象 (CatchBananaRain, TaikoSpinner, Spinner)
                case CatchBananaRain catchBananaRain:
                    // 转换为 ManiaHoldNote，使用开始时间和结束时间作为持续时间
                    maniaObject = new ManiaHoldNote(
                        position,
                        catchBananaRain.StartTime,
                        catchBananaRain.EndTime,
                        catchBananaRain.HitSound,
                        CloneExtras(catchBananaRain.Extras),
                        catchBananaRain.IsNewCombo,
                        catchBananaRain.ComboOffset
                    );
                    break;
                
                case TaikoSpinner taikoSpinner:
                    // 转换为 ManiaHoldNote，使用开始时间和结束时间作为持续时间
                    maniaObject = new ManiaHoldNote(
                        position,
                        taikoSpinner.StartTime,
                        taikoSpinner.EndTime,
                        taikoSpinner.HitSound,
                        CloneExtras(taikoSpinner.Extras),
                        taikoSpinner.IsNewCombo,
                        taikoSpinner.ComboOffset
                    );
                    break;
                    
                case Spinner spinner:
                    // 转换为 ManiaHoldNote，使用开始时间和结束时间作为持续时间
                    maniaObject = new ManiaHoldNote(
                        position,
                        spinner.StartTime,
                        spinner.EndTime,
                        spinner.HitSound,
                        CloneExtras(spinner.Extras),
                        spinner.IsNewCombo,
                        spinner.ComboOffset
                    );
                    break;
                
                // 处理滑条类型对象 (TaikoDrumroll, CatchJuiceStream, Slider)
                case TaikoDrumroll taikoDrumroll:
                    // 转换为 ManiaHoldNote
                    maniaObject = new ManiaHoldNote(
                        position,
                        taikoDrumroll.StartTime,
                        taikoDrumroll.EndTime,
                        taikoDrumroll.HitSound,
                        CloneExtras(taikoDrumroll.Extras),
                        taikoDrumroll.IsNewCombo,
                        taikoDrumroll.ComboOffset
                    );
                    break;
                    
                case CatchJuiceStream catchJuiceStream:
                    // 转换为 ManiaHoldNote
                    maniaObject = new ManiaHoldNote(
                        position,
                        catchJuiceStream.StartTime,
                        catchJuiceStream.EndTime,
                        catchJuiceStream.HitSound,
                        CloneExtras(catchJuiceStream.Extras),
                        catchJuiceStream.IsNewCombo,
                        catchJuiceStream.ComboOffset
                    );
                    break;
                    
                case Slider slider:
                    // 转换为 ManiaHoldNote
                    maniaObject = new ManiaHoldNote(
                        position,
                        slider.StartTime,
                        slider.EndTime,
                        slider.HitSound,
                        CloneExtras(slider.Extras),
                        slider.IsNewCombo,
                        slider.ComboOffset
                    );
                    break;
                
                // 处理点击类型对象 (CatchFruit, TaikoHit)
                case CatchFruit catchFruit:
                    // 转换为普通 ManiaNote
                    maniaObject = new ManiaNote(
                        position,
                        catchFruit.StartTime,
                        catchFruit.StartTime, // endTime 与 startTime 相同表示普通音符
                        catchFruit.HitSound,
                        CloneExtras(catchFruit.Extras),
                        catchFruit.IsNewCombo,
                        catchFruit.ComboOffset
                    );
                    break;
                    
                case TaikoHit taikoHit:
                    // 转换为普通 ManiaNote
                    maniaObject = new ManiaNote(
                        position,
                        taikoHit.StartTime,
                        taikoHit.StartTime, // endTime 与 startTime 相同表示普通音符
                        taikoHit.HitSound,
                        CloneExtras(taikoHit.Extras),
                        taikoHit.IsNewCombo,
                        taikoHit.ComboOffset
                    );
                    break;
                    
                // 处理普通点击类型对象 (HitCircle, ManiaNote)
                default:
                    // 转换为普通 ManiaNote
                    maniaObject = new ManiaNote(
                        position,
                        source.StartTime,
                        source.StartTime, // endTime 与 startTime 相同表示普通音符
                        source.HitSound,
                        CloneExtras(source.Extras),
                        source.IsNewCombo,
                        source.ComboOffset
                    );
                    break;
            }

            // 如果是 ManiaNote，设置列索引
            if (maniaObject is ManiaNote maniaNote)
            {
                maniaNote.ColIndex = columnIndex;
                maniaNote.OriginalColIndex = columnIndex;
            }

            return maniaObject;
        }


        /// <summary>
        /// 获取指定列的 X 坐标
        /// </summary>
        private static int GetColumnXPosition(int columnIndex, int keyCount)
        {
            if (keyCount <= 0 || columnIndex < 0 || columnIndex >= keyCount)
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "列索引超出范围");

            // 使用标准的 Mania 列位置计算方式
            double columnWidth = 512.0 / keyCount;
            return (int)((columnIndex + 0.5) * columnWidth);
        }

        /// <summary>
        /// 克隆 Extras 对象
        /// </summary>
        private static Extras CloneExtras(Extras source)
        {
            if (source == null)
                return new Extras();

            return new Extras(
                source.SampleSet,
                source.AdditionSet,
                source.CustomIndex,
                source.Volume,
                source.SampleFileName
            );
        }

        /// <summary>
        /// 初始化 Mania 模式特有的属性
        /// </summary>
        private static void InitializeManiaProperties(Beatmap maniaBeatmap, int keyCount)
        {
            // 按时间排序 HitObjects
            var sortedObjects = maniaBeatmap.HitObjects
                .OrderBy(h => h.StartTime)
                .ThenBy(h => h.Position.X)
                .ToList();

            // 按时间点分组并分配行索引
            var timeGroups = sortedObjects
                .Where(h => h is ManiaNote)
                .Cast<ManiaNote>()
                .GroupBy(note => note.StartTime)
                .ToList();

            // 为每组相同时间点的音符分配相同的行索引
            for (int i = 0; i < timeGroups.Count; i++)
            {
                var group = timeGroups[i];
                foreach (var maniaNote in group)
                {
                    maniaNote.RowIndex = i;
                    maniaNote.NoteCircleSize = keyCount;
                    maniaNote.InitializeRowData(keyCount);
                    maniaNote.BeatLengthOfThisNote = GetBeatLengthForTime(maniaBeatmap, maniaNote.StartTime);
                }
            }

            maniaBeatmap.HitObjects = sortedObjects;
        }

        /// <summary>
        /// 获取指定时间点的节拍长度
        /// </summary>
        private static int GetBeatLengthForTime(Beatmap beatmap, int time)
        {
            if (beatmap.TimingPoints.Count == 0)
                return 500; // 默认值

            for (int i = beatmap.TimingPoints.Count - 1; i >= 0; i--)
            {
                if (time >= beatmap.TimingPoints[i].Offset)
                {
                    return (int)Math.Round(Math.Abs(beatmap.TimingPoints[i].BeatLength));
                }
            }
            return (int)Math.Round(Math.Abs(beatmap.TimingPoints[0].BeatLength));
        }
       
    }
}
