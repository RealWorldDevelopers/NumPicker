using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace NumPicker.Models
{
    public class BallViewModel
    {
        public int Ball { get; set; }
        public List<SelectListItem> BallOptions { get; set; }
    }

    public class DrawingViewModel
    {
        public DateTime NextDrawDate { get; set; }

        public List<Drawing> Predictions { get; set; }

        public List<BallViewModel> Balls { get; set; }
               
    }

}
