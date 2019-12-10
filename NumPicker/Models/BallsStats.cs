using System;
using System.Collections.Generic;

namespace NumPicker.Models
{
    public class BallsStats
    {
        public List<BallStat> Balls { get; set; } = new List<BallStat>();
    }

    public class BallStat
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public double Score { get; set; }
        public DateTime LastHit { get; set; } = new DateTime(1990, 1, 1);
    }

   
}
