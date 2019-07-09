using System.Collections.Generic;
using System;

namespace CG
{
    public class Dialogue
    {
        public string Name;
        public string Content;
        public float ShowTime;
        public float HideTime;
    }

    public class CommonCgConfig
    {
        public string Path;
        public int Priority;
        public bool IsVideo;
        public bool DontSkip;
        public Action StartCallback;

        //public string CGName;
        public Dictionary<string, Dialogue> Dialogues = new Dictionary<string, Dialogue>();
        public Dictionary<string, string> Names = new Dictionary<string, string>();
        public bool IsMaskShown;

        public void Reset()
        {
            Path = null;
            Priority = 0;
            Dialogues.Clear();
            Names.Clear();
            IsMaskShown = false;
            StartCallback = null;
        }
    }
}



