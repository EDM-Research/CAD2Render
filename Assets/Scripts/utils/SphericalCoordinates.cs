//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class SphericalCoordinates
{
    public static float[] CartesianToSpherical(float x, float y, float z)
    {
        float radius = 1.0f;
        float[] retVal = new float[2];

        if (x == 0)
        {
            x = Mathf.Epsilon;
        }
        retVal[0] = Mathf.Atan(z / x);

        if (x < 0)
        {
            retVal[0] += Mathf.PI;
        }

        retVal[1] = Mathf.Asin(y / radius);

        return retVal;
    }

    public static float[] CartesianToSpherical(Vector3 v)
    {
        return CartesianToSpherical(v.x, v.y, v.z);
    }


    public static Vector3 SphericalToCartesian(float latitude, float longitude, float radius)
    {
        float a = radius * Mathf.Cos(longitude);
        float x = a * Mathf.Cos(latitude);
        float y = radius * Mathf.Sin(longitude);
        float z = a * Mathf.Sin(latitude);

        return new Vector3(x, y, z);
    }
}

