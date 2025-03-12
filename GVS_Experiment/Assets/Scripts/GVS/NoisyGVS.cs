using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GVS
{
    public class NoisyGVS
    {
        private float[] valueMemory = {0,0,0,0};
        private float increment = 0.001f;
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
            float direction = Random.Range(-1, 2);
            values[0] += direction * increment;
            values[2] = -values[0];
            values[1] += direction * increment;
            values[3] = - values[1];

            return values;
        }
    }
}