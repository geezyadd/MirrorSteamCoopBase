using System;
using UnityEngine;

namespace Features.InputModule.Core.Scripts {
    public class InputVector3Actions : InputDefaultActions {
        public Action<Vector3> VectorChangedStarted;
        public Action<Vector3> VectorChangedPerformed;
        public Action<Vector3> VectorChangedCanceled;
    }
}