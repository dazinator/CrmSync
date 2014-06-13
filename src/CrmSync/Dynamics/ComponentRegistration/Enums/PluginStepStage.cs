namespace CrmSync.Dynamics.ComponentRegistration.Enums
{
    public enum PluginStepStage
    {
        Invalid = 0,
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
        PostOperationDeprecated = 50
    }
}