//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inspired by: https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBase/Utils/RandomUtil.cs

public class RandomNumberGenerator
{
    private System.Random rand;

    public RandomNumberGenerator(int seed = 0)
    {
        rand = new System.Random(seed);
    }

    /***
     * gives a random double between ]0-1[
     */
    public double NextDouble()
    {
        return rand.NextDouble();
    }

    /***
     * gives a random float between ]0-1[
     * equivalent to (float)NextDouble()
     */
    public float Next()
    {
        return (float)rand.NextDouble();
    }

    /***
     * randomly returns 1 or -1
     */
    public int RandomSign()
    {
        return Next() < .5 ? 1 : -1;
    }

    /***
     * returns a float in between minNumber and maxNumber
     * range(min, max) == range(max, min)
     */
    public float Range(float minNumber = 0.0f, float maxNumber = 1.0f)
    {
        return this.Next() * (maxNumber - minNumber) + minNumber;
    }

    /***
     * returns an int in the range [min, max[
     * range(min, max) != range(max, min)
     */
    public int IntRange(int low, int high)
    {
        return (int)(rand.NextDouble() * (high - low)) + low;
    }

    /***
     * returns a float in between minAngle and maxAngle (output is same unit as input)
     * equivalent to range(minAngle, maxAngle)
     */
    public float Angle(float minAngle = 0.0f, float maxAngle = 360.0f)
    {
        return this.Next() * (maxAngle - minAngle) + minAngle;
    }


    /***
     * returns a float in between 0 and 2pi
     */
    public float Radian()
    {
        return this.Next() * 2.0f * Mathf.PI;
    }


    /***
     * returns a uniform sampled point on the unit sphere
     */
    public UnityEngine.Vector3 OnUnitSphere()
    {
        //uniform, using angles
        var a = Radian();
        var b = Radian();
        var sa = Mathf.Sin(a);
        return new Vector3(sa * Mathf.Cos(b), sa * Mathf.Sin(b), Mathf.Cos(a));

        //non-uniform, needs to test for 0 vector
        /*
        var v = new UnityEngine.Vector3(Value, Value, Value);
        return (v == UnityEngine.Vector3.zero) ? UnityEngine.Vector3.right : v.normalized;
            */
    }


    /***
     * returns random rotation
     */
    public UnityEngine.Quaternion Rotation()
    {
        return UnityEngine.Quaternion.AngleAxis(this.Angle(), this.OnUnitSphere());
    }


}
