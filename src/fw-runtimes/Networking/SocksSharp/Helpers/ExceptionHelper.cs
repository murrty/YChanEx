namespace SocksSharp.Helpers;
internal static class ExceptionHelper {
    public static bool ValidateTcpPort(int port) {
        return port >= 1 && port <= 65535;
    }
}
