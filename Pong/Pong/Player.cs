using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    class Player
    {
        private double _paddleX;
        private int _discretePaddleX;
        private double _paddleY;
        private int _discretePaddleY;
        private int _deflections;
        private int _gamesPlayed;
        private int _gamesLost;
        private double _paddleHeight = 0.2;
        private Dictionary<Tuple<int, int, int, int, int, string>, double> _Q;
        private Dictionary<Tuple<int, int, int, int, int, string>, int> _N;
        private Dictionary<Tuple<int, int, int, int, int>, string> currQ = new Dictionary<Tuple<int, int, int, int, int>, string>();
        private Dictionary<Tuple<int, int, int, int, int>, string> nextQ = new Dictionary<Tuple<int, int, int, int, int>, string>();

        Dictionary<string, double> moveDict = new Dictionary<string, double>();

        private double _learningRate;  // alpha
        private double _discountFactor; // gamma

        private int _explorationLimit;
        private Tuple<int, int, int, int, int> _discreteState;

        private int _boardX;
        private int _boardY;

        private string _prevAction = "HOLD";
        public Player(double paddleX, double paddleY, double learningRate, double discountFactor, Tuple<int, int, int, int, int> state, int boardX, int boardY, int explorationLimit)
        {
            this._boardX = boardX;
            this._boardY = boardY;
            this._paddleX = paddleX;
            this._discretePaddleX = getDiscretePaddleX();
            this._paddleY = paddleY;
            this._discretePaddleY = getDiscretePaddleY();
            this._learningRate = learningRate;
            this._discountFactor = discountFactor;
            this._discreteState = state;

            this._explorationLimit = explorationLimit;
            this._deflections = 0;
            this._gamesPlayed = 0;
            this._gamesLost = 0;
            this._Q = new Dictionary<Tuple<int, int, int, int, int, string>, double>();
            this._N = new Dictionary<Tuple<int, int, int, int, int, string>, int>();
        }

        public double PaddleX
        {
            get { return this._paddleX; }
            set { this._paddleX = value; }
        }

        public double PaddleY
        {
            get { return this._paddleY; }
            set { this._paddleY = value; }
        }

        public int DiscretePaddleX
        {
            get { return this._discretePaddleX; }
            set { this._discretePaddleX = value; }
        }

        public int DiscretePaddleY
        {
            get { return this._discretePaddleY; }
            set { this._discretePaddleY = value; }
        }

        public int GamesPlayed
        {
            get { return this._gamesPlayed; }
            set { this._gamesPlayed = value; }
        }

        public int GamesLost
        {
            get { return this._gamesLost; }
            set { this._gamesLost = value; }
        }

        public int Deflections
        {
            get { return this._deflections; }
            set { this._deflections = value; }
        }

        public string PrevAction
        {
            get { return this._prevAction; }
            set { this._prevAction = value; }
        }

        public Dictionary<Tuple<int, int, int, int, int, string>, double> Memory
        {
            get { return this._Q; }
            set { this._Q = value; }
        }

        public void MoveLeftPaddle(Ball b)
        {
            if (this._paddleY < b.BallY)
            {
                this.PaddleY += 0.02;
            }
            else if (this._paddleY > b.BallY) {
                this.PaddleY -= 0.02;
            }
            else {
                this.PaddleY = this.PaddleY;
            }
            
        }

        public void MoveRightPaddle(Ball b, Tuple<double, double, double, double, double> currentStateTuple)
        {
            string action = explorationFunction(b, currentStateTuple);
            if (action.Equals("UP"))
            {
                this._paddleY -= 0.04;
                this._discretePaddleY = getDiscretePaddleY();
            }
            else if (action.Equals("DOWN"))
            {
                this._paddleY += 0.04;
                this._discretePaddleY = getDiscretePaddleY();
            }
            else
            {
                this._paddleY = this._paddleY;
                this._discretePaddleY = getDiscretePaddleY();
            }

            if (this._discretePaddleY < 1)
            {
                this._paddleY = 0.08;
                this._discretePaddleY = getDiscretePaddleY();
            }
            else if (this._paddleY > this._boardY)
            {
                this._paddleY = 1 - this._paddleHeight;
                this._discretePaddleY = getDiscretePaddleY();
            }
            _prevAction = action;
        }

        private string explorationFunction(Ball b, Tuple<double, double, double, double, double> currentStateTuple)
        {

            var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "UP");
            var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
            var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");
            try
            {
                if (this._N[discreteStateTupleUp] < this._explorationLimit)
                {
                    try
                    {
                        this._N.Add(discreteStateTupleUp, 1);
                    }
                    catch
                    {
                        int tmpCount = this._N[discreteStateTupleUp];
                        this._N.Remove(discreteStateTupleUp);
                        this._N.Add(discreteStateTupleUp, tmpCount + 1);
                    }

                    updateQ(b, "UP");
                    return "UP";
                }
            }
            catch
            {
                this._N.Add(discreteStateTupleUp, 1);
                updateQ(b, "UP");
                return "UP";
            }
            try
            {
                if (this._N[discreteStateTupleDown] < this._explorationLimit)
                {
                    try
                    {
                        this._N.Add(discreteStateTupleDown, 1);
                    }
                    catch
                    {
                        int tmpCount = this._N[discreteStateTupleDown];
                        this._N.Remove(discreteStateTupleDown);
                        this._N.Add(discreteStateTupleDown, tmpCount + 1);
                    }
                    updateQ(b, "DOWN");
                    return "DOWN";
                }
            }
            catch
            {
                this._N.Add(discreteStateTupleDown, 1);
                updateQ(b, "DOWN");
                return "DOWN";
            }
            try
            {
                if (this._N[discreteStateTupleHold] < this._explorationLimit)
                {
                    try
                    {
                        this._N.Add(discreteStateTupleHold, 1);
                    }
                    catch
                    {
                        int tmpCount = this._N[discreteStateTupleHold];
                        this._N.Remove(discreteStateTupleHold);
                        this._N.Add(discreteStateTupleHold, tmpCount + 1);
                    }
                    updateQ(b, "HOLD");
                    return "HOLD";
                }
            }
            catch
            {
                this._N.Add(discreteStateTupleHold, 1);
                updateQ(b, "HOLD");
                return "HOLD";
            }

            // Exploit based on Q            
            return chooseBasedOnQ(b);                
            
        }

        private string chooseBasedOnQ(Ball b)
        {
            double currentQ;
            double upQ;
            double downQ;
            double holdQ;


            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            nextBallState.MoveBall();
            var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, _prevAction);

            this._Q.TryGetValue(discreteStateTuple, out currentQ);

            moveDict.Clear();
            // Simulate Next Decision
            var discreteNextStateTupleUp = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, getDiscretePaddleY(this.PaddleY - 0.04), "UP");
            this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
            moveDict.Add("UP", upQ);

            var discreteNextStateTupleDown = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, getDiscretePaddleY(this.PaddleY + 0.04), "DOWN");
            this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
            moveDict.Add("DOWN", downQ);

            var discreteNextStateTupleHold = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, getDiscretePaddleY(this.PaddleY), "HOLD");
            this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
            moveDict.Add("HOLD", holdQ);

            Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            updateQ(b, sortedMoveDict.First().Key);
            return sortedMoveDict.First().Key;

        }

        public void updateQ(Ball b, string nextActionChosen)
        {

            var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, this._prevAction);

            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            nextBallState.MoveBall();

            double newPaddleY = this._paddleY;
            int distinctNewPaddleY = 0;

            if (nextActionChosen.Equals("UP"))
            {
                newPaddleY -= 0.04;
            }
            else if (nextActionChosen.Equals("DOWN"))
            {
                newPaddleY += 0.04;
            }
            else
            {
                newPaddleY = newPaddleY;
                this._discretePaddleY = getDiscretePaddleY();
            }
            distinctNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            var discreteNextStateTuple = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, distinctNewPaddleY, nextActionChosen);

            //try
            //{
            //    currQ.Add(discreteStateTuple, this._prevAction);
            //}
            //catch
            //{
            //    ;
            //}
            //try
            //{
            //    nextQ.Add(discreteNextStateTuple, nextActionChosen);
            //}
            //catch
            //{
            //    ;
            //}

            int stateReward = 0;
            // ball past continuous
            if (b.BallX > this._paddleX)
            {
                // right player deflects it
                if (b.BallX < this._paddleX && nextBallState.BallX >= this._paddleX)
                {
                    // Calculate Y coordinate when ball is at the same level as the paddle
                    double yIntersect = b.VelocityY * b.BallX + b.BallY;

                    if (yIntersect <= this._paddleY + GlobalValues.PaddleHeight && yIntersect >= this._paddleY)
                    {
                        stateReward = 1;
                    }
                    else
                    {
                        stateReward = -1;
                    }
                }
            }

            //if (b.BallX == this._paddleX)
            //{
            //    if (b.BallY >= this._paddleY - this._paddleHeight && b.BallY <= this._paddleY)
            //    {
            //        stateReward = 1;
            //    }
            //}
            //else if (b.BallX > this._paddleX)
            //{
            //    stateReward = -1;
            //}
            //else
            //{
            //    stateReward = 0;
            //}

            //if (b.DiscreteBallX == this.DiscretePaddleX)
            //{
            //    if (b.DiscreteBallY == this.DiscretePaddleY)
            //    {
            //        stateReward = 1;
            //    }
            //}

            // Check if states have value, else default it to 0
            double currQVal = 0.0;
            double nextQVal = 0.0;
            int learningRateCount = 0;

            // Calculate Q
            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }
            if (Double.IsNaN(currQVal))
            {
                 Console.WriteLine("NaN");          
            }

            try
            {
                this._Q.TryGetValue(discreteNextStateTuple, out nextQVal);
            }
            catch
            {
                ;
            }
            if (Double.IsNaN(nextQVal))
            {
                 Console.WriteLine("NaN");          
            }


            // Number of times seen this state/action pair
            try
            {
                this._N.TryGetValue(discreteStateTuple, out learningRateCount);
            }
            catch
            { 
                ;
            }


            double decayedLearningRate = this._learningRate / (this._learningRate + learningRateCount);

            var tmpUp = Tuple.Create(6,6,1,0,6, "UP");
            var tmpDown = Tuple.Create(6, 6, 1, 0, 6, "DOWN");
            var tmpHold = Tuple.Create(6, 6, 1, 0, 6, "HOLD");

            this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * nextQVal - currQVal);
            if ( Double.IsNaN(this._Q[discreteStateTuple])) {
                Console.WriteLine("NaN");                
            }

        }

        //public void updateQ(Ball b, string actionChosen, Player rP, Player lP, int boardX, int boardY, double paddleHeight, double learningRate, double discountFactor)
        //{

        //    var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, rP.DiscretePaddleY, actionChosen);

        //    Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, boardX, boardY, b.TwoPlayers);
        //    nextBallState.MoveBall();
        
        //    var discreteNextStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, rP.DiscretePaddleY, actionChosen);

          
        //    int stateReward = 0;

        //    if (b.BallX == rP.PaddleX)
        //    {
        //        if (b.BallY >= rP.PaddleY - paddleHeight && b.BallY <= rP.PaddleY)
        //        {
        //            stateReward = 1;
        //        }
        //    }
        //    else if (b.BallX > rP.PaddleX)
        //    {
        //        stateReward = -1;
        //    }
        //    else
        //    {
        //        stateReward = 0;
        //    }

        //    // Check if states have value, else default it to 0
        //    double currQVal = 0.0;
        //    double nextQVal = 0.0;

        //    // Calculate Q
        //    try
        //    {
        //        rP.Memory.TryGetValue(discreteStateTuple, out currQVal);
        //    }
        //    catch
        //    {
        //        ;
        //    }

        //    try
        //    {
        //        rP.Memory.TryGetValue(discreteNextStateTuple, out nextQVal);
        //    }
        //    catch
        //    {
        //        ;
        //    }

        //    rP.Memory[discreteStateTuple] = currQVal + learningRate * (stateReward + discountFactor * nextQVal - currQVal);


        //}

        public int getDiscretePaddleX()
        {
            this._discretePaddleX = (Int32)this._paddleX * this._boardX;
            return this._discretePaddleX;
        }

        public int getDiscretePaddleY()
        {
            this._discretePaddleY = (Int32)Math.Floor(this._boardY*this._paddleY/(1-this._paddleHeight));
            return this._discretePaddleY;
        }

        public int getDiscretePaddleY(double nextY)
        {
            this._discretePaddleY = (Int32)Math.Floor(this._boardY * nextY / (1 - this._paddleHeight));
            return this._discretePaddleY;
        }   

        public void addDeflection()
        {
            this._deflections++;
        }

        public void addGamesPlayed()
        {
            this._gamesPlayed++;
        }

        public void addGamesLost()
        {
            this._gamesLost++;
        }
    }
}
