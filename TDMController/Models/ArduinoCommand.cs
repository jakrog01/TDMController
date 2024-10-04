
namespace TDMController.Models
{
    public class ArduinoCommand (string Type, int Value)
    {
        public string Type { get; init; } = Type;
        public int Value { get; init; } = Value;
    }
}
