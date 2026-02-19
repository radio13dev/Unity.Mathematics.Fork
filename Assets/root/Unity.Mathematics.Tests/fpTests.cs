using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Unity.Mathematics.Fixed;

using static Unity.Mathematics.math;
using static Unity.Mathematics.Fixed.math;
using math = Unity.Mathematics.Fixed.math;

public class fpTests
{
    static readonly fp[] values = new fp[]
        {
            0, 1, 2, fp._0_10, -1, -2, fp.epsilon, fp.usable_max, fp.usable_min, //fp.max, fp.min  
        };
        
    delegate string opString(fp a, fp b);
    delegate fp fpOp(fp a, fp b);
    delegate float fOp(float a, float b);
    
    class op
    {
        public opString str;
        public fpOp fp;
        public fOp f;
        public HashSet<fp> ignoredA = new();
        public HashSet<fp> ignoredB = new();
        public HashSet<(fp a, fp b)> ignoredPair = new();
        public float Accuracy = 0.01f;
        
        public static implicit operator op((float acc, opString str, fpOp fp, fOp f) c) => new op()
        {
            Accuracy = c.acc,
            str = c.str,
            fp = c.fp,
            f = c.f
        };
        
        public static implicit operator op((opString str, fpOp fp, fOp f) c) => new op()
        {
            str = c.str,
            fp = c.fp,
            f = c.f
        };
        
