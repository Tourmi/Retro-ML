namespace Retro_ML.Models
{
    internal class Error
    {
        public Error()
        {
            FieldError = "";
            Description = "";
        }

        public string FieldError { get; set; }

        public string Description { get; set; }
    }
}
