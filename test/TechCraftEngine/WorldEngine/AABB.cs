using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechCraftEngine.WorldEngine
{
    public class AABB
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public float? Intersects(Ray ray)
        {
            return ray.Intersects(new BoundingBox(Min, Max));
        }
    }
}