        public static implicit operator op((opString str, fpOp fp, fOp f, fp[] ignoredA) c) => new op()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new HashSet<fp>(c.ignoredA)
        };
        public static implicit operator op((opString str, fpOp fp, fOp f, fp[] ignoredA, fp[] ignoredB) c) => new op()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new HashSet<fp>(c.ignoredA),
            ignoredB = new HashSet<fp>(c.ignoredB),
        };
        public static implicit operator op((opString str, fpOp fp, fOp f, fp[] ignoredA, fp[] ignoredB, (fp a,fp b)[] ignoredPair) c) => new op()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new HashSet<fp>(c.ignoredA),
            ignoredB = new HashSet<fp>(c.ignoredB),
            ignoredPair = new HashSet<(fp a,fp b)>(c.ignoredPair)
        };
        
        public static implicit operator op((float acc, opString str, fpOp fp, fOp f, fp[] ignoredA) c) => new op()
        {
            Accuracy = c.acc,
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new HashSet<fp>(c.ignoredA)
        };
    }
        
    static readonly op[] ops = new op[]
        {
            (
                (a,b) => $"{a} + {b}",
                (a,b) => a + b,
                (a,b) => a + b
            ),
            (
                (a,b) => $"{a} - {b}",
                (a,b) => a - b,
                (a,b) => a - b
            ),
            (
                (a,b) => $"{a} * {b}",
                (a,b) => a * b,
                (a,b) => a * b
            ),
            (
                (a,b) => $"{a} / {b}",
                (a,b) => a / b,
                (a,b) => a / b,
                new fp[] { },
                new fp[] { 0 }
            ),
            (
                (a,b) => $"{a} % {b}",
                (a,b) => a % b,
                (a,b) => a % b,
                new fp[] { },
                new fp[] { 0 }
            ),
            (
                (a,b) => $"math.abs({a})",
                (a,b) => math.abs(a),
                (a,b) => Unity.Mathematics.math.abs(a)
            ),
            (
                (a,b) => $"math.sqrt({a})",
                (a,b) => math.sqrt(a),
                (a,b) => Unity.Mathematics.math.sqrt(a),
                new fp[] { -1, -2, fp.usable_min }
            ),
            (
                0.05f,
                (a,b) => $"math.rsqrt({a})",
                (a,b) => math.rsqrt(a),
                (a,b) => Unity.Mathematics.math.rsqrt(a),
                new fp[] { 0, -1, -2, fp.usable_min }
            ),
            
            (
                (a,b) => $"math.log({a})",
                (a,b) => math.log(a),
                (a,b) => Unity.Mathematics.math.log(a),
                new fp[] { 0, -1, -2, fp.usable_min }
            ),
            
            (
                (a,b) => $"math.pow({a},{b})",
                (a,b) => math.pow(a,b),
                (a,b) => Unity.Mathematics.math.pow(a,b),
                new fp[] { },
                new fp[] { },
                new (fp,fp)[] { (0,-1), (0,-2), (0,fp.usable_min) }
            ),
            
            (
                0.1f,
                (a,b) => $"math.sin({a})",
                (a,b) => math.sin(a),
                (a,b) => Unity.Mathematics.math.sin(a)
            ),
            (
                0.1f,
                (a,b) => $"math.cos({a})",
                (a,b) => math.cos(a),
                (a,b) => Unity.Mathematics.math.cos(a)
            ),
            (
                0.6f,
                (a,b) => $"math.tan({a})",
                (a,b) => math.tan(a),
                (a,b) => Unity.Mathematics.math.tan(a)
            ),
            (
                (a,b) => $"math.asin({a})",
                (a,b) => math.asin(a),
                (a,b) => Unity.Mathematics.math.asin(a),
                new fp[] { 2, -2, fp.usable_max, fp.usable_min }
            ),
            (
                (a,b) => $"math.acos({a})",
                (a,b) => math.acos(a),
                (a,b) => Unity.Mathematics.math.acos(a),
                new fp[] { 2, -2, fp.usable_max, fp.usable_min }
            ),
            (
                (a,b) => $"math.atan({a})",
                (a,b) => math.atan(a),
                (a,b) => Unity.Mathematics.math.atan(a),
                new fp[] { 2, -2, fp.usable_max, fp.usable_min }
            ),
            (
                (a,b) => $"math.atan2({a},{b})",
                (a,b) => math.atan2(a,b),
                (a,b) => Unity.Mathematics.math.atan2(a,b),
                new fp[] { 2, -2, fp.usable_max, fp.usable_min },
                new fp[] { 2, -2, fp.usable_max, fp.usable_min }
            ),
        };

    [Test]
    public void fpOps_OperationAccuracy_Success()
    {
        fp test = fp.ParseUnsafe(1.44269504089f);
        
        List<string> errors = new();
    
        for (int opIndex = 0; opIndex < ops.Length; ++opIndex)
        for (int x = 0; x < values.Length; ++x)
        for (int y = 0; y < values.Length; ++y)
        {
            op op = ops[opIndex];
        
            fp fpA = values[x];
            fp fpB = values[y];
            
            if (op.ignoredA.Contains(fpA)) continue;
            if (op.ignoredB.Contains(fpB)) continue;
            if (op.ignoredPair.Contains((fpA, fpB))) continue;
            
            fp fpR = 0;
            try
            {
                fpR = op.fp(fpA, fpB);
            }
            catch (Exception e)
            {
                errors.Add($"{op.str(fpA, fpB)}:  {e}");
                continue;
            }
            
            float fA = (float)values[x];
            float fB = (float)values[y];
            float fR = op.f(fA, fB);
            
            float fDif = Unity.Mathematics.math.abs((float)fpR - fR);
            if (!(fDif < op.Accuracy))
            {
                errors.Add($"{op.str(fpA, fpB)}:  |{fpR} : {fR}| == {fDif}");
            }
        }
        
        Assert.IsTrue(errors.Count == 0, $"Errors {errors.Count}:\n{string.Join('\n',errors)}");
    }
    
    [Test]
    public void fpOps_CloseMatch_Success()
    {
        const float Accuracy = 0.001f;
        for (int x = 0; x < values.Length; ++x)
        {
            fp fpA = values[x];
            float fA = (float)values[x];
            float fDif = Unity.Mathematics.math.abs((float)fpA - fA);
            Assert.IsTrue(fDif < Accuracy, $"|{fpA} - {fA}| == {fDif}");
        }
    }
}