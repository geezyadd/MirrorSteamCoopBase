using System;

namespace Features.InputModule.Core.Scripts {
    public class InputDefaultActions {
        public Action Started;
        public Action Performed;
        public Action Canceled;

        private bool _isEnabled = true;

        public void Enable() =>
            _isEnabled = true;

        public void Disable() =>
            _isEnabled = false;

        public bool IsEnabled() =>
            _isEnabled;
    }
}