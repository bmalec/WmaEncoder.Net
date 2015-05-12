using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WmaEncoder.Net
{
    internal class EncodingParameters
    {
        public int Bitrate = -1;
        public string Title = null;
        public string Artist = null;
        public string Album = null;
        public string Year = null;
        public byte TrackNumber = 0;
        public string Genre = null;
        public byte Quality = 0;
        public string InputFilename = null;
        public string OutputFilename = null;
        public string OutputFolder = null;
    }


    internal static class Parser
    {
        public static EncodingParameters ParseCommandLine(string[] args)
        {
            EncodingParameters result = new EncodingParameters();

            string pendingOption = null;

            foreach (string param in args)
            {
                // check if previous argument was an option, so this argument is the option value

                if (pendingOption != null)
                {
                    if (string.Compare(pendingOption, "-b", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        byte bitrate;

                        if (byte.TryParse(param, out bitrate))
                        {
                            result.Bitrate = bitrate;
                            pendingOption = null;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Invalid bitrate \"" + param + "\"");
                        }
                    }
                    else if (string.Compare(pendingOption, "-o", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.OutputFolder = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--tt", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.Title = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--ta", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.Artist = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--tl", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.Album = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--ty", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.Year = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--tg", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result.Genre = param;
                        pendingOption = null;
                    }
                    else if (string.Compare(pendingOption, "--tn", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        byte trackNumber;

                        if (byte.TryParse(param, out trackNumber))
                        {
                            result.TrackNumber = trackNumber;
                            pendingOption = null;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Invalid track number \"" + param + "\"");
                        }
                    }
                    else if (string.Compare(pendingOption, "-V", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        byte quality;

                        if (byte.TryParse(param, out quality))
                        {
                            result.Quality = quality;
                            pendingOption = null;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Invalid quality \"" + param + "\"");
                        }
                    }

                }
                else if (param.StartsWith("-"))
                {
                    pendingOption = param;
                }
                else if (result.InputFilename == null)
                {
                    result.InputFilename = param;
                }
                else if (result.OutputFilename == null)
                {
                    result.OutputFilename = param;
                }
                else
                {
                    // add something to the ignored parameter list ;-)
                }
            }

            return result;

        }
    }
}
