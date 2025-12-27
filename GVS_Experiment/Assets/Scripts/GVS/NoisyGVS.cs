using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GVS
{
    public class NoisyGVS
    {
        private float[] valueMemory = {0,0,0,0};
        private float max = 0;

        private float interpolator = 0;
        private float speedModifier = 1;

        public float SpeedModifier { get => speedModifier; set => speedModifier = value; }
        public float Interpolator { get => interpolator; set => interpolator = value; }

        public void SetMaxValue(float x)
        {
            max = x;
            valueMemory[0] = max/4;
        }
        public float[] GetNextCurrents()
        {
            return GenerateSemiRandomSinSignal();
        }
        public float[] GenerateSemiRandomSinSignal()
        {
            float[] values = valueMemory;
            float direction = (Mathf.Sin(Time.time * Random.Range(0.9f, 1.1f) ) +1) * Interpolator * max /2;
            values[0] = direction;
            values[2] = -1 * values[0];
            values[1] = direction;
            values[3] = -1* values[1];
            Debug.Log(values[0]);
            return values;
        }

        public float ActivateNoisyGVS(float increment)
        {
            //Interpolate the interpolator between 0 and 1
            Interpolator += increment;
            if(Interpolator >= 1)
            {
                Interpolator = 1;
            }
            return Interpolator;
        }

        public float DectivateNoisyGVS(float decrement)
        {
            //Interpolate the interpolator between 1 and 0
            Interpolator -= decrement;
            if (Interpolator <= 0)
            {
                Interpolator = 0;
            }
            return Interpolator;
        }

    }
}