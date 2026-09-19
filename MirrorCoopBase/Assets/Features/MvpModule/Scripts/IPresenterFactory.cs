namespace Features.MvpModule {
    public interface IPresenterFactory {
        public PresenterBehaviour GetPresenter(ViewBehaviour view);
    }
}