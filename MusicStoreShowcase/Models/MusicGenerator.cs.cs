using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Composing;

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

                int bpm = rand.Next(90, 141);
                var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(bpm));

                int[][] scales = {
                    new int[] { 60, 62, 64, 65, 67, 69, 71, 72 }, 
                    new int[] { 57, 59, 60, 62, 64, 65, 67, 69 } 
                };
                int[] currentScale = scales[rand.Next(scales.Length)];

                int[] bassScale = new int[currentScale.Length];
                for (int i = 0; i < currentScale.Length; i++)
                {
                    bassScale[i] = currentScale[i] - 12; 
                }

                byte[] soloInstruments = { 0, 16, 24, 25, 50, 80 };
                byte[] bassInstruments = { 32, 33, 34, 35, 38 };

                byte soloProgram = soloInstruments[rand.Next(soloInstruments.Length)];
                byte bassProgram = bassInstruments[rand.Next(bassInstruments.Length)];


                var soloBuilder = new PatternBuilder().ProgramChange((SevenBitNumber)soloProgram);
                var bassBuilder = new PatternBuilder().ProgramChange((SevenBitNumber)bassProgram);

                AppendRandomNotes(soloBuilder, rand, currentScale, 8, MusicalTimeSpan.Quarter);
                bassBuilder.StepForward(MusicalTimeSpan.Quarter * 8);

                for (int i = 0; i < 2; i++)
                {
                    AppendRandomNotes(soloBuilder, rand, currentScale, 16, MusicalTimeSpan.Quarter);
                    AppendRandomNotes(bassBuilder, rand, bassScale, 8, MusicalTimeSpan.Half);
                }

                for (int i = 0; i < 2; i++)
                {
                    AppendRandomNotes(soloBuilder, rand, currentScale, 16, MusicalTimeSpan.Eighth);
                    AppendRandomNotes(bassBuilder, rand, bassScale, 16, MusicalTimeSpan.Eighth);
                }

                for (int i = 0; i < 2; i++)
                {
                    AppendRandomNotes(soloBuilder, rand, currentScale, 16, MusicalTimeSpan.Quarter);
                    AppendRandomNotes(bassBuilder, rand, bassScale, 8, MusicalTimeSpan.Half);
                }

                for (int i = 0; i < 4; i++)
                {
                    AppendRandomNotes(soloBuilder, rand, currentScale, 16, MusicalTimeSpan.Eighth);
                    AppendRandomNotes(bassBuilder, rand, bassScale, 16, MusicalTimeSpan.Eighth);
                }

                AppendRandomNotes(soloBuilder, rand, currentScale, 8, MusicalTimeSpan.Quarter);
                bassBuilder.StepForward(MusicalTimeSpan.Quarter * 8);


                TrackChunk soloChunk = soloBuilder.Build().ToTrackChunk(tempoMap, (FourBitNumber)0); 
                TrackChunk bassChunk = bassBuilder.Build().ToTrackChunk(tempoMap, (FourBitNumber)1); 

                var midiFile = new MidiFile(soloChunk, bassChunk);

                using var stream = new MemoryStream();
                midiFile.Write(stream, format: MidiFileFormat.MultiTrack, settings: new WritingSettings());
                return stream.ToArray();
            }
        }

        private static void AppendRandomNotes(PatternBuilder builder, Random rand, int[] scale, int noteCount, MusicalTimeSpan noteLength)
        {
            builder.SetNoteLength(noteLength);

            for (int i = 0; i < noteCount; i++)
            {
                if (rand.Next(100) < 15)
                {
                    builder.StepForward(noteLength);
                }
                else
                {
                    int noteNumber = scale[rand.Next(scale.Length)];
                    builder.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)noteNumber));
                }
            }
        }
    }
}