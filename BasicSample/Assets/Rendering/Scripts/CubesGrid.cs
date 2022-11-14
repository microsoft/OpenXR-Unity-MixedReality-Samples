using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class CubesGrid : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            for (float z = 2f; z <= 10f; z+= 2f)
            {
                for (float x = -2.5f; x <= 2.5f; x += 1f)
                {
                    for (float y = -2f; y <= 2f; y += 1f)
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.transform.parent = gameObject.transform;

                        block.transform.position = new Vector3(x,y,z);
                        block.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    }
                }
            }
        }
    }
}
