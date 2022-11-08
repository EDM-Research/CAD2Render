using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
	[Serializable]
	public class RFFlash
	{
		[Header ("  Intensity")]
		[Space (3)]
		
		[Range(0.1f,  5f)] public float intensityMin;
		[Space (1)]
		[Range(0.1f, 5f)]  public float intensityMax;
		
		[Header ("  Range")]
		[Space (3)]
		
		[Range(0.01f, 10f)] public float rangeMin;
		[Space (1)]
		[Range(0.01f, 10f)] public float rangeMax;
		
		[Header ("  Other")]
		[Space (3)]
		
		[Range(0.01f, 2f)]  public float distance;
		[Space (1)]
		public Color color;

		// Constructor
		public RFFlash()
		{
			intensityMin = 0.5f;
			intensityMax = 0.7f;
			rangeMin     = 5f;
			rangeMax     = 7f;
			distance     = 0.4f;
			color        = new Color (1f, 1f, 0.8f);
		}
	}
	
	[Serializable]
	public class RFDecals
	{
		public                     bool  enable;
		
        [Header ("  Size")]
        [Space (2)]
		
		[Range(0.1f,  5f)]  public float sizeMin;
        [Space (1)]
		[Range(0.1f,  5f)]  public float sizeMax;
		
        
        [Header ("  Limitations")]
        [Space (2)]
        
        
		
		[Range(0.01f, 2f)]  public float distance;
		
		
		
		// mats
		// Duration
		// Max amount
		

		// Constructor
		public RFDecals()
		{
			enable    = true;
			distance  = 0.4f;
			
		}
	}
}