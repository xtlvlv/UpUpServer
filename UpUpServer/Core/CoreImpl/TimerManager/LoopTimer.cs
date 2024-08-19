using System;

namespace UpUpServer
{
    public class LoopTimer : Timer
    {
        /// <summary>
        /// Parameter is loopTime.
        /// </summary>
        protected Action<int> _onComplete;
        public bool executeOnStart { protected set; get; }

        /// <summary>
        /// How many times does the LoopTimer looped.
        /// </summary>
        public int loopTimes { private set; get; }
        int _curLoopTimes { set; get; }
        private long _lastActionTime;   // 上次执行时间


        protected virtual void OnComplete()
        {
        }

        public LoopTimer(bool isPersistence, long interval, Action<int> onComplete,
            Action<float> onUpdate,  bool executeOnStart, object autoDestroyOwner)
            : base(isPersistence, interval, onUpdate,  autoDestroyOwner)
        {
            _onComplete = onComplete;
            this.executeOnStart = executeOnStart;
            _curLoopTimes = 0;
        }

        public override void OnInit()
        {
            if (executeOnStart)
                Update();
            _lastActionTime = GetWorldTime();
        }

        public override void OnRestart()
        {
            loopTimes = 0;
            if (executeOnStart)
                Update();
        }

        public override void Update()
        {
            if (!CheckUpdate()) return;

            bool needUpdate = GetWorldTime() - _lastActionTime >= duration;
            if (_onUpdate != null && needUpdate)
            {
                _curLoopTimes++;
                _lastActionTime = GetWorldTime();
                SafeCall(_onUpdate, GetTimeElapsed());
            }
            if (loopTimes>0 && _curLoopTimes >= loopTimes)
            {
                isCompleted = true;
            }
        }


        public void Restart(long newInterval)
        {
            duration = newInterval;
            Restart();
        }

        public void Restart(long newInterval, Action<int> newOnComplete, Action<float> newOnUpdate, bool newExecuteOnStart)
        {
            duration = newInterval;
            _onComplete = newOnComplete;
            _onUpdate = newOnUpdate;
            executeOnStart = newExecuteOnStart;
            Restart();
        }
    }
}