using System;
using UnityEngine;

namespace Features.InputModule.Core.Scripts {
    public class InputVector2Actions : InputDefaultActions {
        public Action<Vector2> VectorChangedStarted;
        public Action<Vector2> VectorChangedPerformed;
        public Action<Vector2> VectorChangedCanceled;
    }
}