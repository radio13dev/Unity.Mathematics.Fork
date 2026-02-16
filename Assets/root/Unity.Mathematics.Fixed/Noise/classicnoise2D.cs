//
// GLSL textureless classic 2D noise "cnoise",
// with an RSL-style periodic variant "pnoise".
// Author:  Stefan Gustavson (stefan.gustavson@liu.se)
// Version: 2011-08-22
//
// Many thanks to Ian McEwan of Ashima Arts for the
// ideas for permutation and gradient selection.
//
// Copyright (c) 2011 Stefan Gustavson. All rights reserved.
// Distributed under the MIT license. See LICENSE file.
// https://github.com/stegu/webgl-noise
//


using static Unity.Mathematics.Fixed.math;

namespace Unity.Mathematics.Fixed
{
    public static partial class noise
    {
        /// <summary>
        /// Classic Perlin noise
        /// </summary>
        /// <param name="P">Point on a 2D grid of gradient vectors.</param>
        /// <returns>Noise value.</returns>
        public static fp cnoise(float2 P)
        {
            float4 Pi = floor(P.xyxy) + float4(fp._0, fp._0, fp._1, fp._1);
            float4 Pf = frac(P.xyxy) - float4(fp._0, fp._0, fp._1, fp._1);
            Pi = mod289(Pi); // To avoid truncation effects in permutation
            float4 ix = Pi.xzxz;
            float4 iy = Pi.yyww;
            float4 fx = Pf.xzxz;
            float4 fy = Pf.yyww;

            float4 i = permute(permute(ix) + iy);

            float4 gx = frac(i * (fp._1 / fp._41)) * fp._2 - fp._1;
            float4 gy = abs(gx) - fp._0_50;
            float4 tx = floor(gx + fp._0_50);
            gx = gx - tx;

            float2 g00 = float2(gx.x, gy.x);
            float2 g10 = float2(gx.y, gy.y);
            float2 g01 = float2(gx.z, gy.z);
            float2 g11 = float2(gx.w, gy.w);

            float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
            g00 *= norm.x;
            g01 *= norm.y;
            g10 *= norm.z;
            g11 *= norm.w;

            fp n00 = dot(g00, float2(fx.x, fy.x));
            fp n10 = dot(g10, float2(fx.y, fy.y));
            fp n01 = dot(g01, float2(fx.z, fy.z));
            fp n11 = dot(g11, float2(fx.w, fy.w));

            float2 fade_xy = fade(Pf.xy);
            float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
            fp n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
            return fp._2_3 * n_xy;
        }

        /// <summary>
        /// Classic Perlin noise, periodic variant
        /// </summary>
        /// <param name="P">Point on a 2D grid of gradient vectors.</param>
        /// <param name="rep">Period of repetition.</param>
        /// <returns>Noise value.</returns>
        public static fp pnoise(float2 P, float2 rep)
        {
            float4 Pi = floor(P.xyxy) + float4(fp._0, fp._0, fp._1, fp._1);
            float4 Pf = frac(P.xyxy) - float4(fp._0, fp._0, fp._1, fp._1);
            Pi = fmod(Pi, rep.xyxy); // To create noise with explicit period
            Pi = mod289(Pi); // To avoid truncation effects in permutation
            float4 ix = Pi.xzxz;
            float4 iy = Pi.yyww;
            float4 fx = Pf.xzxz;
            float4 fy = Pf.yyww;

            float4 i = permute(permute(ix) + iy);

            float4 gx = frac(i * (fp._1 / fp._41)) * fp._2 - fp._1;
            float4 gy = abs(gx) - fp._0_50;
            float4 tx = floor(gx + fp._0_50);
            gx = gx - tx;

            float2 g00 = float2(gx.x, gy.x);
            float2 g10 = float2(gx.y, gy.y);
            float2 g01 = float2(gx.z, gy.z);
            float2 g11 = float2(gx.w, gy.w);

            float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
            g00 *= norm.x;
            g01 *= norm.y;
            g10 *= norm.z;
            g11 *= norm.w;

            fp n00 = dot(g00, float2(fx.x, fy.x));
            fp n10 = dot(g10, float2(fx.y, fy.y));
            fp n01 = dot(g01, float2(fx.z, fy.z));
            fp n11 = dot(g11, float2(fx.w, fy.w));

            float2 fade_xy = fade(Pf.xy);
            float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
            fp n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
            return fp._2_3 * n_xy;
        }
    }
}
