// Cellular noise ("Worley noise") in 2D in GLSL.
// Copyright (c) Stefan Gustavson 2011-04-19. All rights reserved.
// This code is released under the conditions of the MIT license.
// See LICENSE file for details.
// https://github.com/stegu/webgl-noise

using Deterministic.FixedPoint;
using static Unity.Mathematics.Fixed.math;

namespace Unity.Mathematics.Fixed
{
    public static partial class noise
    {
        static readonly fp K = ((fp)1)/((fp)7); // 1/7
        static readonly fp K3div7 = ((fp)3)/((fp)7); // 3/7
        static readonly fp jitter = fp._1; // Less gives more regular pattern
        
        /// <summary>
        /// 2D Cellular noise ("Worley noise") with standard 3x3 search window for good feature point values.
        /// </summary>
        /// <param name="P">A point in 2D space.</param>
        /// <returns>Feature points. F1 is in the x component, F2 in the y component.</returns>
        public static float2 cellular(float2 P)
        {

            float2 Pi = mod289(floor(P));
            float2 Pf = frac(P);
            float3 oi = float3(-fp._1, fp._0, fp._1);
            float3 of = float3(-fp._0_50, fp._0_50, fp._1_50);
            float3 px = permute(Pi.x + oi);
            float3 p = permute(px.x + Pi.y + oi); // p11, p12, p13
            float3 ox = frac(p * K) - K3div7;
            float3 oy = mod7(floor(p * K)) * K - K3div7;
            float3 dx = Pf.x + fp._0_50 + jitter * ox;
            float3 dy = Pf.y - of + jitter * oy;
            float3 d1 = dx * dx + dy * dy; // d11, d12 and d13, squared
            p = permute(px.y + Pi.y + oi); // p21, p22, p23
            ox = frac(p * K) - K3div7;
            oy = mod7(floor(p * K)) * K - K3div7;
            dx = Pf.x - fp._0_50 + jitter * ox;
            dy = Pf.y - of + jitter * oy;
            float3 d2 = dx * dx + dy * dy; // d21, d22 and d23, squared
            p = permute(px.z + Pi.y + oi); // p31, p32, p33
            ox = frac(p * K) - K3div7;
            oy = mod7(floor(p * K)) * K - K3div7;
            dx = Pf.x - fp._1_50 + jitter * ox;
            dy = Pf.y - of + jitter * oy;
            float3 d3 = dx * dx + dy * dy; // d31, d32 and d33, squared
            // Sort out the two smallest distances (F1, F2)
            float3 d1a = min(d1, d2);
            d2 = max(d1, d2); // Swap to keep candidates for F2
            d2 = min(d2, d3); // neither F1 nor F2 are now in d3
            d1 = min(d1a, d2); // F1 is now in d1
            d2 = max(d1a, d2); // Swap to keep candidates for F2
            d1.xy = (d1.x < d1.y) ? d1.xy : d1.yx; // Swap if smaller
            d1.xz = (d1.x < d1.z) ? d1.xz : d1.zx; // F1 is in d1.x
            d1.yz = min(d1.yz, d2.yz); // F2 is now not in d2.yz
            d1.y = min(d1.y, d1.z); // nor in  d1.z
            d1.y = min(d1.y, d2.x); // F2 is in d1.y, we're done.
            return sqrt(d1.xy);
        }
    }
}
