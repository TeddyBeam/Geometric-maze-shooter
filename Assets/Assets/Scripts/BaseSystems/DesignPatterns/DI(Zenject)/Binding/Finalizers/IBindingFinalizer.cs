namespace BaseSystems.DesignPatterns.Zenject
{
    public interface IBindingFinalizer
    {
        bool CopyIntoAllSubContainers
        {
            get;
        }

        void FinalizeBinding(DiContainer container);
    }
}
