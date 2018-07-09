namespace Automata.Utility
{
    public class StateMachine
    {
        private State state = null;

        public void SetState<T>(T newState) where T : State
        {
            // execute exit state method
            if (state != null)
            {
                state.OnExit();
            }

            // set new state
            state = newState;

            // execute enter state method
            if (state != null)
            {
                state.OnEnter();
            }
        }

        public void OnUpdate()
        {
            // execute update state method
            if (state != null)
            {
                state.OnUpdate();
            }
        }
    }
}