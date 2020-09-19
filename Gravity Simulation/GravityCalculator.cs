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
        public float speed { get; private set; }

        //Constructor
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

        //private methods****************************************************

        //**********************************************************************
        //public methods
        //Get the current cords
        public void Calculate(float time)
        {
            Dead = false;
            float xspeed = startspeed * (float)(Math.Cos(Math.PI / 180 * angle));
            float yspeed = startspeed * (float)(Math.Sin(Math.PI / 180 * angle));
            xcord = originalxcord + xspeed * time;
            height = originalheight + (yspeed * time - (gravity / 2) * time * time);
            //Calculates the current speed
            speed = (float)Math.Sqrt(Math.Pow(xspeed, 2) + Math.Pow((yspeed - gravity * time), 2));
            //If we would go out of the map
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

        //--------------------------------------------------------------------
        //Makes Dead value true that's it
        public void MakeItDie()
        {
            Dead = true;
        }
    }
}