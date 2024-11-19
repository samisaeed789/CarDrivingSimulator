namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        public abstract class TSAIBaseModule
        {
            protected TSTrafficAI _trafficAI;

            public virtual void Initialize(TSTrafficAI trafficAI)
            {
                _trafficAI = trafficAI;
            }
            public virtual void PostInitialize(){}
            public abstract void OnFixedUpdate();
            public virtual void OnFixedUpdateMainThread(){}
            public virtual void OnEnable(){}
            
            public virtual void OnDrawGizmosSelected(){}
        }
    }
}