using System.Runtime.CompilerServices;

namespace Unity.Mathematics.Fixed {
    public partial struct fixmath {
        private static readonly fp _atan2Number1 = new fp(-883);
        private static readonly fp _atan2Number2 = new fp(3767);
        private static readonly fp _atan2Number3 = new fp(7945);
        private static readonly fp _atan2Number4 = new fp(12821);
        private static readonly fp _atan2Number5 = new fp(21822);
        private static readonly fp _atan2Number6 = new fp(65536);
        private static readonly fp _atan2Number7 = new fp(102943);
        private static readonly fp _atan2Number8 = new fp(205887);
        private static readonly fp _atanApproximatedNumber1 = new fp(16036);
        private static readonly fp _atanApproximatedNumber2 = new fp(4345);
        private static readonly byte[] _bsrLookup = {0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31};

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanReverse(uint num) {
            num |= num >> 1;
            num |= num >> 2;
            num |= num >> 4;
            num |= num >> 8;
            num |= num >> 16;
            return _bsrLookup[(num * 0x07C4ACDDU) >> 27];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeroes(uint num) {
            return num == 0 ? 32 : BitScanReverse(num) ^ 31;
        }

        /// <param name="num">Angle in radians</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Sin(fp num) {
            num.value %= Fixed.raw.pi*2;
            num       *= fp.one_div_pi2;
            var raw = fixlut.sin(num.value);
            fp result;
            result.value = raw;
            return result;
        }

        /// <param name="num">Angle in radians</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Cos(fp num) {
            num.value %= Fixed.raw.pi*2;
            num       *= fp.one_div_pi2;
            return new fp(fixlut.cos(num.value));
        }

