using System;
using System.Timers;

namespace Automata.Utility
{
    public class Timer
    {
        private System.Timers.Timer timer = null;
        private Action action = null;
        private uint interval;

        public void SetInterval(uint newInterval)
        {
            // set the interval to a new interval
            interval = newInterval;
        }

        public void SetAction(Action newAction)
        {
            // set the action to a new action
            if (newAction != null)
            {
                action = newAction;
            }
        }

        public void Start()
        {
            // create a new timer
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += new ElapsedEventHandler(ElapsedHandler);

            // start the timer
            timer.Enabled = true;
        }

        public void Stop()
        {
            // stop the timer if it exists
            if (timer != null)
            {
                timer.Enabled = false;
            }
        }

        private void ElapsedHandler(object sender, ElapsedEventArgs e)
        {
            // execute the assigned method
            action();
        }
    }
}