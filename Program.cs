using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Profiles;
using System.Collections.ObjectModel;

namespace WmaEncoder.Net
{
    class Program
    {


        private static void PrintUsageHelp()
        {
            Console.WriteLine("Help goes here!");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("WmaEncoder.Net");

            if (args.Length < 2)
            {
                PrintUsageHelp();
                return;
            }

            EncodingParameters encodingParams = Parser.ParseCommandLine(args);

            // make sure they've at least specified a filename!

            if (string.IsNullOrWhiteSpace(encodingParams.InputFilename))
            {
                Console.WriteLine("No input file specified");
                return;
            }

            if (string.IsNullOrWhiteSpace(encodingParams.OutputFilename) && string.IsNullOrWhiteSpace(encodingParams.OutputFolder))
            {
                Console.WriteLine("No output filename or folder specified");
                return;
            }

            // next, try to open the specified filename.  If we can't open it, no need to bother with anything else

            MediaItem mi = null;

            try
            {
                mi = new MediaItem(encodingParams.InputFilename);
            }
            catch (InvalidMediaFileException e)
            {
                Console.WriteLine("Error opening source file: " + e.Message);
                return;
            }

            // if an output filename is specified, use that.  Otherwise, try to use the output folder

            if (!string.IsNullOrWhiteSpace(encodingParams.OutputFilename))
            {
                mi.OutputFileName = Path.GetFileName(encodingParams.OutputFilename);
            }
            else
            {
                mi.OutputFileName = Path.GetFileName(encodingParams.InputFilename);
            }

            if (!string.IsNullOrEmpty(encodingParams.Title))
            {
                mi.Metadata["Title"] = encodingParams.Title;
            }

            if (!string.IsNullOrEmpty(encodingParams.Album))
            {
                mi.Metadata["WM/AlbumTitle"] = encodingParams.Album;
            }

            if (!string.IsNullOrEmpty(encodingParams.Artist))
            {
                mi.Metadata["Author"] = encodingParams.Artist;
            }

            if (encodingParams.TrackNumber > 0)
            {
                mi.Metadata["WM/TrackNumber"] = Convert.ToString(encodingParams.TrackNumber);
            }

            if (!string.IsNullOrEmpty(encodingParams.Genre))
            {
                mi.Metadata["WM/Genre"] = encodingParams.Genre;
            }

            if (!string.IsNullOrEmpty(encodingParams.Year))
            {
                mi.Metadata["WM/Year"] = encodingParams.Year;
            }

            ObservableCollection<AudioProfile> codecProfiles = new ObservableCollection<AudioProfile>();

            Microsoft.Expression.Encoder.Profiles.LocalProfiles.EnumerateAudioCodecs(codecProfiles);

            AudioCodec selectedCodec = AudioCodec.Wma;

            if (encodingParams.Quality == 100)
            {
                selectedCodec = AudioCodec.WmaLossless;

                if (encodingParams.Bitrate < 0)
                {
                    encodingParams.Bitrate = 44100;
                }
            }
            else
            {
                selectedCodec = AudioCodec.Wma;

                if (encodingParams.Bitrate < 0)
                {
                    encodingParams.Bitrate = 128;
                }
            }

            // find the correct codec

            AudioProfile selectedAudioProfile = null;

            foreach (AudioProfile audioProfile in codecProfiles)
            {
                if (encodingParams.Quality > 0)
                {
                    if (encodingParams.Quality == 100)
                    {
                        if ((audioProfile.BitsPerSample == 16) && (audioProfile.Codec == selectedCodec) && (audioProfile.Channels == 2) && (audioProfile.SamplesPerSecond == encodingParams.Bitrate))
                        {
                            selectedAudioProfile = audioProfile;
                            break;
                        }
                    }
                    else
                        if (audioProfile.Bitrate is Microsoft.Expression.Encoder.Profiles.VariableQualityBitrate/* (audioProfile.Bitrate.IsVariableBitrate == true) */)
                        {
                            Microsoft.Expression.Encoder.Profiles.VariableQualityBitrate vqb = (Microsoft.Expression.Encoder.Profiles.VariableQualityBitrate)audioProfile.Bitrate;

                            if (vqb.Quality == encodingParams.Quality)
                            {
                                selectedAudioProfile = audioProfile;
                                break;
                            }
                        }
                }
                else
                {
                    if ((audioProfile.BitsPerSample == 16) && (audioProfile.Codec == selectedCodec) && (audioProfile.Channels == 2) && (audioProfile.Bitrate.BitrateValue == encodingParams.Bitrate))
                    {
                        selectedAudioProfile = audioProfile;
                        break;
                    }
                }
            }

            if (selectedAudioProfile != null)
            {
                mi.OutputFormat.AudioProfile = selectedAudioProfile;
            }
            else
            {
                throw new Exception("Unable to find a matching codec profile");
            }


            Console.WriteLine("Initializing Expression Encoder 4 SDK...");

            Job job = new Job();

            job.MediaItems.Add(mi);

            job.CreateSubfolder = false;

            if (string.IsNullOrWhiteSpace(encodingParams.OutputFolder))
            {
                job.OutputDirectory = Path.GetDirectoryName(Path.GetFullPath(encodingParams.OutputFilename));
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(encodingParams.OutputFolder);
                job.OutputDirectory = di.FullName;
            }

            job.EncodeProgress += new EventHandler<EncodeProgressEventArgs>(job_EncodeProgress);

            job.Encode();
        }

        static void job_EncodeProgress(object sender, EncodeProgressEventArgs e)
        {
            Console.Write(string.Format("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bEncoding: {0:f1}%", e.Progress));
        }
    }
}
