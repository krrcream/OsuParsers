using System;
using System.IO;
using System.Linq;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;
using OsuParsers.Enums;

namespace Testing
{
    /// <summary>
    /// BeatmapToManiaConverter 测试模块
    /// </summary>
    public class BeatmapToManiaConverterTests
    {
        // 设置三个模式的测试文件路径
        private static readonly string StandardBeatmapPath = @"DecodeTest\std_etst.osu";//需要替换为实际的标准模式文件
        private static readonly string TaikoBeatmapPath = @"DecodeTest\taiko_test.osu"; // 需要替换为实际的太鼓模式文件
        private static readonly string CatchBeatmapPath = @"DecodeTest\catch_test.osu"; // 需要替换为实际的接水果模式文件

        public static void RunAllTests()
        {
            Console.WriteLine("开始测试 BeatmapToManiaConverter...");
            
            TestConvertStandardToMania();
            TestConvertTaikoToMania();
            TestConvertCatchToMania();
            TestManiaPropertiesInitializationFromFile();
            
            Console.WriteLine("所有测试完成！");
        }
        
        /// <summary>
        /// 测试 Standard 模式转换为 Mania 模式
        /// </summary>
        private static void TestConvertStandardToMania()
        {
            try
            {
                if (!File.Exists(StandardBeatmapPath))
                {
                    Console.WriteLine($"跳过 TestConvertStandardToMania: 文件不存在 {StandardBeatmapPath}");
                    return;
                }

                // 使用 Decode 加载 Beatmap
                var originalBeatmap = BeatmapDecoder.Decode(StandardBeatmapPath);
                
                // 转换为 4K Mania
                var maniaBeatmap = BeatmapToManiaConverter.ConvertToMania(originalBeatmap, 4);
                
                // 验证转换结果
                if (maniaBeatmap.GeneralSection.Mode != Ruleset.Mania)
                    throw new Exception("模式未正确设置为 Mania");
                    
                if (maniaBeatmap.GeneralSection.ModeId != 3)
                    throw new Exception("模式ID未正确设置为3");
                    
                if (maniaBeatmap.DifficultySection.CircleSize != 4)
                    throw new Exception("键数未正确设置为4");
                    
                if (maniaBeatmap.HitObjects.Count != originalBeatmap.HitObjects.Count)
                    throw new Exception("HitObjects 数量不匹配");
                
                Console.WriteLine("✓ TestConvertStandardToMania 通过");
                Console.WriteLine($"  原始对象数: {originalBeatmap.HitObjects.Count}, 转换后对象数: {maniaBeatmap.HitObjects.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ TestConvertStandardToMania 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 测试 Taiko 模式转换为 Mania 模式
        /// </summary>
        private static void TestConvertTaikoToMania()
        {
            try
            {
                if (!File.Exists(TaikoBeatmapPath))
                {
                    Console.WriteLine($"跳过 TestConvertTaikoToMania: 文件不存在 {TaikoBeatmapPath}");
                    return;
                }

                // 使用 Decode 加载 Beatmap
                var originalBeatmap = BeatmapDecoder.Decode(TaikoBeatmapPath);
                
                // 转换为 4K Mania
                var maniaBeatmap = BeatmapToManiaConverter.ConvertToMania(originalBeatmap, 4);
                
                // 验证是否都转换为了 ManiaNote 或 ManiaHoldNote
                bool allAreManiaObjects = maniaBeatmap.HitObjects.All(h => h is ManiaNote);
                if (!allAreManiaObjects)
                    throw new Exception("不是所有对象都被转换为 Mania 对象");
                
                Console.WriteLine("✓ TestConvertTaikoToMania 通过");
                Console.WriteLine($"  原始对象数: {originalBeatmap.HitObjects.Count}, 转换后对象数: {maniaBeatmap.HitObjects.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ TestConvertTaikoToMania 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 测试 Catch 模式转换为 Mania 模式
        /// </summary>
        private static void TestConvertCatchToMania()
        {
            try
            {
                if (!File.Exists(CatchBeatmapPath))
                {
                    Console.WriteLine($"跳过 TestConvertCatchToMania: 文件不存在 {CatchBeatmapPath}");
                    return;
                }

                // 使用 Decode 加载 Beatmap
                var originalBeatmap = BeatmapDecoder.Decode(CatchBeatmapPath);
                
                // 转换为 4K Mania
                var maniaBeatmap = BeatmapToManiaConverter.ConvertToMania(originalBeatmap, 4);
                
                // 验证转换结果
                if (maniaBeatmap.GeneralSection.Mode != Ruleset.Mania)
                    throw new Exception("模式未正确设置为 Mania");
                
                Console.WriteLine("✓ TestConvertCatchToMania 通过");
                Console.WriteLine($"  原始对象数: {originalBeatmap.HitObjects.Count}, 转换后对象数: {maniaBeatmap.HitObjects.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ TestConvertCatchToMania 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 测试从文件加载的 Mania 特有属性初始化
        /// </summary>
        private static void TestManiaPropertiesInitializationFromFile()
        {
            try
            {
                if (!File.Exists(StandardBeatmapPath))
                {
                    Console.WriteLine($"跳过 TestManiaPropertiesInitializationFromFile: 文件不存在 {StandardBeatmapPath}");
                    return;
                }

                // 使用 Decode 加载 Beatmap
                var originalBeatmap = BeatmapDecoder.Decode(StandardBeatmapPath);
                
                // 转换为 Mania 模式
                var maniaBeatmap = BeatmapToManiaConverter.ConvertToMania(originalBeatmap, 7); // 7K测试
                
                // 验证 ManiaNote 属性是否正确初始化
                int noteCount = 0;
                foreach (var hitObject in maniaBeatmap.HitObjects)
                {
                    if (hitObject is ManiaNote maniaNote)
                    {
                        noteCount++;
                        if (maniaNote.RowIndex < 0)
                            throw new Exception("RowIndex 未正确初始化");
                            
                        if (maniaNote.NoteCircleSize != 7)
                            throw new Exception("NoteCircleSize 未正确设置");
                    }
                }
                
                Console.WriteLine("✓ TestManiaPropertiesInitializationFromFile 通过");
                Console.WriteLine($"  处理了 {noteCount} 个 ManiaNote 对象");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ TestManiaPropertiesInitializationFromFile 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 测试从.osu文件加载并转换（通用方法）
        /// </summary>
        public static void TestConvertFromFile(string filePath, int keyCount = 4)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"文件不存在: {filePath}");
                    return;
                }
                
                // 加载 Beatmap
                var beatmap = BeatmapDecoder.Decode(filePath);
                Console.WriteLine($"加载了 {beatmap.MetadataSection.Title} - {beatmap.MetadataSection.Artist}");
                Console.WriteLine($"原始模式: {beatmap.GeneralSection.Mode}");
                Console.WriteLine($"原始对象数量: {beatmap.HitObjects.Count}");
                
                // 转换为 Mania 模式
                var maniaBeatmap = BeatmapToManiaConverter.ConvertToMania(beatmap, keyCount);
                
                Console.WriteLine($"转换后模式: {maniaBeatmap.GeneralSection.Mode}");
                Console.WriteLine($"转换后对象数量: {maniaBeatmap.HitObjects.Count}");
                Console.WriteLine($"目标键数: {maniaBeatmap.DifficultySection.CircleSize}");
                
                // 保存转换后的文件
                string outputPath = Path.ChangeExtension(filePath, $".{keyCount}k.mania.osu");
                maniaBeatmap.Save(outputPath);
                Console.WriteLine($"已保存转换后的文件到: {outputPath}");
                
                Console.WriteLine("✓ TestConvertFromFile 通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ TestConvertFromFile 失败: {ex.Message}");
            }
        }
    }
}
