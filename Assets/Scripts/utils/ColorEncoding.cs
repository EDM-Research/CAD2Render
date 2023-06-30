//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using UnityEngine;




public class ColorEncoding {

    private static Color[] colors = {  new Color32(255,0,0, 255),
                                new Color32(0,255,0, 255),
                                new Color32(0,0,255, 255),
                                new Color32(255,255,0, 255),
                                new Color32(0,255,255, 255),
                                new Color32(255,0,255, 255),
                                new Color32(192,192,192, 255),
                                new Color32(128,128,128, 255),
                                new Color32(128,0,0, 255),
                                new Color32(128,128,0, 255),
                                new Color32(0,128,0, 255),
                                new Color32(128,0,128, 255),
                                new Color32(0,128,128, 255),
                                new Color32(0,0,128, 255),
                                new Color32(165,42,42, 255),
                                new Color32(220,20,60, 255),
                                new Color32(255,127,80, 255),
                                new Color32(205,92,92, 255),
                                new Color32(255,69,0, 255),
                                new Color32(255,140,0, 255),
                                new Color32(255,215,0, 255),
                                new Color32(184,134,11, 255),
                                new Color32(218,165,32, 255),
                                new Color32(238,232,170, 255),
                                new Color32(189,183,107, 255),
                                new Color32(154,205,50, 255),
                                new Color32(85,107,47, 255),
                                new Color32(107,142,35, 255),
                                new Color32(173,255,47, 255),
                                new Color32(0,100,0, 255),
                                new Color32(34,139,34, 255),
                                new Color32(50,205,50, 255),
                                new Color32(144,238,144, 255),
                                new Color32(143,188,143, 255),
                                new Color32(0,250,154, 255),
                                new Color32(102,205,170, 255),
                                new Color32(32,178,170, 255),
                                new Color32(47,79,79, 255),
                                new Color32(64,224,208, 255),
                                new Color32(175,238,238, 255),
                                new Color32(176,224,230, 255),
                                new Color32(95,158,160, 255),
                                new Color32(70,130,180, 255),
                                new Color32(100,149,237, 255),
                                new Color32(30,144,255, 255),
                                new Color32(173,216,230, 255),
                                new Color32(135,206,235, 255),
                                new Color32(25,25,112, 255),
                                new Color32(65,105,225, 255),
                                new Color32(138,43,226, 255),
                                new Color32(75,0,130, 255),
                                new Color32(72,61,139, 255),
                                new Color32(123,104,238, 255),
                                new Color32(147,112,219, 255),
                                new Color32(139,0,139, 255),
                                new Color32(153,50,204, 255),
                                new Color32(216,191,216, 255),
                                new Color32(238,130,238, 255),
                                new Color32(218,112,214, 255),
                                new Color32(255,20,147, 255),
                                new Color32(255,105,180, 255),
                                new Color32(255,228,196, 255),
                                new Color32(255,250,205, 255),
                                new Color32(139,69,19, 255),
                                new Color32(160,82,45, 255),
                                new Color32(205,133,63, 255),
                                new Color32(244,164,96, 255),
                                new Color32(188,143,143, 255),
                                new Color32(255,240,245, 255),
                                new Color32(112,128,144 , 255),
                                new Color32(240,255,240, 255),
                                new Color32(240,248,255, 255),
                                new Color32(105,105,105, 255)};

    public static byte ReverseBits(byte value) {
        return (byte)((value * 0x0202020202 & 0x010884422010) % 1023);
    }

    public static int SparsifyBits(byte value, int sparse) {
        int retVal = 0;
        for (int bits = 0; bits < 8; bits++, value >>= 1) {
            retVal |= (value & 1);
            retVal <<= sparse;
        }
        return retVal >> sparse;
    }

    public static Color GetColorByIndex(int index)
    {
        if (index >= colors.Length)
        {
            Debug.Log("WARNING EncodeColorByIndex: index exceeds color array.");
        }
        return colors[index % colors.Length];
    }

    private static int globalColorIndex = -1;
    public static void resetGlobalColorIndex() { globalColorIndex = -1; }
    public static int NextGlobalColorIndex() { return ++globalColorIndex; }

    public static Color EncodeIDAsColor(int instanceId) {
        var uid = instanceId * 2;
        if (uid < 0)
            uid = -uid + 1;

        var sid =
            (SparsifyBits((byte)(uid >> 16), 3) << 2) |
            (SparsifyBits((byte)(uid >> 8), 3) << 1) |
             SparsifyBits((byte)(uid), 3);
        //Debug.Log(uid + " >>> " + System.Convert.ToString(sid, 2).PadLeft(24, '0'));

        var r = (byte)(sid >> 8);
        var g = (byte)(sid >> 16);
        var b = (byte)(sid);

        //Debug.Log(r + " " + g + " " + b);
        return new Color32(r, g, b, 255);
    }

    public static Color EncodeNameAsColor(string tag) {
        var hash = tag.GetHashCode();
        var r = (byte)(hash >> 16);
        var g = (byte)(hash >> 8);
        var b = (byte)(hash);
        var a = (byte)255;
        return new Color32(r, g, b, a);
    }


    public static Color EncodeTagAsColor(string tag)
    {
        var hash = tag.GetHashCode();
        var a = (byte)(hash >> 24);
        var r = (byte)(hash >> 16);
        var g = (byte)(hash >> 8);
        var b = (byte)(hash);
        return new Color32(r, g, b, a);
    }


    public static Color EncodeLayerAsColor(int layer, bool grayscale) {
        if (grayscale) {
            return new Color(layer / 255.0f, layer / 255.0f, layer / 255.0f);
        }
        else {
            // Following value must be in the range (0.5 .. 1.0)
            // in order to avoid color overlaps when using 'divider' in this func
            var z = .7f;

            // First 8 layers are Unity Builtin layers
            // Unity supports up to 32 layers in total

            // Lets create palette of unique 16 colors
            var uniqueColors = new Color[] {
            new Color(1,1,1,1), new Color(z,z,z,1),						// 0
            new Color(1,1,z,1), new Color(1,z,1,1), new Color(z,1,1,1), // 
            new Color(1,z,0,1), new Color(z,0,1,1), new Color(0,1,z,1), // 7
            
            new Color(1,0,0,1), new Color(0,1,0,1), new Color(0,0,1,1), // 8
            new Color(1,1,0,1), new Color(1,0,1,1), new Color(0,1,1,1), // 
            new Color(1,z,z,1), new Color(z,1,z,1)						// 15
        };

            // Create as many colors as necessary by using base 16 color palette
            // To create more than 16 - will simply adjust brightness with 'divider'
            var color = uniqueColors[layer % uniqueColors.Length];
            var divider = 1.0f + Mathf.Floor(layer / uniqueColors.Length);
            color /= divider;

            return color;
        }

    }
}
