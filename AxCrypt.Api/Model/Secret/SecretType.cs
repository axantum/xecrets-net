namespace AxCrypt.Api.Model.Secret
{
    /// <summary>
    /// The types of a secret.
    /// </summary>
    public enum SecretType
    {
        Legacy = 1,

        Password,

        Card,

        Note
    }
}