using UnityEngine;
using System.Collections;

static class Vector3Extension {

	public static Vector2 To2DVector(this Vector3 v3) 
	{
		return new Vector2(v3.x, v3.y);
	}

	public static Vector3 ToIntRoundedValues(this Vector3 v3)
	{
		int x, y, z;
	
		if(v3.x > 0) {
			x = (int)(v3.x + 0.5f);
		} else {
			x = (int)(v3.x - 0.5f);
		}

		if(v3.y > 0) {
			y = (int)(v3.y + 0.5f);
		} else {
			y = (int)(v3.y - 0.5f);
		}

		if(v3.z > 0) {
			z = (int)(v3.z + 0.5f);
		} else {
			z = (int)(v3.z - 0.5f);
		}

		return new Vector3(x, y, z);
	}

}
