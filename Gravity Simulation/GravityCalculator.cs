using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity_Simulation
{
    internal class GravityCalculator
    {
        private const float gravity = 9.81f;
        private readonly float originalheight;
        private readonly float originalxcord;

        //Properties
        public float startspeed { get; private set; }

        public float height { get; private set; }
        public float xcord { get; private set; }
        public float angle { get; private set; }
        public bool Dead { get; private set; }

        //Constructors
        public GravityCalculator(float height, float xcord)
        {
            Dead = false;
            this.height = height;
            this.xcord = xcord;
            this.angle = 90;
            this.startspeed = 0;
            this.originalheight = height;
            this.originalxcord = xcord;
        }

        public GravityCalculator(float height, float xcord, float startspeed)
        {
            Dead = false;
            this.height = height;
            this.xcord = xcord;
            this.angle = 90;
            this.startspeed = startspeed;
            this.originalheight = height;
            this.originalxcord = xcord;
        }

        public GravityCalculator(float height, float xcord, float startspeed, float angle)
        {
            Dead = false;
            this.height = height;
            this.xcord = xcord;
            this.angle = angle;
            this.startspeed = startspeed;
            this.originalheight = height;
            this.originalxcord = xcord;
        }

        //private methods

        private float CalcPathWithoutAngle()
        {
            float maxheight = startspeed * startspeed / (2 * gravity);

            return maxheight * 2;
        }

        private float Calcpath()
        {
            float output = 0;
            float xspeed = startspeed * (float)(Math.Cosh(Math.PI / 180 * angle));
            float yspeed = startspeed * (float)(Math.Sinh(Math.PI / 180 * angle));
            return output;
        }

        //public methods
        public float Calculatepath()
        {
            if (startspeed == 0 || angle == 270)
            {
                return height;
            }
            else if (angle == 90)
            {
                return CalcPathWithoutAngle();
            }
            else
            {
                return Calculatepath();
            }
        }

        //Get the current cords
        public void CalcCords(float time)
        {
            Dead = false;
            float xspeed = startspeed * (float)(Math.Cos(Math.PI / 180 * angle));
            float yspeed = startspeed * (float)(Math.Sin(Math.PI / 180 * angle));
            xcord = originalxcord + xspeed * time;
            height = originalheight + (yspeed * time - (gravity / 2) * time * time);
            if (xcord < 0)
            {
                xcord = 0;
                Dead = true;
            }
            if (height < 0)
            {
                height = 0;
                Dead = true;
            }
        }

        //Makes Dead value true that's it
        public void MakeItDie()
        {
            Dead = true;
        }
    }
}