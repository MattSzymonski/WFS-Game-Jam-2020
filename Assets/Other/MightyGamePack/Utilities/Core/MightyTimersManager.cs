/* 
NAME:
    Mighty Timers Manager

DESCRIPTION:
    Timers manager for faster and cleaner timer creation
    Stores all timer used by other scripts

USAGE:
    Look under the code below 

TODO:
   
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace MightyGamePack
{
    [System.Serializable]
    public class MightyTimer
    {
        [ReadOnly] public string name;
        [ReadOnly] public float speed;
        [ReadOnly] public float currentTime;
        [ReadOnly] public float targetTime;
        [ReadOnly] public bool loop;

        [ReadOnly] public bool stopped;
        [ReadOnly] public bool finished;

        public MightyTimer(string name, float targetTime, float speed, bool loop, bool stopped)
        {
            this.name = name;
            this.targetTime = targetTime;
            this.speed = speed;
            this.loop = loop;
            this.stopped = stopped;
        }

        public void UpdateTimer() //Called by MightyTimersManager every frame
        {
            if (stopped == false)
            {
                if (currentTime < targetTime)
                {
                    currentTime += speed * Time.deltaTime;
                }
                else
                {
                    finished = true;
                    if (loop)
                    {
                        RestartTimer();
                    }
                }
            }
        }

        public void RestartTimer()
        {
            currentTime = 0;
            finished = false;
        }

        public void StopTimer()
        {
            stopped = true;
        }

        public void PlayTimer()
        {
            stopped = false;
        }
    }


    public class MightyTimersManager : MonoBehaviour
    {
        public List<MightyTimer> timers;

        private void Update()
        {
            for (int i = timers.Count - 1; i >= 0; i--)
            {
                timers[i].UpdateTimer(); 
            }
        }

        public MightyTimer CreateTimer(string name, float targetTime, float speed = 1, bool loop = false, bool stopped = false)
        {
            MightyTimer newTimer = new MightyTimer(name, targetTime, speed, loop, stopped);
            timers.Add(newTimer);
            return newTimer;
        }

        public void RemoveTimer(MightyTimer timer)
        {
            timers.Remove(timer);
        }
    }
}


/*
USAGE:

    Add this component to GameObject in Hierarchy
    public MightyGamePack.MightyTimer myTimer; // Declare timer in any script
    myTimer = MightyGamePack.MightyGameManager.timersManager.CreateTimer("MyTimer", 1f, 1f, false, true); // Create new timer (Not looping, stopped on start)
    myTimer.RestartTimer();
    myTimer.PlayTimer();
    if (myTimer.finished) { Do something }




    Example of usage - Sniper breath stop:

        public MightyGamePack.MightyTimer sniperBreathTimer;;

        void Start()
        {
            sniperBreathTimer = MightyGamePack.MightyGameManager.timersManager.CreateTimer("MyTimer", 1f, 1f, false, true); //Not looping, stopped on start
        }

        void HoldBreath() //Triggered on button 
        {
            sniperBreathTimer.RestartTimer();
            sniperBreathTimer.PlayTimer();
        }

        void Update() 
        {
            if(!sniperBreathTimer.finished) 
            {
                //Do holding breath related stuff
            }
        }
*/
