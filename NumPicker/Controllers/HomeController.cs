using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NumPicker.Models;
using SODA;


namespace NumPicker.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings _appSettings;

        public HomeController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult MegaMillions()
        {
            // get drawing records
            var sodaHost = _appSettings.MegaMillions.HostUrl;
            var soda4x4 = _appSettings.MegaMillions.TableId;
            var sodaAppToken = _appSettings.SODA.AppToken; 

            var client = new SodaClient(sodaHost, sodaAppToken);

            // go max one year back to look at data
            var soql = new SoqlQuery().Select("draw_date", "winning_numbers", "mega_ball", "multiplier")
                        .Where($"draw_date > '{DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd")}'").Order(SoqlOrderDirection.DESC, "draw_date");

            var dataset = client.GetResource<MegaMillionsResults>(soda4x4);
            var results = dataset.Query(soql);

            // create drawing collection            
            var drawings = new List<Drawing>();
            // var ballStatList = new List<BallsStats>();

            // set drawing limits
            var maxBallValue = _appSettings.MegaMillions.MaxBallValue;
            var maxMegaBallValue = _appSettings.MegaMillions.MaxMegaBallValue;
            var ballCount = _appSettings.MegaMillions.BallCount;

            // calculate next drawing date
            var nextDrawDate = CalculateNextDrawDate(new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday });            

            // build base drawing collection
            foreach (var draw in results)
            {
                var winningNumbers = draw.winning_numbers.Split(' ');
                var drawing = new Drawing { DrawDate = DateTime.Parse(draw.draw_date) };
                foreach (var nbr in winningNumbers)
                {
                    var idx = Array.IndexOf(winningNumbers, nbr) + 1;
                    var ball = new DrawnBall
                    {
                        Id = Convert.ToInt32(nbr),
                        Seq = idx
                    };

                    drawing.Balls.Add(ball);
                }
                var megaBall = new DrawnBall { Id = Convert.ToInt32(draw.mega_ball), Seq = 6 };
                drawing.Balls.Add(megaBall);

                drawings.Add(drawing);
            }

            var ballStatsList = BuildStats(maxBallValue, ballCount, drawings, nextDrawDate, maxMegaBallValue);

            var autoPcikList = GetAutoPicks(nextDrawDate, ballStatsList, maxBallValue);

            var model = BuildViewModel(nextDrawDate, autoPcikList, ballStatsList);

            return View(model);

        }
        
        public IActionResult Powerball()
        {
            // get drawing records
            var sodaHost = _appSettings.Powerball.HostUrl;
            var soda4x4 = _appSettings.Powerball.TableId;
            var sodaAppToken = _appSettings.SODA.AppToken;

            var client = new SodaClient(sodaHost, sodaAppToken);

            // go max one year back to look at data
            var soql = new SoqlQuery().Select("draw_date", "winning_numbers", "multiplier")
                        .Where($"draw_date > '{DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd")}'").Order(SoqlOrderDirection.DESC, "draw_date");

            var dataset = client.GetResource<PowerballResults>(soda4x4);
            var results = dataset.Query(soql);

            // create drawing collection            
            var drawings = new List<Drawing>();
            //var ballStatList = new List<BallsStats>();

            // set drawing limits
            var maxBallValue = _appSettings.Powerball.MaxBallValue;
            var maxPowerBallValue = _appSettings.Powerball.MaxPowerBallValue;
            var ballCount = _appSettings.Powerball.BallCount;

            // calculate next drawing date
            var nextDrawDate = CalculateNextDrawDate(new List<DayOfWeek> { DayOfWeek.Wednesday, DayOfWeek.Saturday });            

            // build base drawing collection
            foreach (var draw in results)
            {
                var winningNumbers = draw.winning_numbers.Split(' ');
                var drawing = new Drawing { DrawDate = DateTime.Parse(draw.draw_date) };
                foreach (var nbr in winningNumbers)
                {
                    var idx = Array.IndexOf(winningNumbers, nbr) + 1;
                    var ball = new DrawnBall
                    {
                        Id = Convert.ToInt32(nbr),
                        Seq = idx
                    };

                    drawing.Balls.Add(ball);
                }

                drawings.Add(drawing);
            }

            var ballStatsList = BuildStats(maxBallValue, ballCount, drawings, nextDrawDate, maxPowerBallValue);

            var autoPcikList = GetAutoPicks(nextDrawDate, ballStatsList, maxBallValue);

            var model = BuildViewModel(nextDrawDate, autoPcikList, ballStatsList);

            return View(model);

        }
        
        public IActionResult LuckyForLife()
        {
            // get drawing records  
            XmlReader reader = XmlReader.Create(_appSettings.LuckyForLife.HostUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            var results = new List<LuckyForLifeResults>();
            foreach (SyndicationItem item in feed.Items.Where(x => x.PublishDate > DateTime.Now.AddYears(-1)).OrderByDescending(x => x.PublishDate))
            {
                var result = new LuckyForLifeResults();

                // get draw date
                var dateLineValues = item.Title.Text.Split(':');
                if (DateTime.TryParse(dateLineValues[1].Trim(), out DateTime tmpDate))
                    result.draw_date = tmpDate;

                // get winning balls
                var contentLine = ((TextSyndicationContent)item.Content).Text;
                var contentLineItems = contentLine.Split("<br />");

                var winningNumbersValues = contentLineItems[0].Split(':');
                result.winning_numbers = winningNumbersValues[1].Trim();

                var luckyBallValues = contentLineItems[1].Split(':');
                result.lucky_ball = luckyBallValues[1].Trim();

                results.Add(result);

            }

            // create drawing collection            
            var drawings = new List<Drawing>();

            // set drawing limits
            var maxBallValue = _appSettings.LuckyForLife.MaxBallValue;
            var maxLuckyBallValue = _appSettings.LuckyForLife.MaxLuckyBallValue;
            var ballCount = _appSettings.LuckyForLife.BallCount;

            // calculate next drawing date
            var nextDrawDate = CalculateNextDrawDate(new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Thursday });            

            // build base drawing collection
            foreach (var draw in results)
            {
                var winningNumbers = draw.winning_numbers.Split(',');
                var drawing = new Drawing { DrawDate = draw.draw_date.Value };
                foreach (var nbr in winningNumbers)
                {
                    var idx = Array.IndexOf(winningNumbers, nbr) + 1;
                    var ball = new DrawnBall
                    {
                        Id = Convert.ToInt32(nbr),
                        Seq = idx
                    };

                    drawing.Balls.Add(ball);
                }
                var luckyBall = new DrawnBall { Id = Convert.ToInt32(draw.lucky_ball), Seq = 6 };
                drawing.Balls.Add(luckyBall);

                drawings.Add(drawing);
            }

            var ballStatsList = BuildStats(maxBallValue, ballCount, drawings, nextDrawDate, maxLuckyBallValue);

            var autoPcikList = GetAutoPicks(nextDrawDate, ballStatsList, maxBallValue);

            var model = BuildViewModel(nextDrawDate, autoPcikList, ballStatsList);

            return View(model);

        }

        

        private List<Drawing> GetAutoPicks(DateTime nextDrawDate, List<BallStat>[] ballStatLists, int maxBallId)
        {
            var picks = new List<Drawing>();

            var ballList = ballStatLists[ballStatLists.Length - 1];
            List<BallStat>[] mainBallList = new List<BallStat>[ballStatLists.Length - 1];
            Array.Copy(ballStatLists, mainBallList, ballStatLists.Length - 1);

            // calculate ball draws
            var ballMaxScore = ballList.Max(b => b.Score);
            var ball = ballList.Where(b => b.Score == ballMaxScore).First();
            var ballBestDraw = new DrawnBall { Id = ball.Id, Seq = ball.Position };

            // Balls fall into zones (ie: ball 1 is balls 1-10 ball, 2 11-20, ...)
            var zoneDraw = new Drawing();
            zoneDraw.DrawDate = nextDrawDate;
            zoneDraw.Balls = ZoneBall(mainBallList, maxBallId);
            zoneDraw.Balls.Add(ballBestDraw);
            if (!IsPickInList(picks, zoneDraw))
                picks.Add(zoneDraw);

            // Start with Ball 1 and everyone after must be bigger
            var ascendingDraw = new Drawing();
            ascendingDraw.DrawDate = nextDrawDate;
            ascendingDraw.Balls = AscendingBall(mainBallList);
            ascendingDraw.Balls.Add(ballBestDraw);
            if (!IsPickInList(picks, ascendingDraw))
                picks.Add(ascendingDraw);

            // Reverse Logic of Ascending Picks
            var desendingDraw = new Drawing();
            desendingDraw.DrawDate = nextDrawDate;
            desendingDraw.Balls = DesendingBall(mainBallList, maxBallId);
            desendingDraw.Balls.Add(ballBestDraw);
            if (!IsPickInList(picks, desendingDraw))
                picks.Add(desendingDraw);

            // Best Ball in each list no order no repeat
            var bestBallDraw = new Drawing();
            bestBallDraw.DrawDate = nextDrawDate;
            bestBallDraw.Balls = BestBall(mainBallList);
            bestBallDraw.Balls.Add(ballBestDraw);
            if (!IsPickInList(picks, bestBallDraw))
                picks.Add(bestBallDraw);

            return picks;
        }

        private DrawingViewModel BuildViewModel(DateTime nextDrawDate, List<Drawing> autoPcikList, List<BallStat>[] ballStatsList)
        {
            var model = new DrawingViewModel
            {
                NextDrawDate = nextDrawDate,
                Predictions = autoPcikList,
                Balls = new List<BallViewModel>()
            };
            for (int i = 0; i < 6; i++)
            {
                model.Balls.Add(new BallViewModel { BallOptions = CreateSelectList("Ball", ballStatsList[i]) });
            }

            return model;
        }

        private List<BallStat>[] BuildStats(int maxBallValue, int ballCount, List<Drawing> drawings, DateTime nextDrawDate, int maxSpecialBallValue)
        {
            var ballStatList = new List<BallsStats>();

            // calculate ball stats            
            for (int ballNbr = 1; ballNbr <= maxBallValue; ballNbr++)
            {
                var positionStats = new BallsStats();
                for (int pos = 1; pos <= ballCount; pos++)
                {
                    var ball = CalcBallPositionStats(ballNbr, pos, drawings, nextDrawDate);
                    positionStats.Balls.Add(ball);
                }

                if (ballNbr <= maxSpecialBallValue)
                {
                    var ball = CalcBallPositionStats(ballNbr, ballCount + 1, drawings, nextDrawDate);
                    positionStats.Balls.Add(ball);
                }
                ballStatList.Add(positionStats);
            }

            // calculate drawing stats
            var ballStatsList = new[] { new List<BallStat>(), new List<BallStat>(), new List<BallStat>(), new List<BallStat>(), new List<BallStat>(), new List<BallStat>() };

            foreach (var stat in ballStatList)
            {
                foreach (var b in stat.Balls)
                {
                    if (b.Position == 6)
                    {
                        ballStatsList[b.Position - 1].Add(b);
                    }
                    else
                    {
                        if (b.Id >= b.Position && b.Id <= maxBallValue - (ballCount - b.Position))
                            ballStatsList[b.Position - 1].Add(b);
                    }
                }
            }
            return ballStatsList;
        }

        private DateTime CalculateNextDrawDate(List<DayOfWeek> drawDays)
        {
            var nextDrawDate = DateTime.Now;
            for (int i = 0; i < 7; i++)
            {
                nextDrawDate = nextDrawDate.AddDays(1);
                if (drawDays.Contains(nextDrawDate.DayOfWeek))
                    i = 99;
            }
            return nextDrawDate;
        }
        



        /// <summary>
        /// Check if a Draw is in a list of draws
        /// </summary>
        private bool IsPickInList(List<Drawing> currentList, Drawing newPick)
        {
            foreach (var pick in currentList)
            {
                var matchedBallCount = 0;
                foreach (var ball in newPick.Balls)
                {
                    matchedBallCount += pick.Balls.Count(b => b.Id == ball.Id);
                    var allBallsMatch = matchedBallCount == newPick.Balls.Count;
                    if (allBallsMatch)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Best Ball in each list no order no repeat
        /// </summary>
        private List<DrawnBall> BestBall(List<BallStat>[] ballStatLists)
        {
            var picks = new List<DrawnBall>();

            foreach (var stat in ballStatLists)
            {
                DrawnBall draw = null;
                var fail = false;
                var maxScore = 99999D;

                do
                {
                    fail = false;

                    var balls = stat.Where(b => b.Score < maxScore).ToList();
                    maxScore = balls.Max(b => b.Score);
                    var ball = balls.Where(b => b.Score == maxScore).First();
                    draw = new DrawnBall { Id = ball.Id, Seq = ball.Position };

                    foreach (var checkStat in ballStatLists)
                    {
                        var checkBalls = checkStat.Where(b => b.Position != ball.Position && b.Score < 99999D).ToList();
                        var checkedBall = checkBalls.Where(b => b.Id == ball.Id && b.Score > ball.Score).FirstOrDefault();
                        if (checkedBall != null)
                        {
                            fail = true;
                            maxScore -= 1;
                            break;
                        }
                    }


                } while (fail);

                picks.Add(draw);
            }

            return picks;

        }

        /// <summary>
        /// Start with Ball 1 and everyone after must be bigger
        /// </summary>
        private List<DrawnBall> AscendingBall(List<BallStat>[] ballStatLists)
        {
            var picks = new List<DrawnBall>();
            var maxScore = 99999D;
            var minBallId = 0;

            foreach (var stat in ballStatLists)
            {
                var balls = stat.Where(b => b.Score < 99999D && b.Id > minBallId).ToList();
                maxScore = balls.Max(b => b.Score);
                var ball = balls.Where(b => b.Score == maxScore).First();
                var draw = new DrawnBall { Id = ball.Id, Seq = ball.Position };
                minBallId = draw.Id;
                picks.Add(draw);
            }

            return picks;
        }

        /// <summary>
        /// Reverse Logic of Ascending Picks
        /// </summary>
        private List<DrawnBall> DesendingBall(List<BallStat>[] ballStatLists, int maxBallId)
        {
            var picks = new List<DrawnBall>();
            var maxScore = 99999D;
            var maxId = maxBallId;

            var idx = ballStatLists.Length - 1;
            for (int i = idx; i >= 0; i--)
            {
                var balls = ballStatLists[i].Where(b => b.Score < 99999D && b.Id < maxId).ToList();
                maxScore = balls.Max(b => b.Score);
                var ball = balls.Where(b => b.Score == maxScore).First();
                var draw = new DrawnBall { Id = ball.Id, Seq = ball.Position };
                maxId = draw.Id;
                picks.Add(draw);
            }

            picks.Reverse();
            return picks;
        }

        /// <summary>
        /// Balls fall into zones (ie: ball 1 is balls 1-10 ball, 2 11-20, ...)
        /// </summary>
        private List<DrawnBall> ZoneBall(List<BallStat>[] ballStatLists, int maxBallId)
        {
            var picks = new List<DrawnBall>();
            var count = ballStatLists.Count();
            var rangeSize = maxBallId / count;
            var maxScore = 99999D;
            var minId = 0;
            var maxId = rangeSize;

            foreach (var stat in ballStatLists)
            {
                var balls = stat.Where(b => b.Score < 99999D && b.Id >= minId && b.Id <= maxId).ToList();
                maxScore = balls.Max(b => b.Score);
                var ball = balls.Where(b => b.Score == maxScore).First();
                var draw = new DrawnBall { Id = ball.Id, Seq = ball.Position };
                minId = draw.Id;
                picks.Add(draw);
                minId = maxId + 1;
                maxId = minId + rangeSize;
            }

            return picks;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }





        private BallStat CalcBallPositionStats(int ballNbr, int position, List<Drawing> allDrawings, DateTime nextPickDate)
        {
            var totalDrawings = allDrawings.Count;
            var positionStats = new BallsStats();

            var snglBallDrawings = allDrawings.Where(d => d.Balls.Any(b => b.Seq == position && b.Id == ballNbr)).ToList();

            if (snglBallDrawings.Count == 0)
                return new BallStat { Position = position, Id = ballNbr, Score = 99999 };
            else
                return CalcBallScore(ballNbr, position, snglBallDrawings, totalDrawings, nextPickDate);
        }

        private BallStat CalcBallScore(int ballNbr, int position, List<Drawing> drawingsByPosition, int totalDrawings, DateTime nextPickDate)
        {
            var ballStat = new BallStat
            {
                Id = ballNbr,
                Position = position
            };

            // date last hit
            var positionLastHit = drawingsByPosition.Select(d => d.DrawDate).Max();
            if (positionLastHit > ballStat.LastHit)
                ballStat.LastHit = positionLastHit;

            // how often does number come up
            var dueInDrawingsCount = totalDrawings / drawingsByPosition.Count;

            // set begging score.  More often number comes up the better the score
            ballStat.Score = Convert.ToInt32(drawingsByPosition.Count / Convert.ToDouble(totalDrawings) * 100);

            // determine next due date
            var dueDraw = ballStat.LastHit;
            for (int ii = 1; ii < dueInDrawingsCount; ii++)
            {
                if (dueDraw.DayOfWeek == DayOfWeek.Friday)
                    dueDraw = dueDraw.AddDays(4);
                else
                    dueDraw = dueDraw.AddDays(3);
            }

            ballStat.Score += Convert.ToInt32((nextPickDate - dueDraw).TotalDays);

            return ballStat;
        }


        private List<SelectListItem> CreateSelectList(string title, List<BallStat> balls)
        {
            var list = new List<SelectListItem>();

            var selectItem = new SelectListItem
            {
                Value = "",
                Text = "Select a " + title,
                Disabled = true,
                Selected = true
            };
            list.Add(selectItem);

            if (balls != null)
            {
                foreach (var ball in balls.OrderBy(b => b.Score))
                {
                    selectItem = new SelectListItem
                    {
                        Value = ball.Id.ToString(),
                        Text = $"{ball.Id.ToString("00")} ({ball.Score.ToString("#####.##")})"
                    };
                    list.Add(selectItem);
                }
            }

            return list;
        }

        private bool ValidatePicks(List<int> drawBalls, int maxDrawBallValue, int? soloBall = null, int soloBallMaxValue = 0)
        {
            // pass or fail draw balls are within range
            if (drawBalls.Any(b => b > maxDrawBallValue || b < 1))
                return false;

            // pass or fail draw balls has no repeats
            var previousBall = 0;
            drawBalls.Sort();
            foreach (var ball in drawBalls)
            {
                if (ball > previousBall)
                    previousBall = ball;
                else
                    return false;
            }

            // pass or fail solo ball
            if (!soloBall.HasValue)
                return true;

            return soloBall > 0 && soloBall <= soloBallMaxValue;

        }

    }
}
