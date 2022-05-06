namespace Retro_ML.Application.Models
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
