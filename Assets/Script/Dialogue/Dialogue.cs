using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : ScriptableObject
{
    public string speaker;
    [TextArea(3, 10)]
    public string[] sentences;
}
