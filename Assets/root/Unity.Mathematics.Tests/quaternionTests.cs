using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using Unity.Mathematics.Fixed;

using static Unity.Mathematics.math;
using static Unity.Mathematics.Fixed.math;
using float3 = Unity.Mathematics.Fixed.float3;
using math = Unity.Mathematics.Fixed.math;
using quaternion = Unity.Mathematics.Fixed.quaternion;
        
    public abstract class op
    {
        public float Accuracy = 0.01f;
        
        public abstract bool Ignores(params object[] args);
    }
        
    public class op2<a, aR, b, bR> : op
    {
        public delegate string opString(a a, a b);
        public delegate aR fpOp(a a, a b);
        public delegate bR fOp(b a, b b);

        public opString str;
        public fpOp fp;
        public fOp f;
        
        public HashSet<a> ignoredA = new();
        public HashSet<a> ignoredB = new();
        public HashSet<(a a, a b)> ignoredPair = new();
        
        public static implicit operator op2<a, aR, b, bR>((float acc, opString str, fpOp fp, fOp f) c) => new op2<a, aR, b, bR>()
        {
            Accuracy = c.acc,
            str = c.str,
            fp = c.fp,
            f = c.f
        };
        
        public static implicit operator op2<a, aR, b, bR>((opString str, fpOp fp, fOp f) c) => new op2<a, aR, b, bR>()
        {
            str = c.str,
            fp = c.fp,
            f = c.f
        };
        
        public static implicit operator op2<a, aR, b, bR>((opString str, fpOp fp, fOp f, a[] ignoredA) c) => new op2<a, aR, b, bR>()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new (c.ignoredA)
        };
        public static implicit operator op2<a, aR, b, bR>((opString str, fpOp fp, fOp f, a[] ignoredA, a[] ignoredB) c) => new op2<a, aR, b, bR>()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new (c.ignoredA),
            ignoredB = new (c.ignoredB),
        };
        public static implicit operator op2<a, aR, b, bR>((opString str, fpOp fp, fOp f, a[] ignoredA, a[] ignoredB, (a a, a b)[] ignoredPair) c) => new op2<a, aR, b, bR>()
        {
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new (c.ignoredA),
            ignoredB = new (c.ignoredB),
            ignoredPair = new (c.ignoredPair)
        };
        
        public static implicit operator op2<a, aR, b, bR>((float acc, opString str, fpOp fp, fOp f, a[] ignoredA, a[] ignoredB) c) => new op2<a, aR, b, bR>()
        {
            Accuracy = c.acc,
            str = c.str,
            fp = c.fp,
            f = c.f,
            ignoredA = new (c.ignoredA),
            ignoredB = new (c.ignoredB),
        };

        public override bool Ignores(params object[] args)
        {
            a a = (a)args[0];
            a b = (a)args[1];
            
            if (ignoredA.Contains(a)) return true;
            if (ignoredB.Contains(b)) return true;
            if (ignoredPair.Contains((a,b))) return true;
            
            return false;
        }
    }
    
    public static class opExtension
    {
        public static void RunTests<a, aR, b, bR>(this op2<a, aR, b, bR>[] tests, a[] aValues, Func<a,b> aConvert, Func<aR,bR> aRConvert, Func<bR,bR,float> difFunc)
        {
            List<string> errors = new();
    
            for (int opIndex = 0; opIndex < tests.Length; ++opIndex)
            for (int x = 0; x < aValues.Length; ++x)
            for (int y = 0; y < aValues.Length; ++y)
            {
                var op = tests[opIndex];
        
                var aA = aValues[x];
                var aB = aValues[y];
            
                if (op.Ignores(aA, aB)) continue;
            
                aR aResult = default;
                try
                {
                    aResult = op.fp(aA, aB);
                }
                catch (Exception e)
                {
                    errors.Add($"{op.str(aA, aB)}:  {e}");
                    continue;
                }
            
                var bA = aConvert(aValues[x]);
                var bB = aConvert(aValues[y]);
                var bResult = op.f(bA, bB);
            
                float fDif = difFunc(aRConvert(aResult), bResult);
                if (!(fDif < op.Accuracy))
                {
                    errors.Add($"{op.str(aA, aB)}:  |{aResult} : {bResult}| == {fDif}");
                }
            }
        
            Assert.IsTrue(errors.Count == 0, $"Errors {errors.Count}:\n{string.Join('\n',errors)}");
        }
    }

public class quaternionTests
{
    static readonly quaternion[] qValues = new quaternion[]
        {
            quaternion.identity, 
            quaternion.Euler(1,0,0), quaternion.Euler(0,1,0), quaternion.Euler(0,0,1),
            quaternion.Euler(1,1,0), quaternion.Euler(1,0,1),quaternion.Euler(0,1,1),
            quaternion.Euler(1,1,1)
        };
    static readonly float3[] f3Values = new float3[]
    {
        default, 
        new float3(1,0,0), new float3(0,1,0), new float3(0,0,1),
        new float3(1,1,0), new float3(1,0,1), new float3(0,1,1),
        new float3(1,1,1),
        -new float3(1,0,0), -new float3(0,1,0), -new float3(0,0,1),
        -new float3(1,1,0), -new float3(1,0,1), -new float3(0,1,1),
        -new float3(1,1,1),
        
        new float3(fp._0_10,0,0),
        new float3(fp.epsilon,0,0),
        new float3(fp.usable_max,0,0),
        new float3(fp.usable_min,0,0),
        
        fp._0_10,
        -fp._0_10,
        fp.epsilon,
        fp.usable_max,
        fp.usable_min,
    };
        
