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
        //private Dictionary<Tuple<int, int, int, int, int, string>, double> _Q;
        private Dictionary<Tuple<int, int, int, int, int>, double> _Q;
        private Dictionary<Tuple<int, int, int, int, int, string>, int> _N;
        private Dictionary<Tuple<int, int, int, int, int>, string> currQ = new Dictionary<Tuple<int, int, int, int, int>, string>();
        private Dictionary<Tuple<int, int, int, int, int>, string> nextQ = new Dictionary<Tuple<int, int, int, int, int>, string>();


        Dictionary<string, double> moveDict = new Dictionary<string, double>();

        private double _learningConstant;  // alpha
        private double _discountFactor; // gamma

        private int _explorationLimit;
        private Tuple<int, int, int, int, int> _discreteState;

        private int _boardX;
        private int _boardY;

        private string _prevAction = "N/A";
        public Player(double paddleX, double paddleY, double learningConstant, double discountFactor, Tuple<int, int, int, int, int> state, int boardX, int boardY, int explorationLimit)
        {
            this._boardX = boardX;
            this._boardY = boardY;
            this._paddleX = paddleX;
            this._discretePaddleX = getDiscretePaddleX();
            this._paddleY = paddleY;
            this._discretePaddleY = getDiscretePaddleY();
            this._learningConstant = learningConstant;
            this._discountFactor = discountFactor;
            this._discreteState = state;

            this._explorationLimit = explorationLimit;
            this._deflections = 0;
            this._gamesPlayed = 0;
            this._gamesLost = 0;
            //this._Q = new Dictionary<Tuple<int, int, int, int, int, string>, double>();
            this._Q = new Dictionary<Tuple<int, int, int, int, int>, double>();
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

        //public Dictionary<Tuple<int, int, int, int, int, string>, double> Memory
        public Dictionary<Tuple<int, int, int, int, int>, double> Memory
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

        public void MoveRightPaddle(Ball b)
        {
            // Each time step, choose action using exploration/exploitation functoin
            string action = explorationFunction(b);

            // get successor state and perform TD Update
            tdUpdate(b, action);


            if (action.Equals("UP"))
            {

                this._paddleY -= 0.04;
                if (this._paddleY < 0)
                {
                    this._paddleY = 0;
                }
                this._discretePaddleY = getDiscretePaddleY();
            }
            else if (action.Equals("DOWN"))
            {
                this._paddleY += 0.04;
                if (this._paddleY > 1 - GlobalValues.PaddleHeight)
                {
                    this._paddleY = 1 - GlobalValues.PaddleHeight;
                }
                this._discretePaddleY = getDiscretePaddleY();
            }
            else
            {
                // HOLD
                // Do Nothing
            }

            //if (this._discretePaddleY < 1)
            //{
            //    this._paddleY = 0.08;
            //    this._discretePaddleY = getDiscretePaddleY();
            //}
            //else if (this._paddleY > this._boardY)
            //{
            //    this._paddleY = 1 - this._paddleHeight;
            //    this._discretePaddleY = getDiscretePaddleY();
            //}

            //_prevAction = action;
        }

        private string explorationFunction(Ball b)
        {


            var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "UP");
            var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
            var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");
            
            // To try things equally, use random value to pick action and see if it hasn't been explored too much
            // Or get the action that has been explored the least

            Dictionary<string, int> nextAction = new Dictionary<string, int>();
            try
            {
                nextAction.Add("UP", this._N[discreteStateTupleUp]);
            }
            catch
            {
                nextAction.Add("UP", 0);
            }
            try
            {
                nextAction.Add("DOWN", this._N[discreteStateTupleDown]);
            }
            catch
            {
                nextAction.Add("DOWN", 0);
            }
            try
            {
                nextAction.Add("HOLD", this._N[discreteStateTupleHold]);
            }
            catch
            {
                nextAction.Add("HOLD", 0);
            }

            Dictionary<string, int> sortedNextAction = nextAction.OrderBy(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            if (sortedNextAction.First().Value < this._explorationLimit)
            {
                var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, sortedNextAction.First().Key);
                
                try
                {
                    this._N.Add(discreteStateTuple, 1);
                }
                catch
                {
                    int tmpCount = this._N[discreteStateTuple];
                    this._N.Remove(discreteStateTuple);
                    this._N.Add(discreteStateTuple, tmpCount + 1);
                }
                return sortedNextAction.First().Key;
            }

            //            try
            //            {
            //                this._N.Add(discreteStateTupleUp, 1);
            //            }
            //            catch
            //            {
            //                int tmpCount = this._N[discreteStateTupleUp];
            //                this._N.Remove(discreteStateTupleUp);
            //                this._N.Add(discreteStateTupleUp, tmpCount + 1);
            //            }

            ////if (Math.Round(this._paddleY, 2) - 0.04 > 0.08)
            ////{
            //// Explore Up/Down/Hold actions
            //    try
            //    {
            //        if (this._N[discreteStateTupleUp] < this._explorationLimit)
            //        {
            //            try
            //            {
            //                this._N.Add(discreteStateTupleUp, 1);
            //            }
            //            catch
            //            {
            //                int tmpCount = this._N[discreteStateTupleUp];
            //                this._N.Remove(discreteStateTupleUp);
            //                this._N.Add(discreteStateTupleUp, tmpCount + 1);
            //            }

            //            //this._paddleY -= 0.04;
            //            //if (this._paddleY < 0)
            //            //{
            //            //    this._paddleY = 0;
            //            //}
            //            //this._discretePaddleY = getDiscretePaddleY();

            //            //updateQ(b, "UP");
            //            return "UP";
            //        }
            //    }
            //    catch
            //    {
            //        this._N.Add(discreteStateTupleUp, 1);

            //        //this._paddleY -= 0.04;

            //        //if (this._paddleY < 0)
            //        //{
            //        //    this._paddleY = 0;
            //        //}
            //        //this._discretePaddleY = getDiscretePaddleY();

            //        //updateQ(b, "UP");
            //        return "UP";
            //    }

            ////}
            ////if (Math.Round(this._paddleY + 0.04, 2) <= 1 - GlobalValues.PaddleHeight+.06)
            ////{
            //    try
            //    {
            //        if (this._N[discreteStateTupleDown] < this._explorationLimit)
            //        {
            //            try
            //            {
            //                this._N.Add(discreteStateTupleDown, 1);
            //            }
            //            catch
            //            {
            //                int tmpCount = this._N[discreteStateTupleDown];
            //                this._N.Remove(discreteStateTupleDown);
            //                this._N.Add(discreteStateTupleDown, tmpCount + 1);
            //            }

            //            //this._paddleY += 0.04;
            //            //if (this._paddleY > 1-GlobalValues.PaddleHeight)
            //            //{
            //            //    this._paddleY = 1 - GlobalValues.PaddleHeight;
            //            //}
            //            //this._discretePaddleY = getDiscretePaddleY();

            //            //updateQ(b, "DOWN");
            //            return "DOWN";
            //        }
            //    }
            //    catch
            //    {
            //        this._N.Add(discreteStateTupleDown, 1);

            //        //this._paddleY += 0.04;
            //        //this._discretePaddleY = getDiscretePaddleY();

            //        //updateQ(b, "DOWN");
            //        return "DOWN";
            //    }
            ////}
            //try
            //{
            //    if (this._N[discreteStateTupleHold] < this._explorationLimit)
            //    {
            //        try
            //        {
            //            this._N.Add(discreteStateTupleHold, 1);
            //        }
            //        catch
            //        {
            //            int tmpCount = this._N[discreteStateTupleHold];
            //            this._N.Remove(discreteStateTupleHold);
            //            this._N.Add(discreteStateTupleHold, tmpCount + 1);
            //        }
            //        //updateQ(b, "HOLD");
            //        return "HOLD";
            //    }
            //}
            //catch
            //{
            //    this._N.Add(discreteStateTupleHold, 1);
            //    //updateQ(b, "HOLD");
            //    return "HOLD";
            //}

            // Exploit based on Q  aka Greedy          
            return chooseBasedOnQ(b);                
            
        }

        private string chooseBasedOnQ(Ball b)
        {

            double upQ;
            double downQ;
            double holdQ;

            //var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "UP");
            //var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
            //var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");

            double newPaddleY = this._paddleY;
            int discreteNewPaddleY = this._discretePaddleY;

            newPaddleY -= 0.04;
            if (newPaddleY < 0)
            {
                newPaddleY = 0;
            }
            discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            newPaddleY = this._paddleY;
            newPaddleY += 0.04;
            if (newPaddleY > 1 - GlobalValues.PaddleHeight)
            {
                newPaddleY = 1 - GlobalValues.PaddleHeight;
            }
            discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY);
            
            moveDict.Clear();
            this._Q.TryGetValue(discreteStateTupleUp, out upQ);
            moveDict.Add("UP", upQ);
            this._Q.TryGetValue(discreteStateTupleDown, out downQ);
            moveDict.Add("DOWN", upQ);
            this._Q.TryGetValue(discreteStateTupleHold, out holdQ);
            moveDict.Add("HOLD", upQ);

            Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            //double currentQ;
            //double upQ;
            //double downQ;
            //double holdQ;


            ////Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            ////nextBallState.MoveBall();
            //var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, _prevAction);

            //this._Q.TryGetValue(discreteStateTuple, out currentQ);

            //moveDict.Clear();
            //// Simulate Next Decision
            //var discreteNextStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, (getDiscretePaddleY(this.PaddleY - 0.04) < 0 ? 0 : getDiscretePaddleY(this.PaddleY - 0.04)), "UP");
            //this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
            //moveDict.Add("UP", upQ);

            //var discreteNextStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, (getDiscretePaddleY(this.PaddleY + 0.04) > GlobalValues.DiscreteBoardY - 1 ? GlobalValues.DiscreteBoardY - 1 : getDiscretePaddleY(this.PaddleY + 0.04)), "DOWN");
            //this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
            //moveDict.Add("DOWN", downQ);

            //var discreteNextStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, getDiscretePaddleY(this.PaddleY), "HOLD");
            //this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
            //moveDict.Add("HOLD", holdQ);

            //Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            ////if (sortedMoveDict.First().Key.Equals("UP"))
            ////{
            ////    this._paddleY -= 0.04;
            ////    if (this._paddleY < 0)
            ////    {
            ////        this._paddleY = 0;
            ////    }
            ////    this._discretePaddleY = getDiscretePaddleY();
            ////}
            ////else if (sortedMoveDict.First().Key.Equals("DOWN"))
            ////{
            ////    this._paddleY += 0.04;
            ////    if (this._paddleY > 1 - GlobalValues.PaddleHeight)
            ////    {
            ////        this._paddleY = 1 - GlobalValues.PaddleHeight;
            ////    }
            ////    this._discretePaddleY = getDiscretePaddleY();
            ////}
            ////else
            ////{
            ////    this._paddleY = this._paddleY;
            ////}
            ////this._discretePaddleY = getDiscretePaddleY();

            ////updateQ(b, sortedMoveDict.First().Key);
            return sortedMoveDict.First().Key;

        }

        public void tdUpdate(Ball b, string actionChosen)
        {
            // Q(s,a)
            double currQVal = 0.0;
            //var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, actionChosen);

            //double newPaddleY = this._paddleY;
            //int discreteNewPaddleY = this._discretePaddleY;

            //if (actionChosen.Equals("UP"))
            //{
            //    newPaddleY -= 0.04;
            //    if (newPaddleY < 0)
            //    {
            //        newPaddleY = 0;
            //    }
            //    discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            //    var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);
            //}
            //else if (actionChosen.Equals("DOWN"))
            //{
            //    newPaddleY = this._paddleY;
            //    if (newPaddleY > 1 - GlobalValues.PaddleHeight)
            //    {
            //        newPaddleY = 1 - GlobalValues.PaddleHeight;
            //    }
            //    discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            //    var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            //}
            // s
            
            // Should I have moved it based on the action to get Q(s,a).  Currently, no action was taken.
            var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY);

            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }

            // R(s) Current Reward in initial state, regardless of action
            int stateReward = 0;
            if (b.BallX > this._paddleX)
            {
                // right player deflects it
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

            // Pretend to be in successor state 
            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            nextBallState.MoveBall();

            double newPaddleY = this._paddleY;
            int discreteNewPaddleY = this.DiscretePaddleY;

            if (actionChosen.Equals("UP"))
            {

                newPaddleY -= 0.04;
                if (newPaddleY < 0)
                {
                    newPaddleY = 0;
                }
                discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));
            }
            else if (actionChosen.Equals("DOWN"))
            {
                newPaddleY += 0.04;
                if (newPaddleY > 1 - GlobalValues.PaddleHeight)
                {
                    newPaddleY = 1 - GlobalValues.PaddleHeight;
                }
                discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));
            }
            else
            {
                // HOLD
                // Do nothing
            }

            // s'
            //var discreteSuccessorStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, actionChosen);
            var discreteSuccessorStateTuple = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNewPaddleY, actionChosen);

            // Get max Action in successor state
            double upQ;
            double downQ;
            double holdQ;

            moveDict.Clear();
            // Simulate Next Decision, get Max Q
            // Q(s',a')

            //var discreteNextStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "UP");
            //this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
            //moveDict.Add("UP", upQ);

            //var discreteNextStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "DOWN");
            //this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
            //moveDict.Add("DOWN", downQ);

            //var discreteNextStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "HOLD");
            //this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
            //moveDict.Add("HOLD", holdQ);

            // Get Successor State actions by changing Y 
            double nextPaddleY = newPaddleY;
            int discreteNextPaddleY = discreteNewPaddleY;

            // TODO: move the ball again?

            // Get successor state UP action

            nextPaddleY -= 0.04;
            if (nextPaddleY < 0)
            {
                nextPaddleY = 0;
            }
            discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
            var discreteNextStateTupleUp = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
            this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
            moveDict.Add("UP", upQ);

            // Get successor state DOWN action
            nextPaddleY = newPaddleY;
            nextPaddleY += 0.04;
            if (nextPaddleY > 1 - GlobalValues.PaddleHeight)
            {
                nextPaddleY = 1 - GlobalValues.PaddleHeight;
            }
            discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
            var discreteNextStateTupleDown = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
            this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
            moveDict.Add("DOWN", upQ);

            // Get successor state HOLD action
            nextPaddleY = newPaddleY;
            discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
            var discreteNextStateTupleHold = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
            this._Q.TryGetValue(discreteNextStateTupleHold, out downQ);
            moveDict.Add("HOLD", upQ);

            Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            // MaxQ(s',a')
            double maxNextQValue = sortedMoveDict.First().Value;

            var discreteStateActionTuple = Tuple.Create(discreteStateTuple.Item1, discreteStateTuple.Item2, discreteStateTuple.Item3, discreteStateTuple.Item4, discreteStateTuple.Item5, actionChosen);

            // Number of times seen this state/action pair
            int stateActionCount = 0;

            try
            {
                this._N.TryGetValue(discreteStateActionTuple, out stateActionCount);
            }
            catch
            {
                ;
            }

            double decayedLearningRate = this._learningConstant/ (this._learningConstant + stateActionCount);

            this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * maxNextQValue - currQVal);

            //// Check if states have value, else default it to 0
            //double currQVal = 0.0;
            //double nextQVal = 0.0;
            //int learningRateCount = 0;

            //// Calculate Q
            //try
            //{
            //    this._Q.TryGetValue(discreteStateTuple, out currQVal);
            //}
            //catch
            //{
            //    ;
            //}


            //try
            //{
            //    this._Q.TryGetValue(discreteSuccessorStateTuple, out nextQVal);
            //}
            //catch
            //{
            //    ;
            //}

            //// Number of times seen this state/action pair
            //try
            //{
            //    this._N.TryGetValue(discreteStateTuple, out learningRateCount);
            //}
            //catch
            //{ 
            //    ;
            //}


            //double decayedLearningRate = this._learningRate / (this._learningRate + learningRateCount);

            //var tmpUp = Tuple.Create(6,6,1,0,6, "UP");
            //var tmpDown = Tuple.Create(6, 6, 1, 0, 6, "DOWN");
            //var tmpHold = Tuple.Create(6, 6, 1, 0, 6, "HOLD");

            //nextQVal = maxNextQValue;

            //this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * nextQVal - currQVal);


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
