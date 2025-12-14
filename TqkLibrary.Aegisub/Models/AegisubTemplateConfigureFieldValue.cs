namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTemplateConfigureFieldValue
    {
        public required object DefaultValue { get; set; }
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
        public AegisubTemplateConfigureFieldValue<T> As<T>()
        {
            return new AegisubTemplateConfigureFieldValue<T>()
            {
                DefaultValue = (T)this.DefaultValue,
                MinValue = this.MinValue is not null? ((T)this.MinValue) : default(T),
                MaxValue = this.MaxValue is not null ? ((T)this.MaxValue) : default(T),
            };
        }
    }
    public class AegisubTemplateConfigureFieldValue<T>
    {
        public required T DefaultValue { get; set; }
        public T? MinValue { get; set; }
        public T? MaxValue { get; set; }
    }
}
