//----------------------------------------------
//            Xffect Editor
// Copyright © 2012- Shallway Studio
// http://shallway.net
//----------------------------------------------

using UnityEngine;

namespace Xft
{
    public class UVAnimation
    {
        public Vector2[] frames;
        public Vector2[] UVDimensions;

        // Animation state vars:
        public int curFrame = 0;
        protected int stepDir = 1;
        //fixed ver 2.1.0, should be reset when finished.
        public int numLoops = 0;

        public string name;
        public int loopCycles = 0;                      // How many times to loop the animation (-1 loop infinitely)
        public bool loopReverse = false;

        // Stores the UV of the next frame in 'uv', returns false if
        // we've reached the end of the animation (this will never
        // happen if it is set to loop infinitely)
        public bool GetNextFrame(ref Vector2 uv, ref Vector2 dm)
        {
            // See if we can advance to the next frame:
            if ((curFrame + stepDir) >= frames.Length || (curFrame + stepDir) < 0)
            {
                // See if we need to loop (if we're reversing, we don't loop until we get back to the beginning):
                if (stepDir > 0 && loopReverse)
                {
                    stepDir = -1;   // Reverse playback direction
                    curFrame += stepDir;

                    uv = frames[curFrame];
                    dm = UVDimensions[curFrame];
                }
                else
                {
                    // See if we can loop:
                    if (numLoops + 1 > loopCycles && loopCycles != -1)
                        return false;
                    else
                    {   // Loop the animation:
                        ++numLoops;
                        if (loopReverse)
                        {
                            stepDir *= -1;
                            curFrame += stepDir;
                        }
                        else
                            curFrame = 0;

                        uv = frames[curFrame];
                        dm = UVDimensions[curFrame];
                    }
                }
            }
            else
            {
                curFrame += stepDir;
                uv = frames[curFrame];
                dm = UVDimensions[curFrame];
            }
            return true;
        }

        //top-left to bottom right.
        public Vector2[] BuildUVAnim(Vector2 start, Vector2 cellSize, int cols, int rows, int totalCells)
        {
            int cellCount = 0;
            frames = new Vector2[totalCells];
            UVDimensions = new Vector2[totalCells];
            frames[0] = start;
            for (int row = 0; row < rows; ++row)
            {
                for (int col = 0; col < cols && cellCount < totalCells; ++col)
                {
                    frames[cellCount].x = start.x + cellSize.x * ((float)col);
                    frames[cellCount].y = start.y + cellSize.y * ((float)row);
                    UVDimensions[cellCount] = cellSize;
                    //UVDimensions[cellCount].y = -UVDimensions[cellCount].y;
                    ++cellCount;
                }
            }
            return frames;
        }
    }
}