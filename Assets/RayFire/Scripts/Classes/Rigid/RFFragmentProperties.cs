using System;

namespace RayFire
{
	[Serializable]
	public class RFFragmentProperties
	{
		public RFColliderType colliderType;
		public float          sizeFilter;
		public bool           decompose;
		public bool           removeCollinear;
		public bool           l; // Copy layer
		public int            m; // Copy layer
		public int            layer;
		public bool           t; // Copy tag
		public string         tag;

		/// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
		
		// Constructor
		public RFFragmentProperties()
		{
			colliderType    = RFColliderType.Mesh;
			sizeFilter      = 0;
			decompose       = false;
			removeCollinear = false;
			l               = true;
			layer           = 0;
			t               = true;
			tag             = "";
		}

		// Copy from
		public void CopyFrom (RFFragmentProperties fragmentProperties)
		{
			colliderType    = fragmentProperties.colliderType;
			sizeFilter      = fragmentProperties.sizeFilter;
			decompose       = false;
			removeCollinear = fragmentProperties.removeCollinear;
			l               = fragmentProperties.l;
			layer           = fragmentProperties.layer;
			t               = fragmentProperties.t;
			tag             = fragmentProperties.tag;
		}
		
		/// /////////////////////////////////////////////////////////
		/// Layer & Tag
		/// /////////////////////////////////////////////////////////
        
		// Get layer for fragments
		public int GetLayer (RayfireRigid scr)
		{
			// Inherit layer
			if (scr.meshDemolition.properties.l == true)
				return scr.gameObject.layer;

			// Get custom layer
			return layer;
		}
        
		// Get tag for fragments
		public string GetTag (RayfireRigid scr)
		{
			// Inherit tag
			if (scr.meshDemolition.properties.t == true)
				return scr.gameObject.tag;
            
			// Set tag. Not defined -> Untagged
			if (tag.Length == 0)
				return "Untagged";
            
			// Set tag.
			return tag;
		}
	}
}