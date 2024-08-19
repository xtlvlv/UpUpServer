
namespace UpUpServer
{
    public abstract class Timer
    {
        #region 定时器属性
        /// <summary>
        /// How long the timer takes to complete from start to finish. (ms)
        /// </summary>
        public long duration { get; protected set; }

        /// <summary>
        /// whether the timer is persistence.
        /// when set to true, this timer is unaffected by pause and cancel operations. If this timer is attached to an object, the timer will also be destroyed when the object is destroyed.
        /// </summary>
        public bool isPersistence { get; }

        public bool isCompleted { get; protected set; }

        public bool isPaused
        {
            get { return this._timeElapsedBeforePause.HasValue; }
        }

        public bool isCancelled
        {
            get { return this._timeElapsedBeforeCancel.HasValue; }
        }

        public bool isDone
        {
            get { return this.isCompleted || this.isCancelled || this.isOwnerDestroyed; }
        }

        public object? autoDestroyOwner { get; }
        public bool hasAutoDestroyOwner { get; }
        private bool isOwnerDestroyed
        {
            get
            {
                if (!hasAutoDestroyOwner || autoDestroyOwner==null) return false;
                if (!_timeElapsedBeforeAutoDestroy.HasValue)
                    _timeElapsedBeforeAutoDestroy = GetTimeElapsed();
                return true;
            }
        }
        
        #endregion

        #region 静态创建方法

        public static DelayTimer DelayAction(long duration, Action onComplete,
            Object autoDestroyOwner = null)
        {
            return DelayActionInternal(false, duration, onComplete, null,  autoDestroyOwner);
        }
        public static DelayTimer DelayAction(long duration, Action onComplete, Action<float> onUpdate,
             Object autoDestroyOwner = null)
        {
            return DelayActionInternal(false, duration, onComplete, onUpdate,  autoDestroyOwner);
        }

        public static LoopTimer LoopAction(long interval, Action<float> onUpdate,
            bool executeOnStart = false, Object autoDestroyOwner = null)
        {
            return LoopActionInternal(false, interval, null, onUpdate,  executeOnStart,
                autoDestroyOwner);
        }
        public static LoopTimer LoopAction(long interval, Action<int> onComplete, Action<float> onUpdate,
             bool executeOnStart = false, Object autoDestroyOwner = null)
        {
            return LoopActionInternal(false, interval, onComplete, onUpdate,  executeOnStart,
                autoDestroyOwner);
        }
        
        public static DelayTimer PersistenceDelayAction(long duration, Action onComplete,
            Action<float> onUpdate,  Object autoDestroyOwner = null)
        {
            return DelayActionInternal(true, duration, onComplete, onUpdate,  autoDestroyOwner);
        }

        public static LoopTimer PersistenceLoopAction(long interval, Action<int> onComplete, Action<float> onUpdate,
             bool executeOnStart = false, Object autoDestroyOwner = null)
        {
            return LoopActionInternal(true, interval, onComplete, onUpdate,  executeOnStart,
                autoDestroyOwner);
        }

        

        /// <summary>
        /// Restart a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to restart.</param>
        public static void Restart(Timer timer)
        {
            if (timer != null)
            {
                timer.Restart();
            }
        }

        /// <summary>
        /// Cancels a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to cancel.</param>
        public static void Cancel(Timer timer)
        {
            if (timer != null)
            {
                timer.Cancel();
            }
        }

        /// <summary>
        /// Pause a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to pause.</param>
        public static void Pause(Timer timer)
        {
            if (timer != null)
            {
                timer.Pause();
            }
        }

        /// <summary>
        /// Resume a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to resume.</param>
        public static void Resume(Timer timer)
        {
            if (timer != null)
            {
                timer.Resume();
            }
        }

        public static void CancelAllRegisteredTimersByOwner(Object owner)
        {
            if (_manager != null)
            {
                _manager.CancelAllTimersByOwner(owner);
            }

            // if the manager doesn't exist, we don't have any registered timers yet, so don't
            // need to do anything in this case
        }
        
