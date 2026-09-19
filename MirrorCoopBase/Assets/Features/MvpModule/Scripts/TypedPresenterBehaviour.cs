namespace Features.MvpModule {
    public abstract class PresenterBehaviour<TViewType> : PresenterBehaviour where TViewType : ViewBehaviour {
        protected TViewType View =>
            ViewBehaviour as TViewType;
    }
}