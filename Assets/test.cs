using System;
using Unity.Mathematics.Fixed;
using UnityEngine;

public class test : MonoBehaviour
{
    public fp fpval;
    public fp2 fp2val;
    public float2 float2val;
    public float2x2 float2x2val;

    public long minVal;
    public long minValPow2;
    public long minValPow3;
    public long minValPow4;
    
    public fp minValfp;
    public fp minValPow2fp;
    public fp minValPow3fp;
    public fp minValPow4fp;
    
    public long taylorInvSqrtA;
    public long taylorInvSqrtB;
    

    private void OnValidate()
    {
        minVal = fp.epsilon.value;
        minValPow2 = fixmath.Sqrt(minValfp = fp.ParseRaw(minVal)).value;
        minValPow3 = fixmath.Sqrt(minValPow2fp = fp.ParseRaw(minValPow2)).value;
        minValPow4 = fixmath.Sqrt(minValPow3fp = fp.ParseRaw(minValPow3)).value;
        minValPow4fp = fp.ParseRaw(minValPow4);
        
        taylorInvSqrtA = fp.taylorInvSqrtA.value;
        taylorInvSqrtB = fp.taylorInvSqrtB.value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
