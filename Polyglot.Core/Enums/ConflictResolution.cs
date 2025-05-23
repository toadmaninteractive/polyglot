namespace Polyglot.Core
{
    public enum ConflictResolution
    {
        UseTranslatedBackend,
        UseTranslatedFrontend,
        DeleteFromTranslatedBackend,
        Ignore,
        Manual,
        Unknown
    }
}
