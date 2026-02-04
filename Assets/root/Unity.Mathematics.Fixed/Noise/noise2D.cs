//
// Description : Array and textureless GLSL 2D simplex noise function.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmath.mod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
//

using Deterministic.FixedPoint;
using static Unity.Mathematics.Fixed.math;

namespace Unity.Mathematics.Fixed
{
    public static partial class noise
    {
        static readonly fp _3minusSqrt3on6 = (fp._3-fixmath.Sqrt(fp._3))/6;
        static readonly fp _0_5timesSqrt3Minus1 = fp._0_50*(fixmath.Sqrt(fp._3)-fp._1);
    
        /// <summary>
        /// Simplex noise.
        /// </summary>
        /// <param name="v">Input coordinate.</param>
        /// <returns>Noise value.</returns>
        public static fp snoise(float2 v)
        {
            float4 C = float4(_3minusSqrt3on6,  // (3.0-math.sqrt(3.0))/6.0
                _0_5timesSqrt3Minus1,  // 0.5*(math.sqrt(3.0)-1.0)
                                 -fp._1 + fp._2*_3minusSqrt3on6,  // -1.0 + 2.0 * C.x
                                  fp._1div41); // 1.0 / 41.0
            // First corner
            float2 i = floor(v + dot(v, C.yy));
            float2 x0 = v - i + dot(i, C.xx);

            // Other corners
            float2 i1;
            //i1.x = math.step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
            //i1.y = 1.0 - i1.x;
            i1 = (x0.x > x0.y) ? float2(fp._1, fp._0) : float2(fp._0, fp._1);
            // x0 = x0 - 0.0 + 0.0 * C.xx ;
            // x1 = x0 - i1 + 1.0 * C.xx ;
            // x2 = x0 - 1.0 + 2.0 * C.xx ;
            float4 x12 = x0.xyxy + C.xxzz;
            x12.xy -= i1;

            // Permutations
            i = mod289(i); // Avoid truncation effects in permutation
            float3 p = permute(permute(i.y + float3(fp._0, i1.y, fp._1)) + i.x + float3(fp._0, i1.x, fp._1));

            float3 m = max(fp._0_50 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), fp._0);
            m = m * m;
            m = m * m;

            // Gradients: 41 points uniformly over a line, mapped onto a diamond.
            // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

            float3 x = fp._2 * frac(p * C.www) - fp._1;
            float3 h = abs(x) - fp._0_50;
            float3 ox = floor(x + fp._0_50);
            float3 a0 = x - ox;

            // Normalise gradients implicitly by scaling m
            // Approximation of: m *= inversemath.sqrt( a0*a0 + h*h );
            m *= fp.snoiseA  - fp.snoiseB * (a0 * a0 + h * h);

            // Compute final noise value at P

            fp  gx = a0.x * x0.x + h.x * x0.y;
            float2 gyz = a0.yz * x12.xz + h.yz * x12.yw;
            float3 g = float3(gx,gyz);

            return fp._130 * dot(m, g);
        }
    }
}
