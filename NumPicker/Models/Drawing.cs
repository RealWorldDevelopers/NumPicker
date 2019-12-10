using System;
using System.Collections.Generic;

namespace NumPicker.Models
{
    public class Drawing
    {
        public DateTime DrawDate { get; set; }
        public List<DrawnBall> Balls { get; set; } = new List<DrawnBall>();
    }

    public class DrawnBall
    {
        public int Id { get; set; }
        public int Seq { get; set; }
    }

}
