using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects.Mania;
using OsuParsers.Decoders;
using OsuParsers.Enums;
using OsuParsers.Extensions;


public class BeatmapToManiaConverter
{
    public static void Run()
    { 
        string projectRoot = Directory.GetCurrentDirectory();
        string fullPath1 = Path.Combine(projectRoot, "DecodeTest", "stdtest.osu");
          Beatmap beatmap1 = BeatmapDecoder
            .Decode(fullPath1);
          
          var beatmapMania = beatmap1.ConvertToManiaMode(10);
          
          var title = beatmapMania.MetadataSection.Title;
          var artist = beatmapMania.MetadataSection.Artist;
          Console.WriteLine($"{artist} - {title}");
          
          var notes = beatmapMania.HitObjects.AsManiaNotes();
          
          var PX = new newPositionX(10);
          
          Console.WriteLine($"修改前的{beatmapMania.HitObjects[0].Position.X}");
          notes.First().Position = PX.Vector2(2);  
          Console.WriteLine($"修改后的{beatmapMania.HitObjects[0].Position.X}");
    }
    
}