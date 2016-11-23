using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Pong
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalValues.DiscreteBoardX = 12;
            GlobalValues.DiscreteBoardY = 12;
            GlobalValues.InitialBallX = 0.5;
            GlobalValues.InitialBallY = 0.5;
            GlobalValues.InitialVelocityX = 0.03;
            GlobalValues.InitialVelocityY = 0.01;
            GlobalValues.PaddleHeight = 0.2;
            GlobalValues.RandomVelocityX = 0.015;
            GlobalValues.RandomVelocityY = 0.03;

            double boardX = 1.0;
            double boardY = 1.0;
            //double ball_x = 0.5;
            //double ball_y = 0.5;
            //double vel_x = 0.03;
            //double vel_y = 0.01;
            //double paddleHeight = 0.2;
            //double randomVelX = 0.015;
            //double randomVelY = 0.03;

            int totalTrials = 100000;
            double learningConstant = 5;
            double discountFactor = .9;
            int explorationLimit = 30000;

            Ball initialBall = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, false);
            Player rP = new Player(1, 0.5 - (GlobalValues.PaddleHeight / 2), learningConstant, discountFactor, Tuple.Create(initialBall.DiscreteBallX, initialBall.DiscreteBallY, initialBall.DiscreteVelocityX, initialBall.DiscreteVelocityY, (int)Math.Floor(GlobalValues.DiscreteBoardY * (0.5 - (GlobalValues.PaddleHeight / 2)) / (1 - GlobalValues.PaddleHeight))), GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, explorationLimit);

            int maxDeflections = 0;

            DateTime start = DateTime.Now;
            for (int numTrial = 0; numTrial <= totalTrials; numTrial++)
            {            
                // initial state of the ball
                Ball b = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, false);
                rP.PaddleY = 0.5 - (GlobalValues.PaddleHeight / 2);
                rP.DiscretePaddleY = rP.getDiscretePaddleY();
                rP.PrevAction = "N/A";

                var currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);
                var currentDiscreteTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, rP.DiscretePaddleY);
                var prevStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                
                // Epsiode/Trial Loop
                int trialIterations = 0;
                rP.addGamesPlayed();
                while (true)
                {                                        
                    trialIterations++;
                    if (goalState(currentStateTuple, rP, null))
                    {
                        if (deflectionDetected(currentStateTuple, prevStateTuple, rP, null, b))
                        {
                            if (rP.Deflections >= 9)
                            {
                                Console.WriteLine("HERE");
                            }
                            Random rand = new Random();

                            b.BallX = 2 * rP.PaddleX - b.BallX;
                            b.VelocityX = b.VelocityX * -1 + rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX);
                            b.VelocityY = b.VelocityY + rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY);

                            if (Math.Abs(b.VelocityX) < 0.03)
                            {                                                                
                                if (b.VelocityX < 0)
                                {
                                    b.VelocityX = 0.03;
                                    b.VelocityX = b.VelocityX * -1;
                                }
                                else
                                {
                                    b.VelocityX = 0.03;
                                }
                                
                            }
                        }
                        else
                        {
                            if (rP.Deflections >= 9)
                            {
                                Console.WriteLine("HERE");
                            }
                            rP.addGamesLost();                            
                            //rP.updateQ(b, rP.PrevAction, rP, null, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, GlobalValues.PaddleHeight, GlobalValues.LearningRate, GlobalValues.DiscountFactor, int stateReward);
                            break;
                        }
                    }
                    // Draw Board
                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numTrial);
                    // Move Ball
                    b.MoveBall();
                    //DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numTrial);
                    // Right Player Makes Decision
                    rP.MoveRightPaddle(b);

                    //DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numTrial);
                    // Add new State
                    prevStateTuple = currentStateTuple;
                    currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                }
                if (rP.Deflections > maxDeflections)
                {
                    maxDeflections = rP.Deflections;
                }                 
                Console.WriteLine("Number of deflections: " + rP.Deflections);
                Console.WriteLine("Trial: " + numTrial);
                Console.WriteLine("Max number of deflections: " + maxDeflections);
                rP.Deflections = 0;
            }


            DateTime end = DateTime.Now;
            Console.WriteLine("*************SUMMARY:*************");
            Console.WriteLine("Training start: " + start);
            Console.WriteLine("Training end: " + end);
            Console.WriteLine("Training duration: " + (end - start));
            do
            {
                
                Console.WriteLine("Press q to quit");
            } while (Console.ReadKey().KeyChar != 'q');

        }

        static bool goalState(Tuple<double, double, double, double, double> currState, Player rP, Player lP)
        {
            // ball past 
            if (currState.Item1 > rP.PaddleX)
            {
                //rP.addGamesLost();
                return true;
            }

            if (lP != null)
            {
                if (currState.Item1 < lP.PaddleX)
                {
                    //lP.addGamesLost();
                    return true;
                }

            }

            return false;
        }

        static bool deflectionDetected(Tuple<double, double, double, double, double> currState, Tuple<double, double, double, double, double> prevState, Player rP, Player lP, Ball b)
        {            
            // right player deflects it
            if (prevState.Item1 < rP.PaddleX && currState.Item1 >= rP.PaddleX)
            {
                // Calculate Y coordinate when ball is at the same level as the paddle
                double yIntersect = currState.Item4 * prevState.Item1 + prevState.Item2;
             
                if (yIntersect <= rP.PaddleY + GlobalValues.PaddleHeight && yIntersect >= rP.PaddleY)
                {
                    rP.addDeflection();

                    // HERE                   
                    //rP.updateQ(b, rP.PrevAction, rP, lP, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, GlobalValues.PaddleHeight, GlobalValues.LearningRate, GlobalValues.DiscountFactor);
                    return true;
                }
            }

            if (lP != null)
            {
                // left player deflects it
                if (prevState.Item1 < rP.PaddleX && currState.Item1 >= lP.PaddleX)
                {
                    // Calculate Y coordinate when ball is at the same level as the paddle
                    double yIntersect = currState.Item4 * prevState.Item1 + prevState.Item2;

                    if (yIntersect >= rP.PaddleY - 0.2 && yIntersect <= lP.PaddleY)
                    {
                        lP.addDeflection();
                        return true;
                    }
                }
            }
            return false;
        }

        static void DrawPong(int boardX, int boardY, Player lPlayer, Player rPlayer, Ball b, int numTrial)
        {
            Console.Clear();            
            
                //for (int y = 0; y < boardY + 2; y++)
                for (int y = 0; y < boardY; y++)
                {
                    //if (y == 0 || y == boardX + 2)
                    //{
                    //    Console.Write("-");
                    //}
                    //for (int x = 0; x < boardX + 2; x++)
                    for (int x = 0; x < boardX; x++)
                    {
                        if (lPlayer == null)
                        {
                            //if (y == 0 || y == boardY + 1)
                            //{
                            //    Console.Write("-");
                            //}
                            //else
                            //{
                            //    if (x == 0)
                            //    {
                            //        Console.Write("|");
                            //    }
                            //}

                        }
                        else
                        {
                            if (x == lPlayer.DiscretePaddleX && y == lPlayer.DiscretePaddleY)
                            {
                                Console.Write("|");
                            }
                        }

                        //if (x == b.DiscreteBallX - 1 && y == b.DiscreteBallY && x == rPlayer.DiscretePaddleX - 1 && y == rPlayer.DiscretePaddleY)
                        if (x == b.DiscreteBallX-1 && y == b.DiscreteBallY && x == rPlayer.DiscretePaddleX-1 && y == rPlayer.DiscretePaddleY)
                        {
                            Console.Write("X");
                        }
                        //else if (x == rPlayer.DiscretePaddleX-1 && y == rPlayer.DiscretePaddleY)
                        else if (x == rPlayer.DiscretePaddleX-1 && y == rPlayer.DiscretePaddleY)
                        {
                            Console.Write("|");
                        }
                        //else if (x == b.DiscreteBallX-1 && y == b.DiscreteBallY)
                        else if (x == b.DiscreteBallX-1 && y == b.DiscreteBallY)
                        {
                            Console.Write("*");
                        }
                        else
                        {
                            //if (y != 0 && y != boardY + 1)
                            //{
                            //    Console.Write(" ");
                            //}

                            Console.Write(" ");
                        }
                    }
                    Console.WriteLine("");
                }
            Console.WriteLine("------------");
            Console.WriteLine("Trial #: " + numTrial);
            Console.WriteLine("Number of Deflections: " + rPlayer.Deflections);
        }

    }
}
