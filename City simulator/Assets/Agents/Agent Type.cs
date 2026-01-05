using System;

[Flags]
public enum AgentType
{
    None = 0, Car = 1 << 0 , Person = 1 << 1
}
