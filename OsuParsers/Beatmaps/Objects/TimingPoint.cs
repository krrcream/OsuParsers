using System;
using OsuParsers.Enums.Beatmaps;

namespace OsuParsers.Beatmaps.Objects
{
    public class TimingPoint
    {
        public int Offset { get; set; }
        public double BeatLength { get; set; }
        public TimeSignature TimeSignature { get; set; }
        public SampleSet SampleSet { get; set; }
        public int CustomSampleSet { get; set; }
        public int Volume { get; set; }
        public bool Inherited { get; set; }
        public Effects Effects { get; set; }
        public double BPM 
        { 
            get 
            {
                if (BeatLength < 0)
                    return -1;
                return Math.Round(60000 / BeatLength, 3);
            }
            set 
            {
                if (value < 0)
                {
                    return;
                }
                double newBeatLength = 60000 / value;
                if (Math.Abs(BeatLength - newBeatLength) > 1e-10)
                    BeatLength = newBeatLength;
            }
        }
    }
}
