using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using static Unity.Mathematics.math;
using static Unity.Mathematics.Fixed.math;

namespace Unity.Mathematics.Fixed
{
    /// <summary>
    /// A quaternion type for representing rotations.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    [Serializable]
    public partial struct quaternion : System.IEquatable<quaternion>, IFormattable
    {
        /// <summary>
        /// The quaternion component values.
        /// </summary>
        public float4 value;

        /// <summary>A quaternion representing the identity transform.</summary>
        public static readonly quaternion identity = new quaternion(fp._0, fp._0, fp._0, fp._1);

        /// <summary>Constructs a quaternion from four fp values.</summary>
        /// <param name="x">The quaternion x component.</param>
        /// <param name="y">The quaternion y component.</param>
        /// <param name="z">The quaternion z component.</param>
        /// <param name="w">The quaternion w component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public quaternion(fp x, fp y, fp z, fp w) { value.x = x; value.y = y; value.z = z; value.w = w; }

        /// <summary>Constructs a quaternion from float4 vector.</summary>
        /// <param name="value">The quaternion xyzw component values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public quaternion(float4 value) { this.value = value; }

        /// <summary>Implicitly converts a float4 vector to a quaternion.</summary>
        /// <param name="v">The quaternion xyzw component values.</param>
        /// <returns>The quaternion constructed from a float4 vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator quaternion(float4 v) { return new quaternion(v); }

        /// <summary>Constructs a unit quaternion from a float3x3 rotation matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The float3x3 orthonormal rotation matrix.</param>
        public quaternion(float3x3 m)
        {
            float3 u = m.c0;
            float3 v = m.c1;
            float3 w = m.c2;

            fp t;
            bool u_mask;
            bool t_mask;
            fp tr = fp._1 + abs(u.x);
            
            if (u.x >= 0)
            {
                // If positive, don't flip w.z
                // Also, set u_mask to false
                t = v.y + w.z;
                u_mask = false;
            }
            else
            {
                t = v.y - w.z;
                u_mask = true;
            }
                
            if (t >= 0)
            {
                // if positive, t_mask is false
                t_mask = false;
            }
            else
            {
                t_mask = true;
            }

            bool4 sign_flips = bool4(false, true, true, true) ^
                    (u_mask & bool4(false, true, false, true)) ^ 
                    (t_mask & bool4(true, true, true, false));

            value = float4(tr, u.y, w.x, v.z) + (math.float4(t, v.x, u.z, w.y) ^ sign_flips); // +---, +++-, ++-+, +-++

            if (u_mask)
            {
                value = value.zwxy;// math.asfloat_unsafe((asulong_unsafe(value) & ~u_mask) | (asulong_unsafe(value.zwxy) & u_mask));
            }
            else
            {
                value = value;
            }
            
            if (t_mask)
            {
                value = value;
            }
            else
            {
                value = value.wzyx;
            }

            value = normalize(value);
        }

        /// <summary>Constructs a unit quaternion from an orthonormal float4x4 matrix.</summary>
        /// <param name="m">The float4x4 orthonormal rotation matrix.</param>
        public quaternion(float4x4 m)
        {
            float4 u = m.c0;
            float4 v = m.c1;
            float4 w = m.c2;

            ulong u_sign = (asulong_unsafe(u.x) & 0x80000000_00000000UL);
            fp t = v.y + math.asfloat_unsafe(asulong_unsafe(w.z) ^ u_sign);
            ulong4 u_mask = ulong4((long)u_sign >> 63);
            ulong4 t_mask = ulong4(aslong_unsafe(t) >> 63);

            fp tr = fp._1 + abs(u.x);

            ulong4 sign_flips = ulong4(0x00000000_00000000UL, 0x80000000_00000000UL, 0x80000000_00000000UL, 0x80000000_00000000UL) ^ 
                      (u_mask & ulong4(0x00000000_00000000UL, 0x80000000_00000000UL, 0x00000000_00000000UL, 0x80000000_00000000UL)) ^
                      (t_mask & ulong4(0x80000000_00000000UL, 0x80000000_00000000UL, 0x80000000_00000000UL, 0x00000000_00000000UL));

            value = float4(tr, u.y, w.x, v.z) + math.asfloat_unsafe(asulong_unsafe(math.float4(t, v.x, u.z, w.y)) ^ sign_flips); // +---, +++-, ++-+, +-++

            value = math.asfloat_unsafe((asulong_unsafe(value) & ~u_mask) | (asulong_unsafe(value.zwxy) & u_mask));
            value = math.asfloat_unsafe((asulong_unsafe(value.wzyx) & ~t_mask) | (asulong_unsafe(value) & t_mask));

            value = normalize(value);
        }

