﻿namespace epvpapi.TBM
{
    public class Mediations
    {
        public Mediations(uint positive = 0, uint neutral = 0)
        {
            Positive = positive;
            Neutral = neutral;
        }

        public uint Positive { get; set; }
        public uint Neutral { get; set; }
    }
}