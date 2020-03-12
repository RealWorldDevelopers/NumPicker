
namespace NumPicker
{
   public class AppSettings
   {
      public SODA SODA { get; set; }
      public MegaMillions MegaMillions { get; set; }
      public Powerball Powerball { get; set; }
      public LuckyForLife LuckyForLife { get; set; }
      public Pick3 Pick3 { get; set; }

   }

   public class SODA
   {
      public string AppToken { get; set; }
   }

   public class MegaMillions
   {
      public string HostUrl { get; set; }
      public string TableId { get; set; }
      public int MaxBallValue { get; set; }
      public int MaxMegaBallValue { get; set; }
      public int BallCount { get; set; }
   }

   public class Powerball
   {
      public string HostUrl { get; set; }
      public string TableId { get; set; }
      public int MaxBallValue { get; set; }
      public int MaxPowerBallValue { get; set; }
      public int BallCount { get; set; }
   }

   public class LuckyForLife
   {
      public string HostUrl { get; set; }
      public string TableId { get; set; }
      public int MaxBallValue { get; set; }
      public int MaxLuckyBallValue { get; set; }
      public int BallCount { get; set; }
   }

   public class Pick3
   {
      public string HostUrl { get; set; }
      public string TableId { get; set; }
      public int MaxBallValue { get; set; }
      public int BallCount { get; set; }
   }


}
