// Copyright 2022 MachinMachines
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if NET_STANDARD_2_1
using System;
#endif  // NET_STANDARD_2_1

using System.Globalization;

using UnityEngine.Profiling;

namespace MachinMachines.Utils
{
    public static class SerialisationHelpers
    {
        /// <summary>
        /// Serialise all given float values into a string
        /// Decimal count is variable to guarantee the full round trip (float value -> text -> same exact float value)
        /// </summary>
        public static string SerialiseFloatVector(float[] values)
        {
            string result = "";
            for (int i = 0; i < values.Length; ++i)
            {
                if (i != 0)
                {
                    result += ",";
                }
                // Using "round-trip" format specifier for the output formatting
                result += string.Format(CultureInfo.InvariantCulture, "{0:r}", values[i]);
            }
            return result;
        }

        /// <summary>
        /// Parse all given string values into floats
        /// </summary>
        public static float[] ParseFloatVector(string[] values)
        {
            Profiler.BeginSample("utilities - ParseFloatVector");
            float[] result = new float[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                float current = float.Parse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture);
                result[i] = current;
            }
            Profiler.EndSample();
            return result;
        }

#if NET_STANDARD_2_1
        public static float[] ParseFloatVector(ReadOnlySpan<char> values, ReadOnlySpan<char> separators, int expectedValuesCount)
        {
            Profiler.BeginSample("utilities - ParseFloatVector - Span");

            float[] result = new float[expectedValuesCount];
            int i = 0;

            int currentIndex = 0;
            int sliceLength = values.IndexOfAny(separators);
            while (sliceLength > 0)
            {
                ReadOnlySpan<char> slice = values.Slice(currentIndex, sliceLength);
                result[i] = float.Parse(slice, NumberStyles.Float, CultureInfo.InvariantCulture);
                i += 1;
                currentIndex += sliceLength + 1;
                sliceLength = values.Slice(currentIndex).IndexOfAny(separators);
            }
            // Last term
            ReadOnlySpan<char> lastSlice = values.Slice(currentIndex, values.Length - currentIndex);
            result[result.Length - 1] = float.Parse(lastSlice, NumberStyles.Float, CultureInfo.InvariantCulture);

            Profiler.EndSample();
            return result;
        }
#endif  // NET_STANDARD_2_1
    }
}
