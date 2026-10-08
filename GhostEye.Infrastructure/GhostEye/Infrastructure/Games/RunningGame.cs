namespace GhostEye.Infrastructure.Games;

public sealed record RunningGame(GameEntry Game, int ProcessId, string ExecutablePath);
