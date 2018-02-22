using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IScenarioTrigger
{
    /// <summary>
    /// Unique ID of the trigger.
    /// </summary>
    string TriggerId
    {
        get;
    }
}
