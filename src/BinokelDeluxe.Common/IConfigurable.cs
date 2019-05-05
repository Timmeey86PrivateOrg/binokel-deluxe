namespace BinokelDeluxe.Common
{
    /// <summary>
    /// This interface is used to identify classes which store settings.
    /// Since reflections will be used to work with these classes, the interface currently does not define any methods.
    /// Implementers must have the [Serializable()] attribute.
    /// </summary>
    public interface IConfigurable
    {
    }
}
