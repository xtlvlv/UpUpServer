
namespace UpUpServer
{

    public class TimerManager : SingleClass<TimerManager>
    {
        // UTC时间
        public static DateTime Now => DateTime.Now;
        // 当前ms （毫秒时间戳）
        public static long CurrentMs => (Now.Ticks - 621355968000000000) / 10000;
        
        private readonly LinkedList<Timer>
            _persistenceTimers =
                new LinkedList<Timer>(); //can not be effected by Timer.xxAllRegisteredTimers() methods

        private readonly LinkedList<Timer> _timers = new LinkedList<Timer>();

        // buffer adding timers so we don't edit a collection during iteration
        private readonly List<Timer> _timersToAdd            = new List<Timer>();
        private readonly List<Timer> _persistenceTimersToAdd = new List<Timer>();
        
        System.Timers.Timer _sysTimer;      // 默认精度500ms

        public long TimerMs;

        public void Init()
        {
            Timer.InitTimerManager();
            TimerMs = CurrentMs;
            _sysTimer = new System.Timers.Timer(500);
            _sysTimer.Elapsed += (_, _) =>
            {
                TimerMs = CurrentMs;
                this.Update();
            };
            _sysTimer.AutoReset = true;
            _sysTimer.Enabled = true;
        }

        public void CancelAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Cancel();
                timer._isInManager = false;
            }

            foreach (Timer timer in _timersToAdd)
            {
                timer.Cancel();
                timer._isInManager = false;
            }

            _timers.Clear();
            _timersToAdd.Clear();
        }

        public void CancelAllTimersByOwner(object? owner)
        {
            if (owner is null) return;
            CancelAllTimersByOwner(_timers, _timersToAdd, owner);
            CancelAllTimersByOwner(_persistenceTimers, _persistenceTimersToAdd, owner);
        }

        public void PauseAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Pause();
            }

            foreach (Timer timer in _timersToAdd)
            {
                timer.Pause();
            }
        }

        public void ResumeAllTimers()
        {
            foreach (Timer timer in _timers)
            {
                timer.Resume();
            }

            foreach (Timer timer in _timersToAdd)
            {
                timer.Resume();
            }
        }

        public void Register(Timer timer)
        {
            if (!timer.isPersistence)
                _timersToAdd.Add(timer);
            else
                _persistenceTimersToAdd.Add(timer);
        }

        // update all the registered timers on every frame
        public void Update()
        {
            UpdateTimers();
            UpdatePersistenceTimers();
        }

        //Timer
        private void UpdateTimers()
        {
            UpdateTimersInternal(_timers, _timersToAdd);
        }

        //PersistenceTimer
        private void UpdatePersistenceTimers()
        {
            UpdateTimersInternal(_persistenceTimers, _persistenceTimersToAdd);
        }

        private static void UpdateTimersInternal(LinkedList<Timer> timers, List<Timer> timersToAdd)
        {
            int toAddCount = timersToAdd.Count;
            if (toAddCount > 0)
            {
                for (int i = 0; i < toAddCount; i++)
                    timers.AddLast(timersToAdd[i]._linkedListNode);
                timersToAdd.Clear();
            }

            var node = timers.First;
            while (node != null)
            {
                var timer = node.Value;
                timer.Update();
                if (timer.isDone)
                {
                    timer._isInManager = false;
                    var toRemoveNode = node;
                    node = node.Next;
                    //remove
                    timers.Remove(toRemoveNode);
                }
                else
                    node = node.Next;
            }
        }

        private static void CancelAllTimersByOwner(LinkedList<Timer> timers, List<Timer> timersToAdd, Object owner)
        {
            var node = timers.First;
            while (node != null)
            {
                var timer = node.Value;
                if (!timer.isDone && timer.autoDestroyOwner == owner)
                {
                    timer.Cancel();
                    timer._isInManager = false;
                    var toRemoveNode = node;
                    node = node.Next;
                    //remove
                    timers.Remove(toRemoveNode);
                }
                else
                    node = node.Next;
            }

            for (int i = timersToAdd.Count - 1; i >= 0; i--)
            {
                var timer = timersToAdd[i];
                if (!timer.isDone && timer.autoDestroyOwner != owner) continue;
                timer.Cancel();
                timer._isInManager = false;
                //remove
                timersToAdd.RemoveAt(i);
            }
        }
    }
}