using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GVS
{
    public class NoisyGVS
    {
        private float[] valueMemory = {0,0,0,0};
        private float increment = 0.01f;
        private float max = 0;
        public void SetMaxValue(float x)
        {
            max = x;
            valueMemory[0] = max/4;
        }
        public float[] GetNextCurrents()
        {
            return GenerateRandomWalkValues();
        }
        public float[] GenerateRandomWalkValues()
        {
            float[] values = valueMemory;
            float direction = (Mathf.Sin(Time.time * Random.Range(0.9f, 1.1f) ) +1) * max /2;
            values[0] = direction;
            values[2] = -1 * values[0];
            values[1] = direction;
            values[3] = -1* values[1];
            Debug.Log(values[0]);
            return values;
        }
    }
}