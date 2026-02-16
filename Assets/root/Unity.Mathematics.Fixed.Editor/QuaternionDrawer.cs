using UnityEditor;
using Unity.Mathematics;

namespace Unity.Mathematics.Fixed.Editor
{
    [CustomPropertyDrawer(typeof(quaternion))]
    class QuaternionDrawer : PostNormalizedVectorDrawer
    {
        protected override SerializedProperty GetVectorProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("value");
        }

        protected override double4 Normalize(double4 value)
        {
            return Unity.Mathematics.math.normalizesafe(new Unity.Mathematics.quaternion((Unity.Mathematics.float4)value)).value;
        }
    }
}
