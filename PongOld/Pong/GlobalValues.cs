using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    class GlobalValues
    {
        static int _discreteBoardX = 0;
        static int _discreteBoardY = 0;
        static double _initialBallX = 0.0;
        static double _initialBallY = 0.0;
        static double _initialVelocityX = 0.0;
        static double _initialVelocityY = 0.0;
        static double _paddleHeight = 0.0;
        static double _randomVelocityX = 0.0;
        static double _randomVelocityY = 0.0;
        static int _learningConstant = 1;
        static double _discountFactor = 0.0;
        public static int DiscreteBoardX
        {
            get
            {
                return _discreteBoardX;
            }
            set
            {
                _discreteBoardX = value;
            }
        }

        public static int DiscreteBoardY
        {
            get
            {
                return _discreteBoardY;
            }
            set
            {
                _discreteBoardY = value;
            }
        }

        public static double InitialBallX
        {
            get
            {
                return _initialBallX;
            }
            set
            {
                _initialBallX = value;
            }
        }

        public static double InitialBallY
        {
            get
            {
                return _initialBallY;
            }
            set
            {
                _initialBallY = value;
            }
        }

        public static double InitialVelocityX
        {
            get
            {
                return _initialVelocityX;
            }
            set
            {
                _initialVelocityX = value;
            }
        }

        public static double InitialVelocityY
        {
            get
            {
                return _initialVelocityY;
            }
            set
            {
                _initialVelocityY = value;
            }
        }

        public static double RandomVelocityX
        {
            get
            {
                return _randomVelocityX;
            }
            set
            {
                _randomVelocityX = value;
            }
        }

        public static double RandomVelocityY
        {
            get
            {
                return _randomVelocityY;
            }
            set
            {
                _randomVelocityY = value;
            }
        }

        public static double PaddleHeight
        {
            get
            {
                return _paddleHeight;
            }
            set
            {
                _paddleHeight = value;
            }
        }

        public static int LearningConstant
        {
            get
            {
                return _learningConstant;
            }
            set
            {
                _learningConstant = value;
            }
        }

        public static double DiscountFactor
        {
            get
            {
                return _discountFactor;
            }
            set
            {
                _discountFactor = value;
            }
        }
    }


}
