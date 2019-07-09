// Cinema Suite
using UnityEngine;
using System.Collections.Generic;

namespace CinemaDirector.Helpers
{
    /// <summary>
    /// Implement this interface with any timeline item that manipulates data in a scene.
    /// </summary>
    interface IRevertable
    {
        RevertMode EditorRevertMode { get; set; }
        RevertMode RuntimeRevertMode { get; set; }

        List<RevertInfo> CacheState();
    }
}
