// Compatibility entry points for the earlier two-wave verification commands.
// All entry points now verify the current three-wave demo.
public static class HeroArenaVerification
{
    public static void RunRunnerChecks() => CompleteDemoVerification.RunRunnerRegression();
    public static void RunWaveChecks() => CompleteDemoVerification.RunWaves();
    public static void RunVisualChecks() => CompleteDemoVerification.RunVisualUI();
}
