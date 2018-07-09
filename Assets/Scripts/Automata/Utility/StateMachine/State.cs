﻿namespace Automata.Utility
{
    public abstract class State
    {
        public abstract void OnEnter();

        public abstract void OnUpdate();

        public abstract void OnExit();
    }
}