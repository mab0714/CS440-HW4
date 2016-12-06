using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    class GamesState
    {
        private double _ballX;
        private double _ballY;
        private double _velocityX;
        private double _velocityY;
        private double _paddleX;
        private double _paddleY;
        private int _stateReward;
        private string _stateAction;

        public GamesState(double ballX, double ballY, double velocityX, double velocityY, double paddleX, double paddleY, int stateReward, string stateAction)
        {        
            this._ballX = ballX;
            this._ballY = ballY;
            this._velocityX = velocityX;
            this._velocityY = velocityY;
            this._paddleX = paddleX;
            this._paddleY = paddleY;
            this._stateReward = stateReward;
            this._stateAction = stateAction;

        }

        public double BallX
        {
            get { return this._ballX; }
            set { this._ballX = value; }
        }

        public double BallY
        {
            get { return this._ballY; }
            set { this._ballY = value; }
        }

        public double BallVelocityX
        {
            get { return this._velocityX; }
            set { this._velocityX = value; }
        }

        public double BallVelocityY
        {
            get { return this._velocityY; }
            set { this._velocityY = value; }
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

        public string StateAction
        {
            get { return this._stateAction; }
            set { this._stateAction = value; }
        }

        public int StateReward
        {
            get { return this._stateReward; }
            set { this._stateReward = value; }
        }
    }
}
