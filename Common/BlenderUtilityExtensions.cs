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

using UnityEngine;

namespace MachinMachines.Common
{
    public static class BlenderUtilityExtensions
    {
        public static Vector3 FromUnityToBlender(this Vector3 positionVector)
        {
            return new Vector3(-positionVector.x, -positionVector.z, positionVector.y);
        }

        public static Vector3 FromBlenderToUnity(this Vector3 positionVector)
        {
            return new Vector3(-positionVector.x, positionVector.z, -positionVector.y);
        }

        /// <summary>
        /// Apply the input PRS data to the given transform
        /// </summary>
        public static void FromBlenderToUnity(
                this Transform existingTransform,
                Vector3 blenderPosition,
                Vector3 blenderRotation,
                Vector3 blenderScale)
        {
            existingTransform.localPosition = blenderPosition.FromBlenderToUnity();
            if (true)//master.rotation.eulerAngles != rot)
            {
                Vector3 convertedRot = -1.0f * blenderRotation.FromBlenderToUnity();
                existingTransform.localEulerAngles = new Vector3(existingTransform.localEulerAngles.x,
                                                                    0.0f,
                                                                    existingTransform.localEulerAngles.z);
                existingTransform.localEulerAngles += convertedRot;
            }
            existingTransform.localScale = blenderScale;
        }
    }
}
