using System.Diagnostics;
using System.Reflection;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;

Beatmap beatmap1 = BeatmapDecoder
    .Decode(@"C:\Users\klcof\RiderProjects\OsuParsers\Testing\Testing\DecodeTest\Tino & Cha Shao Jun feat. Orihara RuruMashiro KanonHiiroLing Yuan Yousa - Chun Ri You (krrcream) [(LV.13) Another].osu");
//HOW TO USE (WHEN MODE IS MANIA)
Console.WriteLine("Note Matrix");
Matrix matrix1 = beatmap1.getMTXandTimeAxis().Item1;
var timeList = beatmap1.getMTXandTimeAxis().Item2;
for (int i = Math.Min(beatmap1.Rows, 10) - 1; i >= 0; i--) //倒序打印前10行
{
    Console.Write($"Row {i}  TimeAxis {timeList[i]} :");
    for (int j = 0; j < beatmap1.OrgKeys; j++)
    {
        Console.Write($" {matrix1[i, j]} ");
    }
    Console.WriteLine();
}
Console.WriteLine();

Console.WriteLine("Note+Endtime Matrix");

(matrix1, timeList) = beatmap1.getExpandHoldBodyMTXandTimeAxis();
for (int i = Math.Min(beatmap1.Rows, 20) - 1; i >= 0; i--)
{
    Console.Write($"Row {i}  TimeAxis {timeList[i]} :");
    for (int j = 0; j < beatmap1.OrgKeys; j++)
    {
        Console.Write($" {matrix1[i, j]} ");
    }
    Console.WriteLine();
}
Console.WriteLine();

Console.WriteLine("PathProperty");
Console.WriteLine($"beatmap1.OriginalFilePath={beatmap1.OriginalFilePath}"); 
Console.WriteLine();
var allManiaObjects = beatmap1.ManiaNotes;

var Note  = allManiaObjects[1];
Console.WriteLine($"Beford change index note.Position.X: {beatmap1.HitObjects[1].Position.X}");
if (Note is ManiaNote maniaNote)
{
    maniaNote.ColIndex = 0;
}
Console.WriteLine($"Run maniaNote.ColIndex = 0;");
Console.WriteLine($"After change index note.Position.X: {beatmap1.HitObjects[1].Position.X}");
Console.WriteLine();

var LN = allManiaObjects[2];
Console.WriteLine($"Beford change HoldLength: {beatmap1.HitObjects[2].EndTime}-{beatmap1.HitObjects[2].StartTime}={beatmap1.HitObjects[2].EndTime-beatmap1.HitObjects[2].StartTime}");
LN.HoldLength = 9999;
Console.WriteLine($"Run LN.HoldLength = 9999;");
Console.WriteLine($"After change HoldLength: {beatmap1.HitObjects[2].EndTime}-{beatmap1.HitObjects[2].StartTime}={beatmap1.HitObjects[2].EndTime-beatmap1.HitObjects[2].StartTime}");
Console.WriteLine();

// Console.WriteLine($"=========================================");
// Console.WriteLine($"ALL Note Properties");
// PrintProperties(Note);
Console.WriteLine();
Console.WriteLine($"ALL LN Properties");
PrintProperties(LN);

Beatmap beatmap2 = BeatmapDecoder
    .Decode(
        @"E:\Mug\osu\Songs\173078\Nakamura Meiko - Across the Destiny (richardfeder) [Dream World].osu");

String test = $"""
              
              Beatmap2:
              {beatmap2.MetadataSection.Artist} - {beatmap2.MetadataSection.Title} ({beatmap2.MetadataSection.Creator}) [{beatmap2.MetadataSection.Version}]
              OD: {beatmap2.DifficultySection.OverallDifficulty}
              CS: {beatmap2.DifficultySection.CircleSize}
              mainBPM: {beatmap2.MainBPM}  (minBPM: {beatmap2.MinBPM} ~ MaxBPM: {beatmap2.MaxBPM})
              """;

Console.WriteLine(test);
Console.WriteLine("BPM");
foreach (var BpmEvent in beatmap2.BPMEvents)
    Console.Write($"{BpmEvent.BPM}\t|");
Console.WriteLine();
Console.WriteLine("Offset");
foreach (var BpmEvent in beatmap2.BPMEvents)
    Console.Write($"{BpmEvent.Offset}\t|");
Console.WriteLine();
Console.WriteLine("BeatLehgth");
foreach (var BpmEvent in beatmap2.BPMEvents)
    Console.Write($"{BpmEvent.BeatLength}\t|");
Console.WriteLine();
Console.WriteLine("Duration");
foreach (var BpmEvent in beatmap2.BPMEvents)
    Console.Write($"{BpmEvent.Duration}\t|");
Console.WriteLine();



void PrintProperties(object obj)
{
    if (obj == null)
    {
        Console.WriteLine("Object is null");
        return;
    }
    
    Type type = obj.GetType();
    PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    foreach (PropertyInfo property in properties)
    {
        try
        {
            object value = property.GetValue(obj);
            Console.WriteLine($"{property.Name}: {value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{property.Name}: Error getting value - {ex.Message}");
        }
    }
}