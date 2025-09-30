using System.Diagnostics;
using System.Reflection;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;

Beatmap beatmap1 = BeatmapDecoder
    .Decode(@"C:\Users\klcof\RiderProjects\OsuParsers\Testing\Testing\DecodeTest\Tino & Cha Shao Jun feat. Orihara RuruMashiro KanonHiiroLing Yuan Yousa - Chun Ri You (krrcream) [(LV.13) Another].osu");

//HOW TO USE (WHEN MODE IS MANIA)
List<ManiaNote> allManiaObjects = beatmap1.HitObjects.OfType<ManiaNote>().ToList();

var Note  = allManiaObjects[1];
var LN = allManiaObjects[2];
Console.WriteLine($"Beford change index note.Position.X: {beatmap1.HitObjects[1].Position.X}");
if (Note is ManiaNote maniaNote)
{
    maniaNote.ColIndex = 0;
}
Console.WriteLine($"After change index note.Position.X: {beatmap1.HitObjects[1].Position.X}");




Console.WriteLine($"=========================================");
Console.WriteLine($"ALL Note Properties");
PrintProperties(Note);
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