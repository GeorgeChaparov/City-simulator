using UnityEngine;

public class Agent
{
    internal enum State
    {
       Idle, Moving, WaitingOnCrosswalk, WaitingOnTrafficLight
    }
    
    private int m_Destination;
    private State m_State = State.Idle;
}
