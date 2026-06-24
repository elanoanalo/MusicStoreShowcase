using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System;
using System.Collections.Generic;

namespace MusicStoreShowcase.Models
{
    public static class MusicGenerator
    {
        public static byte[] GenerateMidi(ulong userSeed, int trackIndex)
        {
            unchecked
            {
                ulong combined = userSeed * 6364136223846793005UL + (ulong)trackIndex + 0x1234567UL;
                int musicSeed = (int)(combined & 0x7FFFFFFF);
                var rand = new Random(musicSeed);

                // Гамма до-мажор (ноты, которые звучат гармонично вместе).
                // Берём ноты только из этого набора — поэтому мелодия не хаотична.
                int[] scale = { 60, 62, 64, 65, 67, 69, 71, 72 }; // C D E F G A B C

                var pattern = new Melanchall.DryWetMidi.Composing.PatternBuilder();

                int noteCount = 16; // длина мелодии
                for (int i = 0; i < noteCount; i++)
                {
                    int noteNumber = scale[rand.Next(scale.Length)];
                    pattern.SetNoteLength(MusicalTimeSpan.Eighth)
                           .Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)noteNumber));
                }

                var midiFile = pattern.Build().ToFile(TempoMap.Default);

                using var stream = new System.IO.MemoryStream();
                midiFile.Write(stream, format: MidiFileFormat.MultiTrack,
                               settings: new WritingSettings());
                return stream.ToArray();
            }
        }
    }
}