using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        //private Dictionary<Tuple<int, int, int, int, int>, double> _Q;
        private Dictionary<Tuple<int, int, int, int, int, string>, int> _N;
        private Dictionary<Tuple<int, int, int, int, int>, string> currQ = new Dictionary<Tuple<int, int, int, int, int>, string>();
        private Dictionary<Tuple<int, int, int, int, int>, string> nextQ = new Dictionary<Tuple<int, int, int, int, int>, string>();
        private Dictionary<string, int> nextAction = new Dictionary<string, int>();
        private Dictionary<string, double> nextAction2 = new Dictionary<string, double>();

        private int _totalTrialDeflections;
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
            this._Q = new Dictionary<Tuple<int, int, int, int, int, string>, double>();

            //using (var sr = new StreamReader(@"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\Q2.txt"))
            //{
            //    string line = null;

            //    // while it reads a key
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        string tuple = line.Split(')')[0].Trim('(');
            //        double value = Double.Parse(line.Split(')')[1]);

            //        Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(',')[0]), Int32.Parse(tuple.Split(',')[1]), Int32.Parse(tuple.Split(',')[2]), Int32.Parse(tuple.Split(',')[3]), Int32.Parse(tuple.Split(',')[4]), tuple.Split(',')[5].ToString().Trim());
            //        this._Q.Add(newTuple, value);
            //        //d.Add(line, sr.ReadLine());
            //    }
            //}
            this._N = new Dictionary<Tuple<int, int, int, int, int, string>, int>();

            //using (var sr = new StreamReader(@"I:\Backup\Masters\UIUC\2016\Fall\CS_440\Homework\4\CS440-HW4\N2.txt"))
            //{
            //    string line = null;

            //    // while it reads a key
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        string tuple = line.Split(')')[0].Trim('(');
            //        int value = Int32.Parse(line.Split(')')[1]);

            //        Tuple<int, int, int, int, int, string> newTuple = Tuple.Create(Int32.Parse(tuple.Split(' ')[0].Trim(',')), Int32.Parse(tuple.Split(' ')[1].Trim(',')), Int32.Parse(tuple.Split(' ')[2].Trim(',')), Int32.Parse(tuple.Split(' ')[3].Trim(',')), Int32.Parse(tuple.Split(' ')[4].Trim(',')), tuple.Split(' ')[5].ToString());
            //        this._N.Add(newTuple, value);
            //        //d.Add(line, sr.ReadLine());
            //    }
            //}

            //this._Q = new Dictionary<Tuple<int, int, int, int, int>, double>();
            //this._N = new Dictionary<Tuple<int, int, int, int, int, string>, int>();
        }

        public double LearningConstant
        {
            get { return this._learningConstant; }
            set { this._learningConstant = value; }
        }

        public double DiscountFactor
        {
            get { return this._discountFactor; }
            set { this._discountFactor = value; }
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

        public int TotalTrialDeflections
        {
            get { return this._totalTrialDeflections; }
            set { this._totalTrialDeflections = value; }
        }


        public string PrevAction
        {
            get { return this._prevAction; }
            set { this._prevAction = value; }
        }

        public int ExplorationLimit
        {
            get { return this._explorationLimit; }
            set { this._explorationLimit = value; }
        }


        public Dictionary<Tuple<int, int, int, int, int, string>, double> Memory
        //public Dictionary<Tuple<int, int, int, int, int>, double> Memory
        {
            get { return this._Q; }
            set { this._Q = value; }
        }

        public Dictionary<Tuple<int, int, int, int, int, string>, int> MemoryVisited
        //public Dictionary<Tuple<int, int, int, int, int>, double> Memory
        {
            get { return this._N; }
            set { this._N = value; }
        }


        public void MoveLeftPaddle(Ball b)
        {
            if (this._paddleY - GlobalValues.PaddleHeight/2 < b.BallY)
            {
                this.PaddleY += 0.02;
                if (this._paddleY > 1 - GlobalValues.PaddleHeight)
                {
                    this._paddleY = 1 - GlobalValues.PaddleHeight;
                    this._discretePaddleY = this._boardY - 1;
                }
            }
            else if (this._paddleY - GlobalValues.PaddleHeight / 2 > b.BallY)
            {
                this.PaddleY -= 0.02;
                if (this._paddleY < 0)
                {
                    this._paddleY = 0;
                }
            }
            else
            {
                this.PaddleY = this.PaddleY;
            }

            this._discretePaddleY = getDiscretePaddleY();
        }

        public void MoveRightPaddle(Ball b)
        {
            // Each time step, choose action using exploration/exploitation function
            //string action = explorationFunction(b);

            // get successor state and perform TD Update
            //tdUpdate(b, action);


            //if (action.Equals("UP"))
            if (this._prevAction.Equals("UP"))
            {

                this._paddleY -= 0.04;
                if (this._paddleY < 0)
                {
                    this._paddleY = 0;
                }
                this._discretePaddleY = getDiscretePaddleY();
            }
            else if (this._prevAction.Equals("DOWN"))
            {
                this._paddleY += 0.04;
                this._discretePaddleY = getDiscretePaddleY();

                if (this._paddleY > 1 - GlobalValues.PaddleHeight)
                {
                    this._paddleY = 1 - GlobalValues.PaddleHeight;
                    this._discretePaddleY = this._boardX - 1;
                }
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

        public double SimulateMoveRightPaddle(double paddleY, string action)
        {
            // Each time step, choose action using exploration/exploitation function
            //string action = explorationFunction(b);

            // get successor state and perform TD Update
            //tdUpdate(b, action);


            if (action.Equals("UP"))
            {

                paddleY -= 0.04;
                if (paddleY < 0)
                {
                    paddleY = 0;
                }
            }
            else if (action.Equals("DOWN"))
            {
                paddleY += 0.04;

                if (paddleY > 1 - GlobalValues.PaddleHeight)
                {
                    paddleY = 1 - GlobalValues.PaddleHeight;
                }
            }
            else
            {
                // HOLD
                // Do Nothing
            }

            return paddleY;
        }

        private string explorationFunction(Ball b)
        {

            var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "UP");
            var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
            var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");

            // To try things equally, use random value to pick action and see if it hasn't been explored too much
            // Or get the action that has been explored the least]

            // Make this part of player class instead of creating a new one...clear it out each time.
            // Make it check the Q to see if converged to 1, then try other values.

            nextAction.Clear();
            try
            {
                //if (1 - this._Q[discreteStateTupleHold] < 0.1)
                //{
                //    ;
                //}
                //else
                //{
                    nextAction.Add("HOLD", this._N[discreteStateTupleHold]);
                //}
            }
            catch
            {
                nextAction.Add("HOLD", 0);
            }
            try
            {
                //if (1 - this._Q[discreteStateTupleUp] < 0.1)
                //{
                //    ;
                //}
                //else
                //{
                    nextAction.Add("UP", this._N[discreteStateTupleUp]);
                //}
            }
            catch
            {
                nextAction.Add("UP", 0);
            }
            try
            {
                //if (1 - this._Q[discreteStateTupleDown] < 0.1)
                //{
                //    ;
                //}
                //else
                //{
                    nextAction.Add("DOWN", this._N[discreteStateTupleDown]);
                //}
            }
            catch
            {
                nextAction.Add("DOWN", 0);
            }


            Dictionary<string, int> sortedNextAction = nextAction.OrderBy(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

            if (sortedNextAction.Count > 0 && sortedNextAction.First().Value < this._explorationLimit)
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
            //try
            //{
            //    if (this._N[discreteStateTupleUp] < this._explorationLimit)
            //    {
            //        try
            //        {
            //            this._N.Add(discreteStateTupleUp, 1);
            //        }
            //        catch
            //        {
            //            int tmpCount = this._N[discreteStateTupleUp];
            //            this._N.Remove(discreteStateTupleUp);
            //            this._N.Add(discreteStateTupleUp, tmpCount + 1);
            //        }

            //        //this._paddleY -= 0.04;
            //        //if (this._paddleY < 0)
            //        //{
            //        //    this._paddleY = 0;
            //        //}
            //        //this._discretePaddleY = getDiscretePaddleY();

            //        //updateQ(b, "UP");
            //        return "UP";
            //    }
            //}
            //catch
            //{
            //    this._N.Add(discreteStateTupleUp, 1);

            //    //this._paddleY -= 0.04;

            //    //if (this._paddleY < 0)
            //    //{
            //    //    this._paddleY = 0;
            //    //}
            //    //this._discretePaddleY = getDiscretePaddleY();

            //    //updateQ(b, "UP");
            //    return "UP";
            //}

            ////}
            ////if (Math.Round(this._paddleY + 0.04, 2) <= 1 - GlobalValues.PaddleHeight+.06)
            ////{
            //try
            //{
            //    if (this._N[discreteStateTupleDown] < this._explorationLimit)
            //    {
            //        try
            //        {
            //            this._N.Add(discreteStateTupleDown, 1);
            //        }
            //        catch
            //        {
            //            int tmpCount = this._N[discreteStateTupleDown];
            //            this._N.Remove(discreteStateTupleDown);
            //            this._N.Add(discreteStateTupleDown, tmpCount + 1);
            //        }

            //        //this._paddleY += 0.04;
            //        //if (this._paddleY > 1-GlobalValues.PaddleHeight)
            //        //{
            //        //    this._paddleY = 1 - GlobalValues.PaddleHeight;
            //        //}
            //        //this._discretePaddleY = getDiscretePaddleY();

            //        //updateQ(b, "DOWN");
            //        return "DOWN";
            //    }
            //}
            //catch
            //{
            //    this._N.Add(discreteStateTupleDown, 1);

            //    //this._paddleY += 0.04;
            //    //this._discretePaddleY = getDiscretePaddleY();

            //    //updateQ(b, "DOWN");
            //    return "DOWN";
            //}
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

        //private string explorationFunction2(int stateActionPairCount)
        //{

        //    ;

        //}

        private string chooseBasedOnQ(Ball b)
        {

            double upQ;
            double downQ;
            double holdQ;

            var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "UP");
            var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
            var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");

            //double newPaddleY = this._paddleY;
            //int discreteNewPaddleY = this._discretePaddleY;

            //newPaddleY -= 0.04;
            //if (newPaddleY < 0)
            //{
            //    newPaddleY = 0;
            //}
            //discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            //var discreteStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            //newPaddleY = this._paddleY;
            //if (newPaddleY > 1 - GlobalValues.PaddleHeight)
            //{
            //    newPaddleY = 1 - GlobalValues.PaddleHeight;
            //}
            //discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            //var discreteStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            //var discreteStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY);

            moveDict.Clear();
            this._Q.TryGetValue(discreteStateTupleUp, out upQ);
            moveDict.Add("UP", upQ);
            this._Q.TryGetValue(discreteStateTupleDown, out downQ);
            moveDict.Add("DOWN", downQ);
            this._Q.TryGetValue(discreteStateTupleHold, out holdQ);
            moveDict.Add("HOLD", holdQ);

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
            var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, actionChosen);
            //if (b.DiscreteBallX == 12)
            //{
            //    b.DiscreteBallX = 11;
            //}
            //var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY);
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

            //    discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);
            //}
            //else if (actionChosen.Equals("DOWN"))
            //{
            //    newPaddleY = this._paddleY;
            //    newPaddleY += 0.04;
            //    discreteNewPaddleY = (Int32)Math.Floor(this._boardY * newPaddleY / (1 - this._paddleHeight));

            //    if (newPaddleY > 1 - GlobalValues.PaddleHeight)
            //    {
            //        newPaddleY = 1 - GlobalValues.PaddleHeight;
            //        discreteNewPaddleY = 11;
            //    }

            //    discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY);

            //}
            // s
            //var discreteStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY);

            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }

            //// Pretend to be in successor state 
            //Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            //nextBallState.MoveBall();

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
            //var discreteSuccessorStateTuple = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNewPaddleY, actionChosen);

            // R(s) Current Reward
            int stateReward = 0;
            double maxNextQValue = 0;
            // Pretend to be in successor state 
            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            nextBallState.MoveBall();
            if (nextBallState.BallX > this._paddleX)
            {
                // right player deflects it
                // Calculate Y coordinate when ball is at the same level as the paddle
                //double yIntersect = b.VelocityY * b.BallX + b.BallY;
                double yIntersect = (nextBallState.VelocityY / nextBallState.VelocityX) * (-1 * nextBallState.VelocityX) + nextBallState.BallY;

                if (yIntersect <= newPaddleY + GlobalValues.PaddleHeight && yIntersect >= newPaddleY)
                //if (yIntersect <= newPaddleY + GlobalValues.PaddleHeight && yIntersect >= newPaddleY)
                {
                    stateReward = 1;
                }
                else
                {
                    stateReward = -1;
                }

            }

            if (nextBallState.DiscreteBallX == this.DiscretePaddleX)
            {
                if (nextBallState.DiscreteBallY == discreteNewPaddleY)
                {
                    stateReward = 1;
                }
                else
                {
                    stateReward = -1;
                }
            }
            else
            {
                stateReward = 0;
            }

            if (stateReward > -1)
            {
                // Get max Action in successor state
                double upQ;
                double downQ;
                double holdQ;


                moveDict.Clear();
                // Simulate Next Decision, get Max Q
                // Q(s',a')

                var discreteNextStateTupleUp = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "UP");
                this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
                moveDict.Add("UP", upQ);

                var discreteNextStateTupleDown = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "DOWN");
                this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
                moveDict.Add("DOWN", downQ);

                var discreteNextStateTupleHold = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNewPaddleY, "HOLD");
                this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
                moveDict.Add("HOLD", holdQ);

                //// Get Successor State actions by changing Y 
                //double nextPaddleY = newPaddleY;
                //int discreteNextPaddleY = discreteNewPaddleY;

                //// Get successor state UP action

                //nextPaddleY -= 0.04;
                //if (nextPaddleY < 0)
                //{
                //    nextPaddleY = 0;
                //}
                //discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
                //var discreteNextStateTupleUp = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
                //this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
                //moveDict.Add("UP", upQ);

                //// Get successor state DOWN action
                //nextPaddleY = newPaddleY;
                //nextPaddleY += 0.04;
                //discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
                //if (nextPaddleY > 1 - GlobalValues.PaddleHeight)
                //{
                //    nextPaddleY = 1 - GlobalValues.PaddleHeight;
                //    discreteNextPaddleY = 11;
                //}

                //var discreteNextStateTupleDown = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
                //this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
                //moveDict.Add("DOWN", downQ);

                //// Get successor state HOLD action
                //nextPaddleY = newPaddleY;
                //discreteNextPaddleY = (Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight));
                //var discreteNextStateTupleHold = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, discreteNextPaddleY);
                //this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
                //moveDict.Add("HOLD", holdQ);

                Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

                // MaxQ(s',a')
                maxNextQValue = sortedMoveDict.First().Value;
            }

            if (maxNextQValue > 0)
            {
                ;
            }

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
            double decayedLearningRate = this._learningConstant / (this._learningConstant + stateActionCount);

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

        public void tdUpdate(Ball b, Tuple<int, int, int, int, int, int> prevState, Tuple<int, int, int, int, int, int> currState)
        {
            string actionChosen = "";
            if (currState.Item6 > prevState.Item6)
            {
                actionChosen = "DOWN";
            }
            else if (currState.Item6 < prevState.Item6)
            {
                actionChosen = "UP";
            }
            else if (currState.Item6 == prevState.Item6)
            {
                actionChosen = "HOLD";
            }
            var discreteStateTuple = Tuple.Create(prevState.Item1, prevState.Item2, prevState.Item3, prevState.Item4, prevState.Item6, actionChosen);
            var discreteNextStateTuple = Tuple.Create(currState.Item1, currState.Item2, currState.Item3, currState.Item4, currState.Item6, actionChosen);

            // Q(s,a)
            double currQVal = 0.0;

            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }

            //// Pretend to be in successor state 
            // R(s) Reward
            int stateReward = 0;
            double maxNextQValue = 0;

            if (discreteStateTuple.Item1 == currState.Item5)
            {
                if (discreteStateTuple.Item2 == discreteNextStateTuple.Item5)
                {
                    stateReward = 1;
                }
                else
                {
                    stateReward = -1;
                }
            }
            else
            {
                stateReward = 0;
            }


            if (stateReward > -1)
            {
                // Get max Action in successor state
                double upQ;
                double downQ;
                double holdQ;


                moveDict.Clear();
                // Simulate Next Decision, get Max Q
                // Q(s',a')

                var discreteNextStateTupleUp = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "UP");
                this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
                moveDict.Add("UP", upQ);

                var discreteNextStateTupleDown = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "DOWN");
                this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
                moveDict.Add("DOWN", downQ);

                var discreteNextStateTupleHold = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "HOLD");
                this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
                moveDict.Add("HOLD", holdQ);

                Dictionary<string, double> sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

                // MaxQ(s',a')
                maxNextQValue = sortedMoveDict.First().Value;
            }

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
            double decayedLearningRate = this._learningConstant / (this._learningConstant + stateActionCount);

            this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * maxNextQValue - currQVal);

        }
        public void tdUpdate2(Ball b, Tuple<int, int, int, int, int, int> prevState, Tuple<int, int, int, int, int, int> currState)
        {
            string actionChosen = "";
            if (currState.Item6 > prevState.Item6)
            {
                actionChosen = "DOWN";
            }
            else if (currState.Item6 < prevState.Item6)
            {
                actionChosen = "UP";
            }
            else if (currState.Item6 == prevState.Item6)
            {
                actionChosen = "HOLD";
            }
            //var discreteStateTuple = Tuple.Create(prevState.Item1, prevState.Item2, prevState.Item3, prevState.Item4, prevState.Item6, actionChosen);
            //var discreteNextStateTuple = Tuple.Create(currState.Item1, currState.Item2, currState.Item3, currState.Item4, currState.Item6, actionChosen);
            var discreteStateTuple = Tuple.Create(currState.Item1, currState.Item2, currState.Item3, currState.Item4, currState.Item6, actionChosen);

            // Q(s,a)
            double currQVal = 0.0;
            
            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }

            // Pretend to be in successor state 
            // R(s) Reward

            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);

            // Previous Discrete Values
            int currDiscreteBallX = nextBallState.DiscreteBallX;
            int currDiscreteBallY = nextBallState.DiscreteBallY;
            int currDiscretePaddleX = this.DiscretePaddleX;
            int currDiscretePaddleY = this.DiscretePaddleY;
            int currDiscreteVelX = nextBallState.DiscreteVelocityX;
            int currDiscreteVelY = nextBallState.DiscreteVelocityY;                       

            int nextDiscreteBallX = nextBallState.DiscreteBallX;
            int nextDiscreteBallY = nextBallState.DiscreteBallY;
            int nextDiscretePaddleX = this.DiscretePaddleX;
            int nextDiscretePaddleY = this.DiscretePaddleY;
            int nextDiscreteVelX = nextBallState.DiscreteVelocityX;
            int nextDiscreteVelY = nextBallState.DiscreteVelocityY;

            int stateReward = 0;
            while (currDiscreteBallX != nextDiscreteBallX ||
                        currDiscreteBallY != nextDiscreteBallY ||
                        currDiscretePaddleX != nextDiscretePaddleX ||
                        currDiscretePaddleY != nextDiscretePaddleY ||
                        currDiscreteVelX != nextDiscreteVelX ||
                        currDiscreteVelY != nextDiscreteVelY)
            {
                nextBallState.MoveBall();

                nextDiscreteBallX = nextBallState.DiscreteBallX;
                nextDiscreteBallY = nextBallState.DiscreteBallY;
                nextDiscreteVelX = nextBallState.DiscreteVelocityX;
                nextDiscreteVelY = nextBallState.DiscreteVelocityY;

                if (nextBallState.BallX > this._paddleX)
                {
                    // right player deflects it
                    // Calculate Y coordinate when ball is at the same level as the paddle
                    //double yIntersect = b.VelocityY * b.BallX + b.BallY;
                    double yIntersect = (nextBallState.VelocityY / nextBallState.VelocityX) * (-1 * nextBallState.VelocityX) + nextBallState.BallY;

                    if (yIntersect <= this._paddleY + GlobalValues.PaddleHeight && yIntersect >= this._paddleY)
                    //if (yIntersect <= newPaddleY + GlobalValues.PaddleHeight && yIntersect >= newPaddleY)
                    {
                        stateReward = 1;
                    }
                    else
                    {
                        stateReward = -1;
                    }

                }
            }

            //stateReward = 0;
            double maxNextQValue = 0;

            //if (discreteStateTuple.Item1 == currState.Item5)
            //{
            //    if (discreteStateTuple.Item2 == discreteNextStateTuple.Item5)
            //    {
            //        stateReward = 1;
            //    }
            //    else
            //    {
            //        stateReward = -1;
            //    }
            //} 
            //else
            //{
            //    stateReward = 0;
            //}
            var discreteNextStateTupleUp = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "UP");
            var discreteNextStateTupleDown = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "DOWN");
            var discreteNextStateTupleHold = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "HOLD");

            Dictionary<string, double> sortedMoveDict = null;
            if (stateReward > -1)
            {
                // Get max Action in successor state
                double upQ;
                double downQ;
                double holdQ;


                moveDict.Clear();
                // Simulate Next Decision, get Max Q
                // Q(s',a')

                //var discreteNextStateTupleUp = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "UP");
                discreteNextStateTupleUp = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "UP");
                this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
                moveDict.Add("UP", upQ);

                //var discreteNextStateTupleDown = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "DOWN");
                discreteNextStateTupleDown = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "DOWN");
                this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
                moveDict.Add("DOWN", downQ);

                //var discreteNextStateTupleHold = Tuple.Create(discreteNextStateTuple.Item1, discreteNextStateTuple.Item2, discreteNextStateTuple.Item3, discreteNextStateTuple.Item4, discreteNextStateTuple.Item5, "HOLD");
                discreteNextStateTupleHold = Tuple.Create(nextDiscreteBallX, nextDiscreteBallY, nextDiscreteVelX, nextDiscreteVelY, this.DiscretePaddleY, "HOLD");
                this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
                moveDict.Add("HOLD", holdQ);
                
                sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

                // MaxQ(s',a')
                maxNextQValue = sortedMoveDict.First().Value;
            }
            var discreteStateActionTuple = Tuple.Create(discreteStateTuple.Item1, discreteStateTuple.Item2, discreteStateTuple.Item3, discreteStateTuple.Item4, discreteStateTuple.Item5, actionChosen);

            string newAction;
            if (sortedMoveDict.First().Key.Equals("UP"))
            {
                discreteStateActionTuple = Tuple.Create(discreteNextStateTupleUp.Item1, discreteNextStateTupleUp.Item2, discreteNextStateTupleUp.Item3, discreteNextStateTupleUp.Item4, discreteNextStateTupleUp.Item5, discreteNextStateTupleUp.Item6);
            }
            else if (sortedMoveDict.First().Key.Equals("DOWN"))
            {
                discreteStateActionTuple = Tuple.Create(discreteNextStateTupleDown.Item1, discreteNextStateTupleDown.Item2, discreteNextStateTupleDown.Item3, discreteNextStateTupleDown.Item4, discreteNextStateTupleDown.Item5, discreteNextStateTupleDown.Item6);
            }
            else if (sortedMoveDict.First().Key.Equals("HOLD"))
            {
                discreteStateActionTuple = Tuple.Create(discreteNextStateTupleHold.Item1, discreteNextStateTupleHold.Item2, discreteNextStateTupleHold.Item3, discreteNextStateTupleHold.Item4, discreteNextStateTupleHold.Item5, discreteNextStateTupleHold.Item6);
            }

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
            double optimisticReward = -2.0;
            if (stateActionCount < this.ExplorationLimit)
            {
                optimisticReward = 0.5;
            }

            maxNextQValue = Math.Max(maxNextQValue, optimisticReward);

            double decayedLearningRate = this._learningConstant / (this._learningConstant + stateActionCount);

            this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * maxNextQValue - currQVal);          

        }

        public void tdUpdate3(Ball b, Tuple<double, double, double, double, double> prevState, Tuple<double, double, double, double, double> currState)
        {
            string actionChosen = "";
            if (currState.Item5 > prevState.Item5)
            {
                actionChosen = "DOWN";
            }
            else if (currState.Item5 < prevState.Item5)
            {
                actionChosen = "UP";
            }
            else if (currState.Item5 == prevState.Item5)
            {
                actionChosen = "HOLD";
            }
            actionChosen = this._prevAction;
            
            //var discreteStateTuple = Tuple.Create(prevState.Item1, prevState.Item2, prevState.Item3, prevState.Item4, prevState.Item6, actionChosen);
            //var discreteNextStateTuple = Tuple.Create(currState.Item1, currState.Item2, currState.Item3, currState.Item4, currState.Item6, actionChosen);
            var discreteStateTuple = Tuple.Create(Math.Min(b.DiscreteBallX, this._boardX-1), b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, this.DiscretePaddleY, actionChosen);

            // Q(s,a)
            double currQVal = 0.0;

            int stateActionCount = 0;

            // Get learning rate for this state
            try
            {
                this._Q.TryGetValue(discreteStateTuple, out currQVal);
            }
            catch
            {
                ;
            }

            try
            {
                this._N.TryGetValue(discreteStateTuple, out stateActionCount);
            }
            catch
            {
                ;
            } 
            double decayedLearningRate = this._learningConstant / (this._learningConstant + stateActionCount);


            // Pretend to be in successor state 
            // R(s) Reward
            Ball nextBallState = new Pong.Ball(b.BallX, b.BallY, b.VelocityX, b.VelocityY, this._boardX, this._boardY, b.TwoPlayers);
            nextBallState.MoveBall();
            int stateReward = 0;
            if (nextBallState.BallX > this._paddleX)
            {
                // right player deflects it
                // Calculate Y coordinate when ball is at the same level as the paddle
                //double yIntersect = b.VelocityY * b.BallX + b.BallY;
                double yIntersect = (nextBallState.VelocityY / nextBallState.VelocityX) * (-1 * nextBallState.VelocityX) + nextBallState.BallY;

                if (yIntersect <= this._paddleY + GlobalValues.PaddleHeight && yIntersect >= this._paddleY)
                //if (yIntersect <= newPaddleY + GlobalValues.PaddleHeight && yIntersect >= newPaddleY)
                {
                    stateReward = 1;
                }
                else
                {
                    stateReward = -1;
                }

            }

            if (nextBallState.BallX >= 1)
            {
                nextBallState.BallX = 1.0;
                nextBallState.DiscreteBallX = this._boardX-1;
            }


            // Explore by getting max a' Q(s',a')
            // Use f(Q,n), where Q is the highest value  of any action in this next state.  n return some hardcoded , optimistic reward for each state if we didn't explore each state enough
            double maxNextQValue = 0;


            // TODO: if defection happens, does direction of the velocity in X direction change?
            int discreteVelX = 1;
            if (stateReward == 1)
            {
                discreteVelX = -1;
            }
            // TODO: Simulate moving the paddle too, currently doing Q(s,a').  NOT Q(s',a')
            double nextPaddleY;
            nextPaddleY = SimulateMoveRightPaddle(this.PaddleY, "UP");
            int discreteNextPaddleY = Math.Min((Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight)), this._boardY - 1);

            var discreteNextStateTupleUp = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, discreteVelX, nextBallState.DiscreteVelocityY, discreteNextPaddleY, "UP");

            nextPaddleY = SimulateMoveRightPaddle(this.PaddleY, "DOWN");
            discreteNextPaddleY = Math.Min((Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight)), this._boardY - 1);

            var discreteNextStateTupleDown = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, discreteVelX, nextBallState.DiscreteVelocityY, discreteNextPaddleY, "DOWN");
            
            nextPaddleY = SimulateMoveRightPaddle(this.PaddleY, "HOLD");
            discreteNextPaddleY = Math.Min((Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight)), this._boardY - 1);
            
            var discreteNextStateTupleHold = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, discreteVelX, nextBallState.DiscreteVelocityY, discreteNextPaddleY, "HOLD");

            Dictionary<string, double> sortedMoveDict = null;
            string newAction = "";

            // if state reward = -1, then max a' Q(s',a') is 0
            if (stateReward > -1)
            {
                // Get max Action in successor state
                double upQ;
                double downQ;
                double holdQ;


                moveDict.Clear();
                // Simulate Next Decision, get Max Q
                // Q(s',a')

                //if (this._paddleY < 0)
                //{
                    // Can go up
                    //discreteNextStateTupleUp = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, this.DiscretePaddleY, "UP");

                    this._Q.TryGetValue(discreteNextStateTupleUp, out upQ);
                    moveDict.Add("UP", upQ);
                //}

                //if (this._paddleY > 1 - GlobalValues.PaddleHeight)
                //{
                    // Can go down
                    //discreteNextStateTupleDown = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, this.DiscretePaddleY, "DOWN");
                    this._Q.TryGetValue(discreteNextStateTupleDown, out downQ);
                    moveDict.Add("DOWN", downQ);
                //}

                //discreteNextStateTupleHold = Tuple.Create(nextBallState.DiscreteBallX, nextBallState.DiscreteBallY, nextBallState.DiscreteVelocityX, nextBallState.DiscreteVelocityY, this.DiscretePaddleY, "HOLD");
                this._Q.TryGetValue(discreteNextStateTupleHold, out holdQ);
                moveDict.Add("HOLD", holdQ);

                sortedMoveDict = moveDict.OrderByDescending(v => v.Value).ToDictionary(x => x.Key, x => x.Value);

                // MaxQ(s',a')
                maxNextQValue = sortedMoveDict.First().Value;

                
                var discreteNextStateActionTuple = Tuple.Create(discreteStateTuple.Item1, discreteStateTuple.Item2, discreteStateTuple.Item3, discreteStateTuple.Item4, discreteStateTuple.Item5, actionChosen);
                        
                // Number of times taken action a' in state s
                int stateActionCountUp = 0;
                int stateActionCountDown = 0;
                int stateActionCountHold = 0;
                //if (sortedMoveDict.First().Key.Equals("UP"))
                //{
                    discreteNextStateActionTuple = Tuple.Create(discreteNextStateTupleUp.Item1, discreteNextStateTupleUp.Item2, discreteNextStateTupleUp.Item3, discreteNextStateTupleUp.Item4, discreteNextStateTupleUp.Item5, discreteNextStateTupleUp.Item6);
                    try
                    {
                        this._N.TryGetValue(discreteNextStateTupleUp, out stateActionCountUp);
                    }
                    catch
                    {
                        ;
                    }
                //}
                //else if (sortedMoveDict.First().Key.Equals("DOWN"))
                //{
                    discreteNextStateActionTuple = Tuple.Create(discreteNextStateTupleDown.Item1, discreteNextStateTupleDown.Item2, discreteNextStateTupleDown.Item3, discreteNextStateTupleDown.Item4, discreteNextStateTupleDown.Item5, discreteNextStateTupleDown.Item6);
                    try
                    {
                        this._N.TryGetValue(discreteNextStateTupleDown, out stateActionCountDown);
                    }
                    catch
                    {
                        ;
                    }
                //}
                //else if (sortedMoveDict.First().Key.Equals("HOLD"))
                //{
                    discreteNextStateActionTuple = Tuple.Create(discreteNextStateTupleHold.Item1, discreteNextStateTupleHold.Item2, discreteNextStateTupleHold.Item3, discreteNextStateTupleHold.Item4, discreteNextStateTupleHold.Item5, discreteNextStateTupleHold.Item6);
                    try
                    {
                        this._N.TryGetValue(discreteNextStateTupleHold, out stateActionCountHold);
                    }
                    catch
                    {
                        ;
                    }            
                //}

                nextAction2.Clear();

                // each action
                double optimisticRewardUp = -2.0;
                double optimisticRewardDown = -2.0;
                double optimisticRewardHold = -2.0;
                if (stateActionCountUp < this.ExplorationLimit)
                {
                    try 
                    {
                        //if (this._Q[discreteNextStateActionTuple] > 0)
                        //{
                            optimisticRewardUp = 0.83;
                            nextAction2.Add("UP", optimisticRewardUp);
                        //}
                    }
                    catch
                    {
                        optimisticRewardUp = 0.83;
                        nextAction2.Add("UP", optimisticRewardUp);
                    }
                    

                }
                if (stateActionCountDown < this.ExplorationLimit)
                {
                    try
                    {
                        //if (this._Q[discreteNextStateActionTuple] > 0)
                        //{
                            optimisticRewardDown = 0.82;
                            nextAction2.Add("DOWN", optimisticRewardDown);
                        //}
                    }
                    catch
                    {
                        optimisticRewardDown = 0.82;
                        nextAction2.Add("DOWN", optimisticRewardDown);
                    }
                }
                if (stateActionCountHold < this.ExplorationLimit)
                {
                    try
                    {
                        //if (this._Q[discreteNextStateActionTuple] > 0)
                        //{
                            optimisticRewardHold = 0.81;
                            nextAction2.Add("HOLD", optimisticRewardHold);
                        //}
                    }
                    catch
                    {
                        optimisticRewardHold = 0.81;
                        nextAction2.Add("HOLD", optimisticRewardHold);
                    }
                }
                // TODO: What if nextAction2 is empty?  default value
                if (nextAction2.Count > 0)
                {
                    maxNextQValue = Math.Max(maxNextQValue, nextAction2.OrderByDescending(v => v.Value).First().Value);
                }
                else
                {
                    ;
                }             

                if (maxNextQValue == optimisticRewardUp)
                {
                    newAction = "UP";
                    //stateActionCount = stateActionCountUp;
                }
                else if (maxNextQValue == optimisticRewardDown)
                {
                    newAction = "DOWN";
                    //stateActionCount = stateActionCountDown;
                }
                else if (maxNextQValue == optimisticRewardHold)
                {
                    newAction = "HOLD";
                    //stateActionCount = stateActionCountHold;
                }
                else
                {
                    newAction = sortedMoveDict.First().Key;                
                }

                // Update Action taken
                // Update discreteStateTuple with new Action.
                nextPaddleY = SimulateMoveRightPaddle(this.PaddleY, newAction);
                discreteNextPaddleY = Math.Min((Int32)Math.Floor(this._boardY * nextPaddleY / (1 - this._paddleHeight)), this._boardY - 1);

                var discreteNextStateTuple = Tuple.Create(b.DiscreteBallX, b.DiscreteBallY, b.DiscreteVelocityX, b.DiscreteVelocityY, discreteNextPaddleY, newAction);
                try
                {
                    this._N.Add(discreteNextStateTuple, 1);
                }
                catch
                {
                    this._N[discreteNextStateTuple] = this._N[discreteNextStateTuple] + 1;
                    //int tmpCount = this._N[discreteNextStateTuple];
                    //this._N.Remove(discreteNextStateTuple);
                    //this._N.Add(discreteNextStateTuple, tmpCount + 1);
                }

                if (stateReward > -1)
                {
                    if (newAction.Equals("UP"))
                    {

                        //this._paddleY -= 0.04;
                        //if (this._paddleY < 0)
                        //{
                        //    this._paddleY = 0;
                        //}
                        //this._discretePaddleY = getDiscretePaddleY();
                        this._prevAction = "UP";
                    }
                    else if (newAction.Equals("DOWN"))
                    {
                        //this._paddleY += 0.04;
                        //this._discretePaddleY = getDiscretePaddleY();

                        //if (this._paddleY > 1 - GlobalValues.PaddleHeight)
                        //{
                        //    this._paddleY = 1 - GlobalValues.PaddleHeight;
                        //    this._discretePaddleY = 11;
                        //}
                        this._prevAction = "DOWN";
                    }
                    else
                    {
                        // HOLD
                        // Do Nothing
                        this._prevAction = "HOLD";
                    }
                }
            }


            this._Q[discreteStateTuple] = currQVal + decayedLearningRate * (stateReward + _discountFactor * maxNextQValue - currQVal);
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
            //this._discretePaddleX = (Int32)this._paddleX * this._boardX;            
            this._discretePaddleX = Math.Min((Int32)Math.Floor(this._boardX * (this._paddleX - 0) / (1 - 0)), this._boardX-1);
            return this._discretePaddleX;
        }

        public int getDiscretePaddleY()
        {
            this._discretePaddleY = Math.Min((Int32)Math.Floor(this._boardY * this._paddleY / (1 - this._paddleHeight)), this._boardY-1);
            //this._discretePaddleY = Math.Min((Int32)Math.Floor(this._boardY * (this._paddleY - 0) / (1 - 0)), this._boardY - 1);
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
