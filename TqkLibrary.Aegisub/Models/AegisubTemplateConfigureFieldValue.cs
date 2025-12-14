namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTemplateConfigureFieldValue
    {
        public required object Value { get; set; }
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
        public AegisubTemplateConfigureFieldValue<T> As<T>()
        {
            return new AegisubTemplateConfigureFieldValue<T>()
            {
                Value = (T)this.Value,
                MinValue = this.MinValue is not null? ((T)this.MinValue) : default(T),
                MaxValue = this.MaxValue is not null ? ((T)this.MaxValue) : default(T),
            };
        }
    }
    public class AegisubTemplateConfigureFieldValue<T>
    {
        public required T Value { get; set; }
        public T? MinValue { get; set; }
        public T? MaxValue { get; set; }
    }
}
