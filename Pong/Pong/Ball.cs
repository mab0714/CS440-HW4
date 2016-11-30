using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Pong
{
    class Ball
    {
        private double _ballX;
        private double _ballY;

        private int _discreteBallX;
        private int _discreteBallY;

        private double _velocityX;
        private double _velocityY;

        private int _discreteVelocityX;
        private int _discreteVelocityY;

        private int _discreteBoardX;
        private int _discreteBoardY;

        private bool _twoPlayers;

        public Ball(double ballX, double ballY, double velocityX, double velocityY, int discreteBoardX, int discreteBoardY, bool twoPlayers)
        {
            this._discreteBoardX = discreteBoardX;
            this._discreteBoardY = discreteBoardY;
            this._ballX = ballX;
            this._discreteBallX = getDiscreteBallX(this._discreteBoardX);
            this._ballY = ballY;
            this._discreteBallY = getDiscreteBallY(this._discreteBoardY);
            this._velocityX = velocityX;
            this._discreteVelocityX = getDiscreteVelocityX();
            this._velocityY = velocityY;
            this._discreteVelocityY = getDiscreteVelocityY();
            this._twoPlayers = twoPlayers;
        }

        public bool TwoPlayers
        {
            get { return this._twoPlayers; }
            set { this._twoPlayers = value; }
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

        public int DiscreteBallX
        {
            get { return this._discreteBallX; }
            set { this._discreteBallX = value; }
        }

        public int DiscreteBallY
        {
            get { return this._discreteBallY; }
            set { this._discreteBallY = value; }
        }


        public double VelocityX
        {
            get { return this._velocityX; }
            set { this._velocityX = value; }
        }

        public double VelocityY
        {
            get { return this._velocityY; }
            set { this._velocityY = value; }
        }

        public int DiscreteVelocityX
        {
            get { return getDiscreteVelocityX(); }
            set { this._discreteVelocityX = value; }
        }

        public int DiscreteVelocityY
        {
            get { return this.getDiscreteVelocityY(); }
            set { this._discreteVelocityY = value; }
        }
        
        public void MoveBall()
        {
            double x = this._ballX;
            double y = this._ballY;

            double velX = this._velocityX;
            double velY = this._velocityY;

            this._ballX = x + velX;           
            this._ballY = y + velY;
            
            // Collision detection
            if (!_twoPlayers)
            {                
                if (this._ballX < 0)  // ball is off the left edge of the screen
                {
                    this._ballX = this._ballX * -1;
                    this._velocityX = this._velocityX * -1;
                }
            }

            //if (this._ballX >= 1)
            //{
            //    this._ballX = 0.99;
            //    this._discreteBallX = 11;
            //}

            if (this._ballY < 0)  // ball off the top of the screen
            {
                this._ballY = this._ballY * -1;
                this._velocityY = this._velocityY * -1;
            }

            else if (this._ballY > 1) // ball is off the bottom of the screen
            {
                this._ballY = 2 - this._ballY;
                this._velocityY = this._velocityY * -1;
            }

            this._discreteBallX = getDiscreteBallX(this._discreteBoardX);
            this._discreteBallY = getDiscreteBallY(this._discreteBoardY);


        }        

        private int getDiscreteBallX(int boardX)
        {
            //this._discreteBallX = (Int32) Math.Round(Math.Floor(this.BallX * boardX), 0);
            this._discreteBallX = (Int32)Math.Floor(boardX * (this.BallX - 0) / (1-0));
            return this._discreteBallX;
        }
        private int getDiscreteBallY(int boardY)
        {
            //this._discreteBallY = (Int32)Math.Round(Math.Floor(this.BallY * boardY), 0);
            this._discreteBallY = (Int32)Math.Floor(boardY * (this.BallY - 0) / (1 - 0));
            return this._discreteBallY;
        }

        private int getDiscreteVelocityX()
        {
            if (this._velocityX >= 0.03)
            {
                this._discreteVelocityX = 1;
            }
            if (this._velocityX <= -0.03)
            {
                this._discreteVelocityX = -1;
            }
            return this._discreteVelocityX;
        }

        private int getDiscreteVelocityY()
        {
            if (this._discreteVelocityY < -0.015)
            {
                this._discreteVelocityY = -1;
                //if (this._discreteVelocityY < 0.015)
                //{
                //    this._discreteVelocityY = 0;
                //}
            }
            else if (this._discreteVelocityY > 0.015)
            {
                this._discreteVelocityY = 1;
                //if (this._discreteVelocityY < 0.015)
                //{
                //    this._discreteVelocityY = 0;
                //}
            }
            else
            {
                this._discreteVelocityY = 0;
            }
            return this._discreteVelocityY;
        }
    }
}