        /// <param name="num">Angle in radians</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Tan(fp num) {
            num.value %= Fixed.raw.pi*2;
            num       *= fp.one_div_pi2;
            return new fp(fixlut.tan(num.value));
        }

        /// <param name="num">Cos [-1, 1]</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Acos(fp num) {
            return new fp(fixlut.acos(num.value));
        }

        /// <param name="num">Sin [-1, 1]</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Asin(fp num) {
            return new fp(fixlut.asin(num.value));
        }

        /// <param name="num">Tan</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Atan(fp num) {
            return Atan2(num, fp._1);
        }

        /// <param name="num">Tan [-1, 1]</param>
        /// Max error ~0.0015
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp AtanApproximated(fp num) {
            var absX = Abs(num);
            return fp.pi_quarter * num - num * (absX - fp._1) * (_atanApproximatedNumber1 + _atanApproximatedNumber2 * absX);
        }

        /// <param name="x">Denominator</param>
        /// <param name="y">Numerator</param>
        public static fp Atan2(fp y, fp x) {
            var absX = Abs(x);
            var absY = Abs(y);
            var t3   = absX;
            var t1   = absY;
            var t0   = Max(t3, t1);
            t1 = Min(t3, t1);
            t3 = fp._1 / t0;
            t3 = t1 * t3;
            var t4 = t3 * t3;
            t0 = _atan2Number1;
            t0 = t0 * t4 + _atan2Number2;
            t0 = t0 * t4 - _atan2Number3;
            t0 = t0 * t4 + _atan2Number4;
            t0 = t0 * t4 - _atan2Number5;
            t0 = t0 * t4 + _atan2Number6;
            t3 = t0 * t3;
            t3 = absY > absX ? _atan2Number7 - t3 : t3;
            t3 = x < fp._0 ? _atan2Number8 - t3 : t3;
            t3 = y < fp._0 ? -t3 : t3;
            return t3;
        }

        /// <param name="num">Angle in radians</param>
        public static void SinCos(fp num, out fp sin, out fp cos) {
            num.value %= fp.pi2.value;
            num       *= fp.one_div_pi2;
            fixlut.sin_cos(num.value, out var sinVal, out var cosVal);
            sin.value = sinVal;
            cos.value = cosVal;
        }

        public static fp Rcp(fp num) {
            //(fp.1 << 16)
            if (num.value == 0) return fp.max;
            return new fp(4294967296 / num.value);
        }
        
        public static fp Rsqrt(fp num) {
            //(fp.1 << 16)
            var sqrt = Sqrt(num);
            if (sqrt.value == 0) return fp.max;
            return new fp(4294967296 / sqrt.value);
        }

        public static fp Sqrt(fp num) {
            fp r;

            if (num.value == 0) {
                r.value = 0;
            }
            else {
                var b = (num.value >> 1) + 1L;
                var c = (b + (num.value / b)) >> 1;

                while (c < b) {
                    b = c;
                    c = (b + (num.value / b)) >> 1;
                }

                r.value = b << (fixlut.PRECISION >> 1);
            }

            return r;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Floor(fp num) {
            num.value = num.value >> fixlut.PRECISION << fixlut.PRECISION;
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Ceil(fp num) {
            var fractions = num.value & 0x000000000000FFFFL;

            if (fractions == 0) {
                return num;
            }

            num.value = num.value >> fixlut.PRECISION << fixlut.PRECISION;
            num.value += fixlut.ONE;
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Fractions(fp num) {
            return new fp(num.value & 0x000000000000FFFFL);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Truncate(fp num) {
            return new fp(num.value & ~0x000000000000FFFFL);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(fp num) {
            var fraction = num.value & 0x000000000000FFFFL;

            if (fraction >= fixlut.HALF) {
                return num.AsInt + 1;
            }

            return num.AsInt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Min(fp a, fp b) {
            return a.value < b.value ? a : b;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b) {
            return a < b ? a : b;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Min(long a, long b) {
            return a < b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Max(fp a, fp b) {
            return a.value > b.value ? a : b;
        }
                
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b) {
            return a > b ? a : b;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Max(long a, long b) {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Abs(fp num) {
            return new fp(num.value < 0 ? -num.value : num.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Clamp(fp num, fp min, fp max) {
            if (num.value < min.value) {
                return min;
            }

            if (num.value > max.value) {
                return max;
            }

            return num;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int num, int min, int max) {
            return num < min ? min : num > max ? max : num;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long num, long min, long max) {
            return num < min ? min : num > max ? max : num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Clamp01(fp num) {
            if (num.value < 0) {
                return fp._0;
            }

            return num.value > fp._1.value ? fp._1 : num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Lerp(fp from, fp to, fp t) {
            t = Clamp01(t);
            return from + (to - from) * t;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fbool Lerp(fbool from, fbool to, fp t) {
            return t.value > fp._0_50.value ? to : from;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Repeat(fp value, fp length) {
            return Clamp(value - Floor(value / length) * length, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp LerpAngle(fp from, fp to, fp t) {
            var num = Repeat(to - from, fp.pi2);
            return Lerp(from, from + (num > fp.pi ? num - fp.pi2 : num), t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp NormalizeRadians(fp angle) {
            angle.value %= fixlut.PI;
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp LerpUnclamped(fp from, fp to, fp t) {
            return from + (to - from) * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Sign(fp num) {
            return num.value < fixlut.ZERO ? fp.minus_one : fp._1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOppositeSign(fp a, fp b) {
            return ((a.value ^ b.value) < 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp SetSameSign(fp target, fp reference) {
            return IsOppositeSign(target, reference) ? target * fp.minus_one : target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static fp Pow2(int power) {
            return new fp(fixlut.ONE << power);
        }

        public static fp Exp(fp num) {
            if (num == fp._0) return fp._1;
            if (num == fp._1) return fp.e;
            if (num.value >= 2097152) return fp.max;
            if (num.value <= -786432) return fp._0;

            var neg      = num.value < 0;
            if (neg) num = -num;

            var result = num + fp._1;
            var term   = num;

            for (var i = 2; i < 30; i++) {
                term   *= num / (fp)i;
                result += term;

                if (term.value < 500 && ((i > 15) || (term.value < 20)))
                    break;
            }

            if (neg) result = fp._1 / result;

            return result;
        }
        
        public static fp Log(fp num)
        {
            if (num <= fp._0) return fp.min;
            if (num == fp._1) return fp._0;

            var value = num.value;
    
            // Find the position of the most significant bit
            int exponent = 0;
            var absValue = value;
    
            // Normalize to [1, 2) range in fixed-point (i.e., [65536, 131072) in raw values)
            var one = fixlut.ONE;  // 65536 for Q48.16
            var two = one << 1;     // 131072 for Q48.16
    
            if (absValue >= two)
            {
                while (absValue >= two)
                {
                    absValue >>= 1;
                    exponent++;
                }
            }
            else if (absValue < one)
            {
                while (absValue < one)
                {
                    absValue <<= 1;
                    exponent--;
                }
            }

            // Normalize to [1, 2) as fixed-point
            var normalized = new fp(absValue);

            // Taylor series: ln(x) = 2 * sum((x-1)/(x+1))^(2n+1) / (2n+1)
            var x_minus_1 = normalized - fp._1;
            var x_plus_1 = normalized + fp._1;
            var quotient = x_minus_1 / x_plus_1;
            var quotient_sq = quotient * quotient;

            var result = quotient;
            var term = quotient;

            for (var i = 1; i < 20; i++)
            {
                term *= quotient_sq;
                result += term / (2 * i + 1);

                if (term.value < 100)
                    break;
            }

            result *= 2;

            // Add ln(2) * exponent to account for the exponent part
            // ln(2) ≈ 0.693147180559945 in Q48.16
            var ln2 = new fp(45426L);
            result += ln2 * (fp)exponent;

            return result;
        }



        public static fp sinh(fp x) {
            // For a practical implementation, the input x must be range-reduced.
            // The range for which a polynomial or LUT is valid needs to be defined.
    
            // For small values, a polynomial approximation is efficient.
            // Coefficients for the polynomial need to be pre-calculated in Q48.16 format.
    
            // Example using Taylor series (simplified and conceptual):
            fp result = x;
            fp x2 = (x*x);
            fp term = x;
    
            // Term 2: x^3 / 3! 
            term = (term*x2);
            // Pre-calculated 3! (6.0) in Q48.16
            result += (term * fp._1div6);

            // Term 3: x^5 / 5! (120.0)
            term = (term * x2);
            // Pre-calculated 5! (120.0) in Q48.16
            fp div_120 = (fp._1 / (fp)120); // Or precomputed constant
            result += (term * div_120);

            // Add more terms for better accuracy. Range checking is crucial to prevent overflow.

            return result;
        }
        
        public static fp cosh(fp x) {
            // cosh(x) = cosh(-x), so work with positive values
            if (x < 0) x = -x;

            // Handle large values by clamping or returning max possible value if overflow is a concern
            // The range of Q48.16 is large (approx +/- 2.8e14), so overflow is unlikely in typical use
            // unless x itself is very large. Check the expected input range.

            fp result = fp._1; // Start with the first term (1)
            fp term = fp._1;
            fp x_sq = (x* x);
            int n = 1;

            // Iterate until terms become very small
            while (true) {
                // Calculate the next term: term * x^2 / ((2n)*(2n-1))
                term = (term* x_sq);
                term = (term/ (2 * n));
                term = (term/ (2 * n - 1));

                if (term == 0) break; // Term is too small to affect the result

                result += term;
                n++;
            }

            return result;
        }
    }
}