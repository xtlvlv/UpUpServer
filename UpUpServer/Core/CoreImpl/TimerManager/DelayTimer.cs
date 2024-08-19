using System;

namespace UpUpServer
{
    public class DelayTimer : Timer
    {
        protected Action _onComplete;

        /// <summary>
        /// 定时执行
        /// </summary>
        /// <param name="isPersistence">为true时，这个定时器不受暂停和cancel影响，如果该定时器挂载在物体上，物体销毁该定时器还是会被销毁</param>
        /// <param name="duration"> 延迟时间 </param>
        /// <param name="onComplete"> 完成回调 </param>
        /// <param name="onUpdate"> 每帧回调（Unity使用） </param>
        /// <param name="autoDestroyOwner"></param>
        public DelayTimer(bool isPersistence, long duration, Action onComplete, Action<float>? onUpdate,
            object autoDestroyOwner)
            : base(isPersistence, duration, onUpdate, autoDestroyOwner)
        {
            _onComplete = onComplete;
        }
       
        public override void Update()
        {
            if (!CheckUpdate()) return;

            if (_onUpdate != null)
                SafeCall(_onUpdate, GetTimeElapsed());

            if (GetWorldTime() >= GetFireTime())
            {
                isCompleted = true;
                SafeCall(_onComplete);
            }
        }

        public void Restart(long newDuration)
        {
            duration = newDuration;
            Restart();
        }

        public void Restart(long newDuration, Action newOnComplete, Action<float> newOnUpdate)
        {
            duration = newDuration;
            _onComplete = newOnComplete;
            _onUpdate = newOnUpdate;
            Restart();
        }
    }
}