using UnityEngine;

namespace Verse
{
	public class RememberedCameraPos : IExposable
	{
		public Vector3 rootPos;

		public float rootSize;

		public RememberedCameraPos(Map map)
		{
			this.rootPos = map.Center.ToVector3Shifted();
			this.rootSize = 24f;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<Vector3>(ref this.rootPos, "rootPos", default(Vector3), false);
			Scribe_Values.Look<float>(ref this.rootSize, "rootSize", 0f, false);
		}
	}
}
