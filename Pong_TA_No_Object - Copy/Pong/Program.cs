using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

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
            Random rand = new Random();

            double boardX = 1.0;
            double boardY = 1.0;
            //double ball_x = 0.5;
            //double ball_y = 0.5;
            //double vel_x = 0.03;
            //double vel_y = 0.01;
            //double paddleHeight = 0.2;
            //double randomVelX = 0.015;
            //double randomVelY = 0.03;

            int totalTrials = 600000;
            double learningConstant = 30; //30
            double discountFactor = .9;  //.4
            int explorationLimit = 5; //300

            Ball initialBall = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, false);
            Player rP = new Player(1, 0.5 - (GlobalValues.PaddleHeight / 2), learningConstant, discountFactor, Tuple.Create(initialBall.DiscreteBallX, initialBall.DiscreteBallY, initialBall.DiscreteVelocityX, initialBall.DiscreteVelocityY, (int)Math.Floor(GlobalValues.DiscreteBoardY * (0.5 - (GlobalValues.PaddleHeight / 2)) / (1 - GlobalValues.PaddleHeight))), GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, explorationLimit);

            rP.Memory = ReadQ(@"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\Q_J.txt");
            //rP.MemoryVisited = ReadN(@"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\N.txt");

            int maxDeflections = 0;
            int totalTrialLength = 0;
            List<int> last10 = new List<int>();


            DateTime start = DateTime.Now;
            for (int numTrial = 0; numTrial < totalTrials; numTrial++)
            {
                //Write(rP.Memory, @"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\Pong_TA_No_Object\Pong\Q.txt", numTrial, rP.TotalTrialDeflections, rP.GamesPlayed);
                Write(rP.Memory, @"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\Pong_TA_No_Object - Copy\Pong\Q.txt");
                Write(rP.MemoryVisited, @"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\Pong_TA_No_Object - Copy\Pong\N.txt");

                // initial state of the game
                int stateReward = 0;
                string stateAction = "None";
                GamesState currGS = new GamesState(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, 1.0, 0.5 - (GlobalValues.PaddleHeight / 2), stateReward, stateAction);
                //GamesState prevGS = new GamesState(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, 1.0, 0.5 - (GlobalValues.PaddleHeight / 2), stateReward, stateAction);                

                // Epsiode/Trial Loop
                int trialIterations = 0;
                rP.addGamesPlayed();
                while (true)
                {                                        
                    trialIterations++;

                    int discreteBallX = (Int32)Math.Floor(GlobalValues.DiscreteBoardX * (currGS.BallX - 0) / (1 - 0));
                    int discreteBallY = (Int32)Math.Floor(GlobalValues.DiscreteBoardY * (currGS.BallY - 0) / (1 - 0));

                    int discreteVelocityX = 0;
                    if (currGS.BallVelocityX >= 0)
                    {
                        discreteVelocityX = 1;
                    }
                    if (currGS.BallVelocityX < 0)
                    {
                        discreteVelocityX = -1;
                    }

                    int discreteVelocityY = 0;
                    if (Math.Abs(currGS.BallVelocityY) < 0.015)
                    {
                        discreteVelocityY = 0;
                    }
                    else if (currGS.BallVelocityY >= 0)
                    {
                        discreteVelocityY = 1;
                    }
                    else
                    {
                        discreteVelocityY = -1;
                    }                    

                    int discretePaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardX * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);

                    // Q(s,a)
                    var discreteStateTuple = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discretePaddleY, currGS.StateAction);
                    double currQVal = 0.0;
                    try
                    {
                        rP.Memory.TryGetValue(discreteStateTuple, out currQVal);
                    }
                    catch
                    {
                        ;
                    }

                    // Update N
                    try
                    {
                        rP.MemoryVisited.Add(discreteStateTuple, 1);
                    }
                    catch
                    {
                        int tmpCount = rP.MemoryVisited[discreteStateTuple];
                        rP.MemoryVisited[discreteStateTuple] = rP.MemoryVisited[discreteStateTuple] + 1;
                    }

                    // Ball moves
                    currGS.BallX += currGS.BallVelocityX;
                    currGS.BallY += currGS.BallVelocityY;

                    if (currGS.BallX < 0)  // ball is off the left edge of the screen
                    {
                        currGS.BallX = currGS.BallX * -1;
                        currGS.BallVelocityX = currGS.BallVelocityX * -1;
                    }

                    if (currGS.BallY < 0)  // ball off the top of the screen
                    {
                        currGS.BallY = currGS.BallY * -1;
                        currGS.BallVelocityY = currGS.BallVelocityY * -1;
                    }
                    else if (currGS.BallY > 1) // ball is off the bottom of the screen
                    {
                        currGS.BallY = 2 - currGS.BallY;
                        currGS.BallVelocityY = currGS.BallVelocityY * -1;
                    }

                    discreteBallX = (Int32)Math.Floor(GlobalValues.DiscreteBoardX * (currGS.BallX - 0) / (1 - 0));
                    discreteBallY = (Int32)Math.Floor(GlobalValues.DiscreteBoardY * (currGS.BallY - 0) / (1 - 0));

                    // Start Q(s',a) by moving the paddle
                    int discreteNextPaddleY;
                    if (currGS.StateAction.Equals("UP"))
                    {
                        currGS.PaddleY -= 0.04;
                        if (currGS.PaddleY < 0)
                        {
                            currGS.PaddleY = 0;
                        }
                        discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                    }
                    else if (currGS.StateAction.Equals("DOWN"))
                    {
                        currGS.PaddleY += 0.04;
                        if (currGS.PaddleY > 1 - GlobalValues.PaddleHeight)
                        {
                            currGS.PaddleY = 1 - GlobalValues.PaddleHeight;
                        }
                        discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                    }
                    else
                    {
                        discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                    }
                    var discreteNextStateTuple = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "N/A");

                    //rP.DiscretePaddleY = discretePaddleY;
                    //initialBall.DiscreteBallX = discreteBallX;
                    //initialBall.DiscreteBallY = discreteBallY;

                    //DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, initialBall, numTrial, trialIterations);

                    // Observe successor state and reward
                    // Detect bounce for reward of next iteration
                    if (currGS.BallX >= currGS.PaddleX)
                    {
                        //double yIntersect = (currGS.BallVelocityY / currGS.BallVelocityX) * (-1 * currGS.BallVelocityX) + currGS.BallY;
                        double yIntersect = currGS.BallY + ((1 - currGS.BallX) / currGS.BallVelocityX) * currGS.BallVelocityY;

                        if (yIntersect <= currGS.PaddleY + GlobalValues.PaddleHeight && yIntersect >= currGS.PaddleY)
                        {
                            rP.addDeflection();
                            currGS.StateReward = 1;

                            currGS.BallX = 2 * currGS.PaddleX - currGS.BallX;
                            double randomXVel = rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX);
                            double randomYVel = rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY);

                            currGS.BallVelocityX = currGS.BallVelocityX * -1 + randomXVel;
                            currGS.BallVelocityY = currGS.BallVelocityY + randomYVel;

                            if (Math.Abs(currGS.BallVelocityX) < 0.03)
                            {
                                if (currGS.BallVelocityX < 0)
                                {
                                    currGS.BallVelocityX = -0.03;
                                }
                                else
                                {
                                    currGS.BallVelocityX = 0.03;
                                }

                            }
                            if (Math.Abs(currGS.BallVelocityX) > 1)
                            {
                                if (currGS.BallVelocityX < 0)
                                {
                                    currGS.BallVelocityX = -1;
                                }
                                else
                                {
                                    currGS.BallVelocityX = 1;
                                }
                            }
                            if (Math.Abs(currGS.BallVelocityY) > 1)
                            {
                                if (currGS.BallVelocityY < 0)
                                {
                                    currGS.BallVelocityY = -1;
                                }
                                else
                                {
                                    currGS.BallVelocityY = 1;
                                }

                            }

                        }
                        else
                        {
                            rP.addGamesLost();
                            currGS.StateReward = -1;
                            //break;
                        }

                    }
                    //else
                    //{
                    //    currGS.StateReward = 0;
                    //}

                    // Learning Rate
                    int stateActionCount = 0;
                    try
                    {
                        rP.MemoryVisited.TryGetValue(discreteStateTuple, out stateActionCount);
                    }
                    catch
                    {
                        ;
                    }
                    double decayedLearningRate = learningConstant / (learningConstant + stateActionCount);

                    double maxNextQ = 0;
                    double maxQ = 0;
                    if (currGS.StateReward > -1)
                    {
                        // Q(s',a')
                        //// Ball moves
                        //currGS.BallX += currGS.BallVelocityX;
                        //currGS.BallY += currGS.BallVelocityY;

                        //if (currGS.BallX < 0)  // ball is off the left edge of the screen
                        //{
                        //    currGS.BallX = currGS.BallX * -1;
                        //    currGS.BallVelocityX = currGS.BallVelocityX * -1;
                        //}

                        //if (currGS.BallY < 0)  // ball off the top of the screen
                        //{
                        //    currGS.BallY = currGS.BallY * -1;
                        //    currGS.BallVelocityY = currGS.BallVelocityY * -1;
                        //}
                        //else if (currGS.BallY > 1) // ball is off the bottom of the screen
                        //{
                        //    currGS.BallY = 2 - currGS.BallY;
                        //    currGS.BallVelocityY = currGS.BallVelocityY * -1;
                        //}

                        //int discreteNextBallX = (Int32)Math.Floor(GlobalValues.DiscreteBoardX * (currGS.BallX - 0) / (1 - 0));
                        //int discreteNextBallY = (Int32)Math.Floor(GlobalValues.DiscreteBoardY * (currGS.BallY - 0) / (1 - 0));

                        discreteVelocityX = 0;
                        if (currGS.BallVelocityX >= 0)
                        {
                            discreteVelocityX = 1;
                        }
                        if (currGS.BallVelocityX < 0)
                        {
                            discreteVelocityX = -1;
                        }
                        discreteVelocityY = 0;
                        if (Math.Abs(currGS.BallVelocityY) < 0.015)
                        {
                            discreteVelocityY = 0;
                        }
                        else if (currGS.BallVelocityY >= 0)
                        {
                            discreteVelocityY = 1;
                        }
                        else
                        {
                            discreteVelocityY = -1;
                        }

                        // Take some action
                        // a is UP
                        double qUp = 0;
                        //double prevPaddleY = currGS.PaddleY;
                        //currGS.PaddleY -= 0.04;
                        //if (currGS.PaddleY < 0)
                        //{
                        //    currGS.PaddleY = 0;
                        //}
                        //discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        //currGS.PaddleY = prevPaddleY;
                        //var discreteStateTupleUp = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "UP");  // 
                        var discreteStateTupleUp = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "UP");  // 
                        try
                        {
                            //rP.Memory.TryGetValue(discreteStateTupleUp, out qUp);
                            rP.Memory.TryGetValue(discreteStateTupleUp, out qUp);
                        }
                        catch
                        {
                            ;
                        }

                        // a is DOWN
                        double qDown = 0;
                        //currGS.PaddleY += 0.04;
                        //if (currGS.PaddleY > 1 - GlobalValues.PaddleHeight)
                        //{
                        //    currGS.PaddleY = 1 - GlobalValues.PaddleHeight;
                        //}
                        //discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        //currGS.PaddleY = prevPaddleY;
                        //var discreteStateTupleDown = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "DOWN");
                        var discreteStateTupleDown = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "DOWN");
                        try
                        {
                            rP.Memory.TryGetValue(discreteStateTupleDown, out qDown);
                        }
                        catch
                        {
                            ;
                        }
                        // a is HOLD
                        double qHold = 0;
                        discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        //var discreteStateTupleHold = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "HOLD");
                        var discreteStateTupleHold = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "HOLD");
                        try
                        {
                            rP.Memory.TryGetValue(discreteStateTupleHold, out qHold);
                        }
                        catch
                        {
                            ;
                        }

                        // Get Max - Action
                        string maxNextAction = "";
                        if (qUp < 0 || qDown < 0 || qHold < 0)
                        {
                            ;
                        }
                        maxNextQ = Math.Max(qUp, Math.Max(qDown, qHold));
                        if (maxNextQ == qUp)
                        {
                            maxNextAction = "UP";
                        }
                        else if (maxNextQ == qDown)
                        {
                            maxNextAction = "DOWN";
                        }
                        else
                        {
                            maxNextAction = "HOLD";
                        }

                        // Q(s,a') - Exploration
                        // Number of times taken action a' in state s
                        int stateActionCountUp = 0;
                        int stateActionCountDown = 0;
                        int stateActionCountHold = 0;

                        try
                        {
                            rP.MemoryVisited.TryGetValue(discreteStateTupleUp, out stateActionCountUp);
                        }
                        catch
                        {
                            ;
                        }
                        try
                        {
                            rP.MemoryVisited.TryGetValue(discreteStateTupleDown, out stateActionCountDown);
                        }
                        catch
                        {
                            ;
                        }
                        try
                        {
                            rP.MemoryVisited.TryGetValue(discreteStateTupleHold, out stateActionCountHold);
                        }
                        catch
                        {
                            ;
                        }

                        double optimisticRewardUp = -2.0;
                        double optimisticRewardDown = -2.0;
                        double optimisticRewardHold = -2.0;
                        double optimisticReward = -2.0;
                        string optimisticAction = "";
                        if (stateActionCountUp < explorationLimit)
                        {
                            optimisticRewardUp = .83;
                            optimisticReward = optimisticRewardUp;
                            optimisticAction = "UP";
                        }
                        if (stateActionCountDown < explorationLimit)
                        {
                            optimisticRewardDown = .82;
                            if (optimisticReward < optimisticRewardDown)
                            {
                                optimisticReward = optimisticRewardDown;
                                optimisticAction = "DOWN";
                            }
                        }
                        if (stateActionCountHold < explorationLimit)
                        {
                            optimisticRewardHold = .81;
                            if (optimisticReward < optimisticRewardHold)
                            {
                                optimisticReward = optimisticRewardHold;
                                optimisticAction = "HOLD";
                            }
                        }

                        // Exploration vs Exploitation
                        //maxNextQ/maxNextAction vs optimisticReward/optimisticAction
                        maxQ = Math.Max(maxNextQ, optimisticReward);
                        string maxAction = "";
                        if (maxQ == maxNextQ)
                        {
                            maxAction = maxNextAction;
                        }
                        else
                        {
                            maxAction = optimisticAction;
                        }
                        currGS.StateAction = maxAction;

                        //// a is UP
                        //qUp = 0;
                        ////double prevPaddleY = currGS.PaddleY;
                        ////currGS.PaddleY -= 0.04;
                        ////if (currGS.PaddleY < 0)
                        ////{
                        ////    currGS.PaddleY = 0;
                        ////}
                        //discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        ////currGS.PaddleY = prevPaddleY;
                        //var discreteNextStateTupleUp = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "UP");
                        //try
                        //{
                        //    rP.Memory.TryGetValue(discreteNextStateTupleUp, out qUp);
                        //}
                        //catch
                        //{
                        //    ;
                        //}

                        //// a is DOWN
                        //qDown = 0;
                        ////currGS.PaddleY += 0.04;
                        ////if (currGS.PaddleY > 1 - GlobalValues.PaddleHeight)
                        ////{
                        ////    currGS.PaddleY = 1 - GlobalValues.PaddleHeight;
                        ////}
                        //discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        ////currGS.PaddleY = prevPaddleY;
                        //var discreteNextStateTupleDown = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "DOWN");
                        //try
                        //{
                        //    rP.Memory.TryGetValue(discreteNextStateTupleDown, out qDown);
                        //}
                        //catch
                        //{
                        //    ;
                        //}
                        //// a is HOLD
                        //qHold = 0;
                        //discreteNextPaddleY = Math.Min((Int32)Math.Floor(GlobalValues.DiscreteBoardY * currGS.PaddleY / (1 - GlobalValues.PaddleHeight)), GlobalValues.DiscreteBoardY - 1);
                        //var discreteNextStateTupleHold = Tuple.Create(Math.Min(discreteBallX, GlobalValues.DiscreteBoardX - 1), Math.Min(discreteBallY, GlobalValues.DiscreteBoardX - 1), discreteVelocityX, discreteVelocityY, discreteNextPaddleY, "HOLD");
                        //try
                        //{
                        //    rP.Memory.TryGetValue(discreteNextStateTupleHold, out qHold);
                        //}
                        //catch
                        //{
                        //    ;
                        //}

                        // Get Max - Action
                        //maxNextQ = Math.Max(qUp, Math.Max(qDown, qHold));
                   
                    }
                    else
                    {
                        maxQ = 0;
                    }

                    // Update Q
                    if (discreteStateTuple.Item1 == 10)
                    {
                        ;
                    }
                    rP.Memory[discreteStateTuple] = currQVal + decayedLearningRate * (currGS.StateReward + discountFactor * maxQ - currQVal);

                    if (currGS.StateReward == -1)
                    {
                        break;
                    }

                }
                if (rP.Deflections > maxDeflections)
                {
                    maxDeflections = rP.Deflections;
                }
                try
                {
                    if (last10.Count >= 10)
                    {
                        last10.RemoveAt(0);
                    }
                    last10.Add(rP.Deflections);
                }
                catch
                {
                    last10.Add(rP.Deflections);
                }
                totalTrialLength = totalTrialLength + trialIterations;
                rP.TotalTrialDeflections = rP.TotalTrialDeflections + rP.Deflections;
                Console.WriteLine("*********************************");
                Console.WriteLine("Trial: " + numTrial);
                Console.WriteLine("Trial Length: " + trialIterations);
                Console.WriteLine("Number of deflections: " + rP.Deflections);
                Console.WriteLine("Avg Trial number of deflections: " + (double)rP.TotalTrialDeflections / rP.GamesPlayed);
                Console.WriteLine("Last 10 Avg Trial number of deflections: " + (double)last10.Average());

                Console.WriteLine("Avg Trial length: " + (double)totalTrialLength / rP.GamesPlayed);
                if ((double)rP.TotalTrialDeflections / rP.GamesPlayed > 9)
                {
                    Console.WriteLine("Agent may have found an optimal policy!");
                }
                Console.WriteLine("Max number of deflections: " + maxDeflections);
                Console.WriteLine("*********************************");
                rP.Deflections = 0;
            }

            DateTime end = DateTime.Now;
            Console.WriteLine("*************SUMMARY:*************");
            Console.WriteLine("Training start: " + start);
            Console.WriteLine("Training end: " + end);
            Console.WriteLine("Training duration: " + (end - start));

            do
            {

                Console.WriteLine("Press p to play, 1 player");
            } while (Console.ReadKey().KeyChar != 'p');


            int totalGames = 10;
            //double learningConstant = 5;
            //double discountFactor = .9;
            maxDeflections = 0;

            Dictionary<int, int> gameDeflections = new Dictionary<int, int>();
            rP.Deflections = 0;
            rP.ExplorationLimit = 1;
            DateTime startGame = DateTime.Now;
            for (int numGame = 0; numGame < totalGames; numGame++)
            {
                Ball b = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, false);

                // initial state of the ball, make random velocity, ball in the middle, paddleY is in the middle
                //Ball b = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX + rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX), rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY), GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, false);
                rP.PaddleY = 0.5 - (GlobalValues.PaddleHeight / 2);
                rP.DiscretePaddleY = rP.getDiscretePaddleY();
                rP.PrevAction = "N/A";

                var currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);
                var currentDiscreteTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, rP.DiscretePaddleY);
                var prevStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                // Epsiode/Trial Loop
                int gameIterations = 0;
                rP.addGamesPlayed();
                while (true)
                {
                    gameIterations++;

                    prevStateTuple = currentStateTuple;
                    b.MoveBall();
                    currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numGame, gameIterations);

                    if (goalState(currentStateTuple, rP, null))
                    {
                        if (deflectionDetected(currentStateTuple, prevStateTuple, rP, null, b))
                        {
                            b.BallX = 2 * rP.PaddleX - b.BallX;
                            double randomXVel = rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX);
                            double randomYVel = rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY);

                            if (Math.Abs(randomXVel) > 0.015)
                            {
                                ;
                            }
                            if (Math.Abs(randomYVel) > .03)
                            {
                                ;
                            }

                            b.VelocityX = b.VelocityX * -1 + randomXVel;
                            b.VelocityY = b.VelocityY + randomYVel;

                            if (Math.Abs(b.VelocityX) < 0.03)
                            {
                                if (b.VelocityX < 0)
                                {
                                    b.VelocityX = -0.03;
                                    //b.VelocityX = b.VelocityX * -1;
                                }
                                else
                                {
                                    b.VelocityX = 0.03;
                                }

                            }
                            // TODO: Put direction?
                            if (Math.Abs(b.VelocityX) > 1)
                            {
                                if (b.VelocityX < 0)
                                {
                                    b.VelocityX = -1;
                                }
                                else
                                {
                                    b.VelocityX = 1;
                                }
                            }
                            if (Math.Abs(b.VelocityY) > 1)
                            {
                                if (b.VelocityY < 0)
                                {
                                    b.VelocityY = -1;
                                }
                                else
                                {
                                    b.VelocityY = 1;
                                }

                            }
                        }
                        else
                        {

                            rP.addGamesLost();
                            //rP.updateQ(b, rP.PrevAction, rP, null, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, GlobalValues.PaddleHeight, GlobalValues.LearningRate, GlobalValues.DiscountFactor, int stateReward);
                            break;
                        }
                    }
                    // Draw Board
                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numGame, gameIterations);

                    rP.MoveRightPaddle(b);
                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numGame, gameIterations);

                    rP.tdUpdate3(b, prevStateTuple, currentStateTuple);

                    //DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, null, rP, b, numTrial);
                    // Add new State
                    //prevStateTuple = currentStateTuple;
                    //currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                }
                gameDeflections.Add(numGame, rP.Deflections);
                if (rP.Deflections > maxDeflections)
                {
                    maxDeflections = rP.Deflections;
                }
                Console.WriteLine("*********************************");
                Console.WriteLine("Game: " + numGame);
                Console.WriteLine("Game Length: " + gameIterations);
                Console.WriteLine("Number of deflections: " + rP.Deflections);
                Console.WriteLine("Max number of deflections: " + maxDeflections);
                double avgDeflections = (double)gameDeflections.Sum(v => v.Value) / (double)gameDeflections.Count;
                Console.WriteLine("Average number of deflections: " + avgDeflections.ToString());
                Console.WriteLine("*********************************");

                rP.Deflections = 0;
            }


            DateTime endGame = DateTime.Now;
            Console.WriteLine("*************SUMMARY:*************");
            Console.WriteLine("Games start: " + startGame);
            Console.WriteLine("Games end: " + endGame);
            Console.WriteLine("Games duration: " + (endGame - startGame));

            do
            {

                Console.WriteLine("Press p to play, 2 player");
            } while (Console.ReadKey().KeyChar != 'p');

            totalGames = 1000;
            //double learningConstant = 5;
            //double discountFactor = .9;
            maxDeflections = 0;
            rP.GamesLost = 0;
            rP.GamesPlayed = 0;
            rP.Deflections = 0;
            rP.ExplorationLimit = 1;

            Player lP = new Player(0, 0.5 - (GlobalValues.PaddleHeight / 2), learningConstant, discountFactor, Tuple.Create(initialBall.DiscreteBallX, initialBall.DiscreteBallY, initialBall.DiscreteVelocityX, initialBall.DiscreteVelocityY, (int)Math.Floor(GlobalValues.DiscreteBoardY * (0.5 - (GlobalValues.PaddleHeight / 2)) / (1 - GlobalValues.PaddleHeight))), GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, explorationLimit);

            DateTime start2PlayerGame = DateTime.Now;
            for (int numGame = 0; numGame < totalGames; numGame++)
            {
                Ball b = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX, GlobalValues.InitialVelocityY, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, true);
                // initial state of the ball, make random velocity, ball in the middle, paddleY is in the middle
                //Ball b = new Ball(GlobalValues.InitialBallX, GlobalValues.InitialBallY, GlobalValues.InitialVelocityX + rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX), rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY), GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, true);
                rP.PaddleY = 0.5 - (GlobalValues.PaddleHeight / 2);
                rP.DiscretePaddleY = rP.getDiscretePaddleY();
                rP.PrevAction = "N/A";

                lP.PaddleY = 0.5 - (GlobalValues.PaddleHeight / 2);
                lP.DiscretePaddleY = lP.getDiscretePaddleY();
                lP.PrevAction = "N/A";

                var currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);
                var currentDiscreteTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, rP.DiscretePaddleY);
                var prevStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                // Epsiode/Trial Loop
                int gameIterations = 0;
                rP.addGamesPlayed();
                while (true)
                {
                    gameIterations++;

                    prevStateTuple = currentStateTuple;
                    b.MoveBall();
                    currentStateTuple = Tuple.Create(b.BallX, b.BallY, b.VelocityX, b.VelocityY, rP.PaddleY);

                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, lP, rP, b, numGame, gameIterations);

                    if (goalState(currentStateTuple, rP, lP))
                    {
                        if (deflectionDetected(currentStateTuple, prevStateTuple, rP, lP, b))
                        {
                            if (b.BallX < 0.5 * boardX)
                            {
                                b.BallX = 2 * lP.PaddleX - b.BallX;
                            }
                            else
                            {
                                b.BallX = 2 * rP.PaddleX - b.BallX;
                            }
                            double randomXVel = rand.NextDouble() * (GlobalValues.RandomVelocityX - (-1 * GlobalValues.RandomVelocityX)) + (-1 * GlobalValues.RandomVelocityX);
                            double randomYVel = rand.NextDouble() * (GlobalValues.RandomVelocityY - (-1 * GlobalValues.RandomVelocityY)) + (-1 * GlobalValues.RandomVelocityY);

                            if (Math.Abs(randomXVel) > 0.015)
                            {
                                ;
                            }
                            if (Math.Abs(randomYVel) > .03)
                            {
                                ;
                            }

                            b.VelocityX = b.VelocityX * -1 + randomXVel;
                            b.VelocityY = b.VelocityY + randomYVel;

                            if (Math.Abs(b.VelocityX) < 0.03)
                            {
                                if (b.VelocityX < 0)
                                {
                                    b.VelocityX = -0.03;
                                    //b.VelocityX = b.VelocityX * -1;
                                }
                                else
                                {
                                    b.VelocityX = 0.03;
                                }

                            }
                            // TODO: Put direction?
                            if (Math.Abs(b.VelocityX) > 1)
                            {
                                if (b.VelocityX < 0)
                                {
                                    b.VelocityX = -1;
                                }
                                else
                                {
                                    b.VelocityX = 1;
                                }
                            }
                            if (Math.Abs(b.VelocityY) > 1)
                            {
                                if (b.VelocityY < 0)
                                {
                                    b.VelocityY = -1;
                                }
                                else
                                {
                                    b.VelocityY = 1;
                                }

                            }
                        }
                        else
                        {
                            // Who lost?
                            if (currentStateTuple.Item1 >= rP.PaddleX)
                            {
                                rP.addGamesLost();
                            }
                            if (currentStateTuple.Item1 <= lP.PaddleX)
                            {
                                lP.addGamesLost();
                            }
                            //rP.updateQ(b, rP.PrevAction, rP, null, GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, GlobalValues.PaddleHeight, GlobalValues.LearningRate, GlobalValues.DiscountFactor, int stateReward);
                            break;
                        }
                    }
                    // Draw Board
                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, lP, rP, b, numGame, gameIterations);

                    rP.MoveRightPaddle(b);
                    lP.MoveLeftPaddle(b);

                    DrawPong(GlobalValues.DiscreteBoardX, GlobalValues.DiscreteBoardY, lP, rP, b, numGame, gameIterations);

                    rP.tdUpdate3(b, prevStateTuple, currentStateTuple);

                }

                Console.WriteLine("*********************************");
                Console.WriteLine("Game: " + numGame);
                Console.WriteLine("Game Length: " + gameIterations);
                Console.WriteLine("Number of deflections for right player: " + rP.Deflections);
                Console.WriteLine("Number of deflections for left player: " + (lP.Deflections));
                Console.WriteLine("*********************************");

                rP.Deflections = 0;
            }


            DateTime end2PlayerGame = DateTime.Now;
            Console.WriteLine("*************SUMMARY:*************");
            Console.WriteLine("Games start: " + start2PlayerGame);
            Console.WriteLine("Games end: " + end2PlayerGame);
            Console.WriteLine("Games won by agent: " + (rP.GamesPlayed - rP.GamesLost));
            Console.WriteLine("Winning Percentage of agent: " + (double)(rP.GamesPlayed - rP.GamesLost) / rP.GamesPlayed);
            Console.WriteLine("Games duration: " + (end2PlayerGame - start2PlayerGame));

            do
            {

                Console.WriteLine("Press q to quit");
            } while (Console.ReadKey().KeyChar != 'q');

        }

        static bool goalState(Tuple<double, double, double, double, double> currState, Player rP, Player lP)
        {
            // ball past 
            if (currState.Item1 >= rP.PaddleX)
            {
                //rP.addGamesLost();
                return true;
            }

            if (lP != null)
            {
                if (currState.Item1 <= lP.PaddleX)
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
                //double yIntersect = currState.Item4 * prevState.Item1 + prevState.Item2;
                double yIntersect = (b.VelocityY / b.VelocityX) * (-1 * b.VelocityX) + b.BallY;

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
                if (prevState.Item1 < lP.PaddleX && currState.Item1 >= lP.PaddleX)
                {
                    // Calculate Y coordinate when ball is at the same level as the paddle
                    //double yIntersect = currState.Item4 * prevState.Item1 + prevState.Item2;
                    double yIntersect = (b.VelocityY / b.VelocityX) * (-1 * b.VelocityX) + b.BallY;

                    if (yIntersect >= lP.PaddleY - 0.2 && yIntersect <= lP.PaddleY)
                    {
                        lP.addDeflection();
                        return true;
                    }
                }
            }
            return false;
        }

        static void DrawPong(int boardX, int boardY, Player lPlayer, Player rPlayer, Ball b, int numTrial, int trialIteration)
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

                        if (x == b.DiscreteBallX && y == b.DiscreteBallY && x == lPlayer.DiscretePaddleX && y == lPlayer.DiscretePaddleY)
                        {
                            Console.Write("X");
                        }
                    }

                    //if (x == b.DiscreteBallX - 1 && y == b.DiscreteBallY && x == rPlayer.DiscretePaddleX - 1 && y == rPlayer.DiscretePaddleY)
                    if (x == b.DiscreteBallX && y == b.DiscreteBallY && x == rPlayer.DiscretePaddleX && y == rPlayer.DiscretePaddleY)
                    {
                        Console.Write("X");
                    }
                    //else if (x == rPlayer.DiscretePaddleX-1 && y == rPlayer.DiscretePaddleY)
                    else if (x == rPlayer.DiscretePaddleX && y == rPlayer.DiscretePaddleY)
                    {
                        Console.Write("|");
                    }
                    //else if (x == b.DiscreteBallX-1 && y == b.DiscreteBallY)
                    else if (x == b.DiscreteBallX && y == b.DiscreteBallY)
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
            Console.WriteLine("Trial Length: " + trialIteration);
            Console.WriteLine("Number of Agent Deflections: " + rPlayer.Deflections);
            Console.WriteLine("Avg Trial number of deflections: " + (double)rPlayer.TotalTrialDeflections / rPlayer.GamesPlayed);
        }

        //static void Write(Dictionary<Tuple<int, int, int, int, int, string>, double> dictionary, string file, int numTrial, int totalDeflections, int gamesPlayed)
        static void Write(Dictionary<Tuple<int, int, int, int, int, string>, double> dictionary, string file)
        {
            using (StreamWriter f = new StreamWriter(file, false))
            {
                //f.WriteLine("numTrial" + numTrial);
                //f.WriteLine("totalDeflections" + totalDeflections);
                //f.WriteLine("gamesPlayed" + gamesPlayed);

                foreach (var entry in dictionary)
                {
                    f.WriteLine("{0} {1}", entry.Key, entry.Value);
                }
            }
        }

        static void Write(Dictionary<Tuple<int, int, int, int, int, string>, int> dictionary, string file)
        {
            using (StreamWriter f = new StreamWriter(file,false))
                foreach (var entry in dictionary)
                {
                    f.WriteLine("{0} {1}", entry.Key, entry.Value);
                }
        }

        static Dictionary<Tuple<int, int, int, int, int, string>, double> ReadQ(string file)
        {
            Dictionary<Tuple<int, int, int, int, int, string>, double> d = new Dictionary<Tuple<int, int, int, int, int, string>, double>();
            using (var sr = new StreamReader(file))
            {
                string line = null;

                // while it reads a key
                while ((line = sr.ReadLine()) != null)
                {
                    string tuple = line.Split(')')[0].Trim('(');
                    double value = Double.Parse(line.Split(')')[1]);

                    Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(',')[0]), Int32.Parse(tuple.Split(',')[1]), Int32.Parse(tuple.Split(',')[2]), Int32.Parse(tuple.Split(',')[3]), Int32.Parse(tuple.Split(',')[4]), tuple.Split(',')[5].ToString());
                    d.Add(newTuple, value);
                    //d.Add(line, sr.ReadLine());
                }
            }
            return d;
        }

        static Dictionary<Tuple<int, int, int, int, int, string>, int> ReadN(string file)
        {
            Dictionary<Tuple<int, int, int, int, int, string>, int> d = new Dictionary<Tuple<int, int, int, int, int, string>, int>();
            using (var sr = new StreamReader(file))
            {
                string line = null;

                // while it reads a key
                while ((line = sr.ReadLine()) != null)
                {
                    string tuple = line.Split(')')[0].Trim('(');
                    int value = Int32.Parse(line.Split(')')[1]);

                    Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(',')[0]), Int32.Parse(tuple.Split(',')[1]), Int32.Parse(tuple.Split(',')[2]), Int32.Parse(tuple.Split(',')[3]), Int32.Parse(tuple.Split(',')[4]), tuple.Split(',')[5].ToString());
                    d.Add(newTuple, value);
                    //d.Add(line, sr.ReadLine());
                }
            }
            return d;
        }

    }
}
