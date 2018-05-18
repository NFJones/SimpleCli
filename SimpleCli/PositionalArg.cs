
namespace SimpleCli
{
    public class PositionalArg : Arg
    {
        public PositionalArg(string name, string helpText, ValidatorDelegate validator)
        : base(name, helpText, validator)
        {
        }
    }
}
