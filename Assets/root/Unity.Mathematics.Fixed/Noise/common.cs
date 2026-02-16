using Unity.IL2CPP.CompilerServices;

using static Unity.Mathematics.Fixed.math;

namespace Unity.Mathematics.Fixed
{
    /// <summary>
    /// A static class containing noise functions.
    /// </summary>
    [Il2CppEagerStaticClassConstruction]
    public static partial class noise
    {
        // Modulo 289 without a division (only multiplications)
        static fp  mod289(fp x)  { return x - floor(x * (fp._1 / fp._289)) * fp._289; }
        static float2 mod289(float2 x) { return x - floor(x * (fp._1 / fp._289)) * fp._289; }
        static float3 mod289(float3 x) { return x - floor(x * (fp._1 / fp._289)) * fp._289; }
        static float4 mod289(float4 x) { return x - floor(x * (fp._1 / fp._289)) * fp._289; }

        // Modulo 7 without a division
        static float3 mod7(float3 x) { return x - floor(x * (fp._1 / fp._7)) * fp._7; }
        static float4 mod7(float4 x) { return x - floor(x * (fp._1 / fp._7)) * fp._7; }

        // Permutation polynomial: (34x^2 + x) math.mod 289
        static fp  permute(fp x)  { return mod289((fp._34 * x + fp._1) * x); }
        static float3 permute(float3 x) { return mod289((fp._34 * x + fp._1) * x); }
        static float4 permute(float4 x) { return mod289((fp._34 * x + fp._1) * x); }

        static fp  taylorInvSqrt(fp r)  { return fp.taylorInvSqrtA - fp.taylorInvSqrtB * r; }
        static float4 taylorInvSqrt(float4 r) { return fp.taylorInvSqrtA - fp.taylorInvSqrtB * r; }

        static float2 fade(float2 t) { return t*t*t*(t*(t*fp._6-fp._15)+fp._10); }
        static float3 fade(float3 t) { return t*t*t*(t*(t*fp._6-fp._15)+fp._10); }
        static float4 fade(float4 t) { return t*t*t*(t*(t*fp._6-fp._15)+fp._10); }

        static float4 grad4(fp j, float4 ip)
        {
            float4 ones = float4(fp._1, fp._1, fp._1, -fp._1);
            float3 pxyz = floor(frac(float3(j) * ip.xyz) * fp._7) * ip.z - fp._1;
            fp  pw   = fp._1_50 - dot(abs(pxyz), ones.xyz);
            float4 p = float4(pxyz, pw);
            float4 s = float4(p < fp._0);
            p.xyz = p.xyz + (s.xyz*fp._2 - fp._1) * s.www;
            return p;
        }

        // Hashed 2-D gradients with an extra rotation.
        // (The constant 0.0243902439 is 1/41)
        static float2 rgrad2(float2 p, fp rot)
        {
            // For more isotropic gradients, math.sin/math.cos can be used instead.
            fp u = permute(permute(p.x) + p.y) * fp._1div41 + rot; // Rotate by shift
            u = frac(u) * fp.pi2; // 2*pi
            return float2(cos(u), sin(u));
        }
    }
}
