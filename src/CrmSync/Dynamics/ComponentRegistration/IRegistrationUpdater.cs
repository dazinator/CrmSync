namespace CrmSync.Dynamics.ComponentRegistration
{
    public interface IRegistrationDeployer
    {
        ComponentRegistration Registration { get; }
        RegistrationInfo Deploy();
        void Undeploy(RegistrationInfo registrationInfo);
    }
}