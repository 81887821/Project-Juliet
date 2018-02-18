using System;
using UnityEngine;

public class ScenarioScript : ScriptableObject
{
    public enum Character
    {
        /// <summary>
        /// Use following sentence as scenario directive, not a dialogue.
        /// </summary>
        ScenarioDirective,
        /// <summary>
        /// Do not display character image.
        /// </summary>
        None,
        /// <summary>
        /// Show small form player character.
        /// </summary>
        Julia,
        /// <summary>
        /// Show big form player character.
        /// </summary>
        Juliett,
        /// <summary>
        /// Show player character's current form in game.
        /// If no player character is instantiated in current game scene, small form image will be used.
        /// </summary>
        JuliettCurrentForm,
        Romeo
    }

    [Serializable]
    public struct Dialogue
    {
        public Character Speaker;
        [TextArea(1, 10)]
        public string Sentence;
    }

    public Dialogue[] Dialogues;
}