    static readonly op2<quaternion, quaternion, Unity.Mathematics.quaternion, Unity.Mathematics.quaternion>[] q_q_To_q = new op2<quaternion, quaternion, Unity.Mathematics.quaternion, Unity.Mathematics.quaternion>[]
        {
            (
                (a,b) => $"{a} * {b}",
                (a,b) => math.mul(a, b),
                (a,b) => Unity.Mathematics.math.mul(a, b)
            ),
        };
    static readonly op2<Unity.Mathematics.Fixed.float3, quaternion, Unity.Mathematics.float3, Unity.Mathematics.quaternion>[] f3_f3_To_q = new op2<Unity.Mathematics.Fixed.float3, quaternion, Unity.Mathematics.float3, Unity.Mathematics.quaternion>[]
    {
        (
            0.15f,
            (a,b) => $"quaternion.LookRotationSafe({a}, {b})",
            (a,b) => quaternion.LookRotationSafe(a, b),
            (a,b) => Unity.Mathematics.quaternion.LookRotationSafe(a, b),
            new [] { new float3(fp.epsilon,0,0), fp.epsilon, },
            new [] { new float3(fp.epsilon,0,0), fp.epsilon, }
        ),
    };
    static readonly op2<Unity.Mathematics.Fixed.float3, Unity.Mathematics.Fixed.float3, Unity.Mathematics.float3, Unity.Mathematics.float3>[] f3_f3_To_f3 = new op2<Unity.Mathematics.Fixed.float3, Unity.Mathematics.Fixed.float3, Unity.Mathematics.float3, Unity.Mathematics.float3>[]
    {
        (
            (a,b) => $"cross({a}, {b})",
            (a,b) => cross(a, b),
            (a,b) => Unity.Mathematics.math.cross(a, b)
        ),
    };
    static readonly op2<Unity.Mathematics.Fixed.float3, fp, Unity.Mathematics.float3, float>[] f3_f3_To_fp = new op2<Unity.Mathematics.Fixed.float3, fp, Unity.Mathematics.float3, float>[]
    {
        (
            (a,b) => $"dot({a}, {b})",
            (a,b) => dot(a, b),
            (a,b) => Unity.Mathematics.math.dot(a, b)
        ),
    };

    [Test]
    public void q_q_To_qOps_OperationAccuracy_Success()
    {
        q_q_To_q.RunTests(qValues, a => (Unity.Mathematics.quaternion)a, aR => (Unity.Mathematics.quaternion)aR, (aR,bR) => Unity.Mathematics.math.angle(aR, bR));
    }
    [Test]
    public void f3_f3_To_qOps_OperationAccuracy_Success()
    {
        f3_f3_To_q.RunTests(f3Values, a => (Unity.Mathematics.float3)a, aR => (Unity.Mathematics.quaternion)aR, (aR,bR) => Unity.Mathematics.math.angle(aR, bR));
    }
    [Test]
    public void f3_f3_To_f3Ops_OperationAccuracy_Success()
    {
        f3_f3_To_f3.RunTests(f3Values, a => (Unity.Mathematics.float3)a, aR => (Unity.Mathematics.float3)aR, (aR,bR) => Unity.Mathematics.math.distance(aR, bR));
    }
    [Test]
    public void f3_f3_To_fpOps_OperationAccuracy_Success()
    {
        f3_f3_To_fp.RunTests(f3Values, a => (Unity.Mathematics.float3)a, aR => (float)aR, (aR,bR) => Unity.Mathematics.math.distance(aR, bR));
    }
    
    [Test]
    public void quaternionOps_CloseMatch_Success()
    {
        const float Accuracy = 0.001f;
        for (int x = 0; x < qValues.Length; ++x)
        {
            var aA = qValues[x];
            var bA = (Unity.Mathematics.quaternion)qValues[x];
            float fDif = Unity.Mathematics.math.angle((Unity.Mathematics.quaternion)aA, bA);
            Assert.IsTrue(fDif < Accuracy, $"|{aA} - {bA}| == {fDif}");
        }
    }
    [Test]
    public void custom()
    {
        float a = Unity.Mathematics.math.asfloat(Unity.Mathematics.math.asuint(1.0f)^0x80000000);
        float b = Unity.Mathematics.math.asfloat(Unity.Mathematics.math.asuint(-1.0f)^0x80000000);
        float c = Unity.Mathematics.math.asfloat(Unity.Mathematics.math.asuint(2.0f)^0x80000000);
        float d = Unity.Mathematics.math.asfloat(Unity.Mathematics.math.asuint(-2.0f)^0x80000000);
        float f = 0;
    }
}