namespace YChanEx {
    internal enum ArgumentAction {
        None,
        AddThread,
    }

    internal static class Arguments {
        public static bool SetProtocol {
            get; private set;
        }

        public static List<string> URLs {
            get;
        } = [];

        public static ArgumentAction ParseArguments(string[] args) {
            if (args.Length > 0) {
                string arg;
                for (int i = 0; i < args.Length; i++) {
                    arg = args[i].ToLower();
                    switch (arg) {
                        case "--protocol": {
                            SetProtocol = true;
                        } break;

                        default: {
                            URLs.Add(args[i]);
                        } break;
                    }
                }
            }
            return ArgumentAction.None;
        }
    }
}
