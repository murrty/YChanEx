#nullable enable
namespace YChanEx;
internal static class Arguments {
    public static string[] Argv { get; set; } = [];
    public static bool SetProtocol(string[] argv) {
        if (argv?.Length > 0) {
            if (argv[0].Equals("-p", StringComparison.OrdinalIgnoreCase) || argv[0].Equals("--protocol", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
}