        public static void CancelAllRegisteredTimers()
        {
            if (_manager != null)
            {
                _manager.CancelAllTimers();
            }

            // if the manager doesn't exist, we don't have any registered timers yet, so don't
            // need to do anything in this case
        }

        public static void PauseAllRegisteredTimers()
        {
            if (_manager != null)
            {
                _manager.PauseAllTimers();
            }

            // if the manager doesn't exist, we don't have any registered timers yet, so don't
            // need to do anything in this case
        }

        public static void ResumeAllRegisteredTimers()
        {
            if (_manager != null)
            {
                _manager.ResumeAllTimers();
            }

            // if the manager doesn't exist, we don't have any registered timers yet, so don't
            // need to do anything in this case
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Restart a timer that is in-progress or done. The timer's on completion callback will not be called.
        /// </summary>
        public void Restart()
        {
            //auto destroy. return
            if (isOwnerDestroyed) return;

            isCompleted = false;
            _startTime = GetWorldTime();
            _lastUpdateTime = _startTime;
            _timeElapsedBeforeCancel = null;
            _timeElapsedBeforePause = null;
            _timeElapsedBeforeAutoDestroy = null;
            OnRestart();
            Register();
        }


        /// <summary>
        /// Stop a timer that is in-progress or paused. The timer's on completion callback will not be called.
        /// </summary>
        public void Cancel()
        {
            if (this.isDone)
            {
                return;
            }

            this._timeElapsedBeforeCancel = this.GetTimeElapsed();
            this._timeElapsedBeforePause = null;
        }

        /// <summary>
        /// Pause a running timer. A paused timer can be resumed from the same point it was paused.
        /// </summary>
        public void Pause()
        {
            if (this.isPaused || this.isDone)
            {
                return;
            }

            this._timeElapsedBeforePause = this.GetTimeElapsed();
        }

        /// <summary>
        /// Continue a paused timer. Does nothing if the timer has not been paused.
        /// </summary>
        public void Resume()
        {
            if (!this.isPaused || this.isDone)
            {
                return;
            }

            this._timeElapsedBeforePause = null;
        }

        /// <summary>
        /// Get how many seconds/frame have elapsed since the start of this timer's current cycle.
        /// </summary>
        /// <returns>The number of seconds that have elapsed since the start of this timer's current cycle, i.e.
        /// the current loop if the timer is looped, or the start if it isn't.
        ///
        /// If the timer has finished running, this is equal to the duration.
        ///
        /// If the timer was cancelled/paused, this is equal to the number of seconds that passed between the timer
        /// starting and when it was cancelled/paused.</returns>
        public long GetTimeElapsed()
        {
            if (this.isCompleted)
            {
                return this.duration;
            }

            return this._timeElapsedBeforeCancel ??
                   this._timeElapsedBeforePause ??
                   this._timeElapsedBeforeAutoDestroy ??
                   this.GetWorldTime() - this._startTime;
        }

        /// <summary>
        /// Get how many seconds/frame remain before the timer completes.
        /// </summary>
        /// <returns>The number of seconds that remain to be elapsed until the timer is completed. A timer
        /// is only elapsing time if it is not paused, cancelled, or completed. This will be equal to zero
        /// if the timer completed.</returns>
        public long GetTimeRemaining()
        {
            return this.duration - this.GetTimeElapsed();
        }

        /// <summary>
        /// Get how much progress the timer has made from start to finish as a ratio.
        /// </summary>
        /// <returns>A value from 0 to 1 indicating how much of the timer's duration has been elapsed.</returns>
        public long GetRatioComplete()
        {
            return this.GetTimeElapsed() / this.duration;
        }

        /// <summary>
        /// Get how much progress the timer has left to make as a ratio.
        /// </summary>
        /// <returns>A value from 0 to 1 indicating how much of the timer's duration remains to be elapsed.</returns>
        public long GetRatioRemaining()
        {
            return this.GetTimeRemaining() / this.duration;
        }

        #endregion

        #region Private Static Methods

        public static void InitTimerManager()
        {
            if (_manager != null) return;
            // create a manager object to update all the timers if one does not already exist.
            _manager = TimerManager.Instance;
        }
        
        private static DelayTimer DelayActionInternal(bool isPersistence, long duration, Action onComplete,
            Action<float>? onUpdate,  Object autoDestroyOwner)
        {
            //Check
            if (duration <= 0)
            {
                SafeCall(onUpdate, 0);
                SafeCall(onComplete);
                return null;
            }

            var timer = new DelayTimer(isPersistence, duration, onComplete, onUpdate,  autoDestroyOwner);
            timer.Init();
            return timer;
        }
        
        private static LoopTimer LoopActionInternal(bool isPersistence, long interval, Action<int> onComplete, Action<float> onUpdate,
             bool executeOnStart, Object autoDestroyOwner)
        {
            var timer = new LoopTimer(isPersistence, interval, onComplete, onUpdate,  executeOnStart, autoDestroyOwner);
            timer.Init();
            return timer;
        }

        #endregion

        #region Private Static Properties/Fields

        // responsible for updating all registered timers
        private static TimerManager _manager;

        #endregion

        #region Private/Protected Properties/Fields
        

        // whether the timer is in TimeManager
        public bool _isInManager;

        protected Action<float>? _onUpdate;
        protected long _startTime;
        protected long _lastUpdateTime;

        // for pausing, we push the start time forward by the amount of time that has passed.
        // this will mess with the amount of time that elapsed when we're cancelled or paused if we just
        // check the start time versus the current world time, so we need to cache the time that was elapsed
        // before we paused/cancelled/autoDestroy
        private long? _timeElapsedBeforeCancel;
        private long? _timeElapsedBeforePause;
        private long? _timeElapsedBeforeAutoDestroy;

        public readonly LinkedListNode<Timer> _linkedListNode;

        #endregion

        #region Constructor (use static method to create new timer)

        static Timer()
        {
            InitTimerManager();
        }

        protected Timer(bool isPersistence, long duration, Action<float>? onUpdate,
             object autoDestroyOwner)
        {
            this.isPersistence = isPersistence;
            this.duration = duration;
            this._onUpdate = onUpdate;

            this.autoDestroyOwner = autoDestroyOwner;
            this.hasAutoDestroyOwner = autoDestroyOwner != null;

            _linkedListNode = new LinkedListNode<Timer>(this);
        }

        #endregion

        #region Private/Protected Methods

        private void Init()
        {
            _startTime = GetWorldTime();
            _lastUpdateTime = _startTime;
            Register();
            OnInit();
        }

        private void Register()
        {
            if (_isInManager) return;
            _isInManager = true;
            _manager.Register(this);
        }

        protected long GetFireTime()
        {
            return _startTime + duration;
        }

        protected virtual long GetWorldTime()
        {
            // 获得当前时间
            return TimerManager.Instance.TimerMs;
            // return TimerManager.CurrentMs;

        }

        public virtual void OnInit()
        {
        }

        public abstract void Update();

        public virtual void OnRestart()
        {
        }

        protected bool CheckUpdate()
        {
            if (isDone) return false;

            if (isPaused)
            {
                var curTime = GetWorldTime();
                _startTime += curTime - _lastUpdateTime;
                _lastUpdateTime = curTime;
                return false;
            }

            _lastUpdateTime = GetWorldTime();
            return true;
        }

        protected static void SafeCall(Action? action)
        {
            if (action is null) return;
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected static void SafeCall<T>(Action<T>? action, T arg)
        {
            if (action is null) return;
            try
            {
                action(arg);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        protected static TResult SafeCall<TResult>(Func<TResult>? func)
        {
            if (func is null) return default;
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return default;
            }
        }
        
        protected static TResult SafeCall<T, TResult>(Func<T, TResult>? func, T arg)
        {
            if (func is null) return default;
            try
            {
                return func(arg);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        #endregion

       

    }
}