        /// <summary>
        /// Returns a quaternion representing a rotation around a unit axis by an angle in radians.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in radians.</param>
        /// <returns>The quaternion representing a rotation around an axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion AxisAngle(float3 axis, fp angle)
        {
            fp sina, cosa;
            math.sincos(fp._0_50 * angle, out sina, out cosa);
            return quaternion(float4(axis * sina, cosa));
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the x-axis, then the y-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in x-y-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerXYZ(float3 xyz)
        {
            // return mul(rotateZ(xyz.z), mul(rotateY(xyz.y), rotateX(xyz.x)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-fp._1, fp._1, -fp._1, fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the x-axis, then the z-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in x-z-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerXZY(float3 xyz)
        {
            // return mul(rotateY(xyz.y), mul(rotateZ(xyz.z), rotateX(xyz.x)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.z * s.x
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(fp._1, fp._1, -fp._1, -fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the y-axis, then the x-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in y-x-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerYXZ(float3 xyz)
        {
            // return mul(rotateZ(xyz.z), mul(rotateX(xyz.x), rotateY(xyz.y)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z + s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.z * s.x
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-fp._1, fp._1, fp._1, -fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the y-axis, then the z-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in y-z-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerYZX(float3 xyz)
        {
            // return mul(rotateX(xyz.x), mul(rotateZ(xyz.z), rotateY(xyz.y)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z - s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-fp._1, -fp._1, fp._1, fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the z-axis, then the x-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// This is the default order rotation order in Unity.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in z-x-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerZXY(float3 xyz)
        {
            // return mul(rotateY(xyz.y), mul(rotateX(xyz.x), rotateZ(xyz.z)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y - s.x * s.y * c.z,
                // c.x * c.y * c.z + s.y * s.z * s.x
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(fp._1, -fp._1, -fp._1, fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the z-axis, then the y-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in z-y-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerZYX(float3 xyz)
        {
            // return mul(rotateX(xyz.x), mul(rotateY(xyz.y), rotateZ(xyz.z)));
            float3 s, c;
            sincos(fp._0_50 * xyz, out s, out c);
            return quaternion(
                // s.x * c.y * c.z + s.y * s.z * c.x,
                // s.y * c.x * c.z - s.x * s.z * c.y,
                // s.z * c.x * c.y + s.x * s.y * c.z,
                // c.x * c.y * c.z - s.y * s.x * s.z
                float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(fp._1, -fp._1, fp._1, -fp._1)
                );
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the x-axis, then the y-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in x-y-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerXYZ(fp x, fp y, fp z) { return EulerXYZ(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the x-axis, then the z-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in x-z-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerXZY(fp x, fp y, fp z) { return EulerXZY(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the y-axis, then the x-axis and finally the z-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in y-x-z order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerYXZ(fp x, fp y, fp z) { return EulerYXZ(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the y-axis, then the z-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in y-z-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerYZX(fp x, fp y, fp z) { return EulerYZX(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the z-axis, then the x-axis and finally the y-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// This is the default order rotation order in Unity.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in z-x-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerZXY(fp x, fp y, fp z) { return EulerZXY(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the z-axis, then the y-axis and finally the x-axis.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <returns>The quaternion representing the Euler angle rotation in z-y-x order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerZYX(fp x, fp y, fp z) { return EulerZYX(float3(x, y, z)); }

        /// <summary>
        /// Returns a quaternion constructed by first performing 3 rotations around the principal axes in a given order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// When the rotation order is known at compile time, it is recommended for performance reasons to use specific
        /// Euler rotation constructors such as EulerZXY(...).
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in radians.</param>
        /// <param name="order">The order in which the rotations are applied.</param>
        /// <returns>The quaternion representing the Euler angle rotation in the specified order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Euler(float3 xyz, math.RotationOrder order = math.RotationOrder.ZXY)
        {
            switch (order)
            {
                case math.RotationOrder.XYZ:
                    return EulerXYZ(xyz);
                case math.RotationOrder.XZY:
                    return EulerXZY(xyz);
                case math.RotationOrder.YXZ:
                    return EulerYXZ(xyz);
                case math.RotationOrder.YZX:
                    return EulerYZX(xyz);
                case math.RotationOrder.ZXY:
                    return EulerZXY(xyz);
                case math.RotationOrder.ZYX:
                    return EulerZYX(xyz);
                default:
                    return quaternion.identity;
            }
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing 3 rotations around the principal axes in a given order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// When the rotation order is known at compile time, it is recommended for performance reasons to use specific
        /// Euler rotation constructors such as EulerZXY(...).
        /// </summary>
        /// <param name="x">The rotation angle around the x-axis in radians.</param>
        /// <param name="y">The rotation angle around the y-axis in radians.</param>
        /// <param name="z">The rotation angle around the z-axis in radians.</param>
        /// <param name="order">The order in which the rotations are applied.</param>
        /// <returns>The quaternion representing the Euler angle rotation in the specified order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Euler(fp x, fp y, fp z, math.RotationOrder order = math.RotationOrder.Default)
        {
            return Euler(float3(x, y, z), order);
        }

        /// <summary>Returns a quaternion that rotates around the x-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the x-axis towards the origin in radians.</param>
        /// <returns>The quaternion representing a rotation around the x-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateX(fp angle)
        {
            fp sina, cosa;
            math.sincos(fp._0_50 * angle, out sina, out cosa);
            return quaternion(sina, fp._0, fp._0, cosa);
        }

        /// <summary>Returns a quaternion that rotates around the y-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the y-axis towards the origin in radians.</param>
        /// <returns>The quaternion representing a rotation around the y-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateY(fp angle)
        {
            fp sina, cosa;
            math.sincos(fp._0_50 * angle, out sina, out cosa);
            return quaternion(fp._0, sina, fp._0, cosa);
        }

        /// <summary>Returns a quaternion that rotates around the z-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the z-axis towards the origin in radians.</param>
        /// <returns>The quaternion representing a rotation around the z-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateZ(fp angle)
        {
            fp sina, cosa;
            math.sincos(fp._0_50 * angle, out sina, out cosa);
            return quaternion(fp._0, fp._0, sina, cosa);
        }

        /// <summary>
        /// Returns a quaternion view rotation given a unit length forward vector and a unit length up vector.
        /// The two input vectors are assumed to be unit length and not collinear.
        /// If these assumptions are not met use float3x3.LookRotationSafe instead.
        /// </summary>
        /// <param name="forward">The view forward direction.</param>
        /// <param name="up">The view up direction.</param>
        /// <returns>The quaternion view rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion LookRotation(float3 forward, float3 up)
        {
            float3 t = normalize(cross(up, forward));
            return quaternion(float3x3(t, cross(forward, t), forward));
        }

        /// <summary>
        /// Returns a quaternion view rotation given a forward vector and an up vector.
        /// The two input vectors are not assumed to be unit length.
        /// If the magnitude of either of the vectors is so extreme that the calculation cannot be carried out reliably or the vectors are collinear,
        /// the identity will be returned instead.
        /// </summary>
        /// <param name="forward">The view forward direction.</param>
        /// <param name="up">The view up direction.</param>
        /// <returns>The quaternion view rotation or the identity quaternion.</returns>
        public static quaternion LookRotationSafe(float3 forward, float3 up)
        {
            fp forwardLengthSq = dot(forward, forward);
            fp upLengthSq = dot(up, up);

            forward *= rsqrt(forwardLengthSq);
            up *= rsqrt(upLengthSq);

            float3 t = cross(up, forward);
            fp tLengthSq = dot(t, t);
            t *= rsqrt(tLengthSq);

            fp mn = min(min(forwardLengthSq, upLengthSq), tLengthSq);
            fp mx = max(max(forwardLengthSq, upLengthSq), tLengthSq);

            bool accept = mn > fp.epsilon && mx < fp.max && isfinite(forwardLengthSq) && isfinite(upLengthSq) && isfinite(tLengthSq);
            return quaternion(select(float4(fp._0, fp._0, fp._0, fp._1), quaternion(float3x3(t, cross(forward, t),forward)).value, accept));
        }

        /// <summary>Returns true if the quaternion is equal to a given quaternion, false otherwise.</summary>
        /// <param name="x">The quaternion to compare with.</param>
        /// <returns>True if the quaternion is equal to the input, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(quaternion x) { return value.x == x.value.x && value.y == x.value.y && value.z == x.value.z && value.w == x.value.w; }

        /// <summary>Returns whether true if the quaternion is equal to a given quaternion, false otherwise.</summary>
        /// <param name="x">The object to compare with.</param>
        /// <returns>True if the quaternion is equal to the input, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object x) { return x is quaternion converted && Equals(converted); }

        /// <summary>Returns a hash code for the quaternion.</summary>
        /// <returns>The hash code of the quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return (int)math.hash(this); }

        /// <summary>Returns a string representation of the quaternion.</summary>
        /// <returns>The string representation of the quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("quaternion({0}f, {1}f, {2}f, {3}f)", value.x, value.y, value.z, value.w);
        }

        /// <summary>Returns a string representation of the quaternion using a specified format and culture-specific format information.</summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider to use during string formatting.</param>
        /// <returns>The formatted string representation of the quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("quaternion({0}f, {1}f, {2}f, {3}f)", value.x.ToString(format, formatProvider), value.y.ToString(format, formatProvider), value.z.ToString(format, formatProvider), value.w.ToString(format, formatProvider));
        }
    }

    public static partial class math
    {
        /// <summary>Returns a quaternion constructed from four fp values.</summary>
        /// <param name="x">The x component of the quaternion.</param>
        /// <param name="y">The y component of the quaternion.</param>
        /// <param name="z">The z component of the quaternion.</param>
        /// <param name="w">The w component of the quaternion.</param>
        /// <returns>The quaternion constructed from individual components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion quaternion(fp x, fp y, fp z, fp w) { return new quaternion(x, y, z, w); }

        /// <summary>Returns a quaternion constructed from a float4 vector.</summary>
        /// <param name="value">The float4 containing the components of the quaternion.</param>
        /// <returns>The quaternion constructed from a float4.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion quaternion(float4 value) { return new quaternion(value); }

        /// <summary>Returns a unit quaternion constructed from a float3x3 rotation matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The float3x3 rotation matrix.</param>
        /// <returns>The quaternion constructed from a float3x3 matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion quaternion(float3x3 m) { return new quaternion(m); }

        /// <summary>Returns a unit quaternion constructed from a float4x4 matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The float4x4 matrix (must be orthonormal).</param>
        /// <returns>The quaternion constructed from a float4x4 matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion quaternion(float4x4 m) { return new quaternion(m); }

       /// <summary>Returns the conjugate of a quaternion value.</summary>
       /// <param name="q">The quaternion to conjugate.</param>
       /// <returns>The conjugate of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion conjugate(quaternion q)
        {
            return quaternion(q.value * float4(-fp._1, -fp._1, -fp._1, fp._1));
        }

       /// <summary>Returns the inverse of a quaternion value.</summary>
       /// <param name="q">The quaternion to invert.</param>
       /// <returns>The quaternion inverse of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion inverse(quaternion q)
        {
            float4 x = q.value;
            return quaternion(rcp(dot(x, x)) * x * float4(-fp._1, -fp._1, -fp._1, fp._1));
        }

        /// <summary>Returns the dot product of two quaternions.</summary>
        /// <param name="a">The first quaternion.</param>
        /// <param name="b">The second quaternion.</param>
        /// <returns>The dot product of two quaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp dot(quaternion a, quaternion b)
        {
            return dot(a.value, b.value);
        }

        /// <summary>Returns the length of a quaternion.</summary>
        /// <param name="q">The input quaternion.</param>
        /// <returns>The length of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp length(quaternion q)
        {
            return sqrt(dot(q.value, q.value));
        }

        /// <summary>Returns the squared length of a quaternion.</summary>
        /// <param name="q">The input quaternion.</param>
        /// <returns>The length squared of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp lengthsq(quaternion q)
        {
            return dot(q.value, q.value);
        }

        /// <summary>Returns a normalized version of a quaternion q by scaling it by 1 / length(q).</summary>
        /// <param name="q">The quaternion to normalize.</param>
        /// <returns>The normalized quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion normalize(quaternion q)
        {
            float4 x = q.value;
            return quaternion(rsqrt(dot(x, x)) * x);
        }

        /// <summary>
        /// Returns a safe normalized version of the q by scaling it by 1 / length(q).
        /// Returns the identity when 1 / length(q) does not produce a finite number.
        /// </summary>
        /// <param name="q">The quaternion to normalize.</param>
        /// <returns>The normalized quaternion or the identity quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion normalizesafe(quaternion q)
        {
            float4 x = q.value;
            fp len = math.dot(x, x);
            return quaternion(math.select(Mathematics.Fixed.quaternion.identity.value, x * math.rsqrt(len), len > FP_MIN_NORMAL));
        }

        /// <summary>
        /// Returns a safe normalized version of the q by scaling it by 1 / length(q).
        /// Returns the given default value when 1 / length(q) does not produce a finite number.
        /// </summary>
        /// <param name="q">The quaternion to normalize.</param>
        /// <param name="defaultvalue">The default value.</param>
        /// <returns>The normalized quaternion or the default value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion normalizesafe(quaternion q, quaternion defaultvalue)
        {
            float4 x = q.value;
            fp len = math.dot(x, x);
            return quaternion(math.select(defaultvalue.value, x * math.rsqrt(len), len > FP_MIN_NORMAL));
        }

        /// <summary>Returns the natural exponent of a quaternion. Assumes w is zero.</summary>
        /// <param name="q">The quaternion with w component equal to zero.</param>
        /// <returns>The natural exponent of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion unitexp(quaternion q)
        {
            fp v_rcp_len = rsqrt(dot(q.value.xyz, q.value.xyz));
            fp v_len = rcp(v_rcp_len);
            fp sin_v_len, cos_v_len;
            sincos(v_len, out sin_v_len, out cos_v_len);
            return quaternion(float4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len));
        }

        /// <summary>Returns the natural exponent of a quaternion.</summary>
        /// <param name="q">The quaternion.</param>
        /// <returns>The natural exponent of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion exp(quaternion q)
        {
            fp v_rcp_len = rsqrt(dot(q.value.xyz, q.value.xyz));
            fp v_len = rcp(v_rcp_len);
            fp sin_v_len, cos_v_len;
            sincos(v_len, out sin_v_len, out cos_v_len);
            return quaternion(float4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len) * exp(q.value.w));
        }

        /// <summary>Returns the natural logarithm of a unit length quaternion.</summary>
        /// <param name="q">The unit length quaternion.</param>
        /// <returns>The natural logarithm of the unit length quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion unitlog(quaternion q)
        {
            fp w = clamp(q.value.w, -fp._1, fp._1);
            fp s = acos(w) * rsqrt(fp._1 - w*w);
            return quaternion(float4(q.value.xyz * s, fp._0));
        }

        /// <summary>Returns the natural logarithm of a quaternion.</summary>
        /// <param name="q">The quaternion.</param>
        /// <returns>The natural logarithm of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion log(quaternion q)
        {
            fp v_len_sq = dot(q.value.xyz, q.value.xyz);
            fp q_len_sq = v_len_sq + q.value.w*q.value.w;

            fp s = acos(clamp(q.value.w * rsqrt(q_len_sq), -fp._1, fp._1)) * rsqrt(v_len_sq);
            return quaternion(float4(q.value.xyz * s, fp._0_50 * log(q_len_sq)));
        }

        /// <summary>Returns the result of transforming the quaternion b by the quaternion a.</summary>
        /// <param name="a">The quaternion on the left.</param>
        /// <param name="b">The quaternion on the right.</param>
        /// <returns>The result of transforming quaternion b by the quaternion a.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion mul(quaternion a, quaternion b)
        {
            return quaternion(a.value.wwww * b.value + (a.value.xyzx * b.value.wwwx + a.value.yzxy * b.value.zxyy) * float4(fp._1, fp._1, fp._1, -fp._1) - a.value.zxyz * b.value.yzxz);
        }

        /// <summary>Returns the result of transforming a vector by a quaternion.</summary>
        /// <param name="q">The quaternion transformation.</param>
        /// <param name="v">The vector to transform.</param>
        /// <returns>The transformation of vector v by quaternion q.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 mul(quaternion q, float3 v)
        {
            float3 t = 2 * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }

        /// <summary>Returns the result of rotating a vector by a unit quaternion.</summary>
        /// <param name="q">The quaternion rotation.</param>
        /// <param name="v">The vector to rotate.</param>
        /// <returns>The rotation of vector v by quaternion q.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 rotate(quaternion q, float3 v)
        {
            float3 t = 2 * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }

        /// <summary>Returns the result of a normalized linear interpolation between two quaternions q1 and a2 using an interpolation parameter t.</summary>
        /// <remarks>
        /// Prefer to use this over slerp() when you know the distance between q1 and q2 is small. This can be much
        /// higher performance due to avoiding trigonometric function evaluations that occur in slerp().
        /// </remarks>
        /// <param name="q1">The first quaternion.</param>
        /// <param name="q2">The second quaternion.</param>
        /// <param name="t">The interpolation parameter.</param>
        /// <returns>The normalized linear interpolation of two quaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion nlerp(quaternion q1, quaternion q2, fp t)
        {
            return normalize(q1.value + t * (chgsign(q2.value, dot(q1, q2)) - q1.value));
        }

        /// <summary>Returns the result of a spherical interpolation between two quaternions q1 and a2 using an interpolation parameter t.</summary>
        /// <param name="q1">The first quaternion.</param>
        /// <param name="q2">The second quaternion.</param>
        /// <param name="t">The interpolation parameter.</param>
        /// <returns>The spherical linear interpolation of two quaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion slerp(quaternion q1, quaternion q2, fp t)
        {
            fp dt = dot(q1, q2);
            if (dt < fp._0)
            {
                dt = -dt;
                q2.value = -q2.value;
            }

            if (dt < fp._0_9995)
            {
                fp angle = acos(dt);
                fp s = rsqrt(fp._1 - dt * dt);    // fp._1 / sin(angle)
                fp w1 = sin(angle * (fp._1 - t)) * s;
                fp w2 = sin(angle * t) * s;
                return quaternion(q1.value * w1 + q2.value * w2);
            }
            else
            {
                // if the angle is small, use linear interpolation
                return nlerp(q1, q2, t);
            }
        }

        /// <summary>Returns the angle in radians between two unit quaternions.</summary>
        /// <param name="q1">The first quaternion.</param>
        /// <param name="q2">The second quaternion.</param>
        /// <returns>The angle between two unit quaternions.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp angle(quaternion q1, quaternion q2)
        {
            fp diff = asin(length(normalize(mul(conjugate(q1), q2)).value.xyz));
            return diff + diff;
        }

        /// <summary>
        /// Extracts the rotation from a matrix.
        /// </summary>
        /// <remarks>This method supports any type of rotation matrix: if the matrix has a non uniform scale you should use this method.</remarks>
        /// <param name="m">Matrix to extract rotation from</param>
        /// <returns>Extracted rotation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion rotation(float3x3 m)
        {
            fp det = math.determinant(m);
            if (math.abs(fp._1 - det) < svd.k_EpsilonDeterminant)
                return math.quaternion(m);

            if (math.abs(det) > svd.k_EpsilonDeterminant)
            {
                float3x3 tmp = mulScale(m, math.rsqrt(math.float3(math.lengthsq(m.c0), math.lengthsq(m.c1), math.lengthsq(m.c2))));
                if (math.abs(fp._1 - math.determinant(tmp)) < svd.k_EpsilonDeterminant)
                    return math.quaternion(tmp);
            }

            return svd.svdRotation(m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3x3 adj(float3x3 m, out fp det)
        {
            float3x3 adjT;
            adjT.c0 = math.cross(m.c1, m.c2);
            adjT.c1 = math.cross(m.c2, m.c0);
            adjT.c2 = math.cross(m.c0, m.c1);
            det = math.dot(m.c0, adjT.c0);

            return math.transpose(adjT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool adjInverse(float3x3 m, out float3x3 i)
            => adjInverse(m, out i, svd.k_EpsilonNormal);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool adjInverse(float3x3 m, out float3x3 i, fp epsilon)
        {
            i = adj(m, out fp det);
            bool c = math.abs(det) > epsilon;
            float3 detInv = math.select(math.float3(fp._1), math.rcp(det), c);
            i = scaleMul(detInv, i);
            return c;
        }

        /// <summary>Returns a uint hash code of a quaternion.</summary>
        /// <param name="q">The quaternion to hash.</param>
        /// <returns>The hash code for the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint hash(quaternion q)
        {
            return hash(q.value);
        }

        /// <summary>
        /// Returns a uint4 vector hash code of a quaternion.
        /// When multiple elements are to be hashes together, it can more efficient to calculate and combine wide hash
        /// that are only reduced to a narrow uint hash at the very end instead of at every step.
        /// </summary>
        /// <param name="q">The quaternion to hash.</param>
        /// <returns>The uint4 vector hash code of the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 hashwide(quaternion q)
        {
            return hashwide(q.value);
        }

        /// <summary>
        /// Transforms the forward vector by a quaternion.
        /// </summary>
        /// <param name="q">The quaternion transformation.</param>
        /// <returns>The forward vector transformed by the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 forward(quaternion q) { return mul(q, float3(0, 0, 1)); }  // for compatibility
    }
}
