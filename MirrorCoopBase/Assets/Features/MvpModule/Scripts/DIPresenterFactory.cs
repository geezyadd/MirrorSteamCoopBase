using System;
using Zenject;

namespace Features.MvpModule {
    public sealed class DIPresenterFactory : IPresenterFactory {
        private const string VIEWBASE_ENDING = "ViewBase";
        private string PRESENTER_ENDING = "Presenter";
        private readonly DiContainer _container;

        public DIPresenterFactory(DiContainer container) =>
            _container = container ?? throw new ArgumentNullException(nameof(container));

        public PresenterBehaviour GetPresenter(ViewBehaviour view) {
            Type baseType = view.GetType().BaseType;
            if (baseType == null)
                throw new ViewIsNotSuitableException(view.GetType());

            try {
                return _container.Instantiate(GetPresenterType(baseType)) as PresenterBehaviour;
            }
            catch (Exception e) {
                throw new PresenterCannotBeCreatedException(view.GetType(), e);
            }
        }

        private Type GetPresenterType(Type baseType) {
            string presenterTypeName = baseType.FullName?.Replace(VIEWBASE_ENDING, PRESENTER_ENDING);
            Type presenterType = baseType.Assembly.GetType(presenterTypeName);
            if (presenterType == null)
                throw new PresenterCannotBeCreatedException(baseType);

            return presenterType;
        }
    }
}