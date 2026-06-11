// TCP 통신에서 사용하는 패킷 타입 문자열을 모아둔 클래스
public static class PacketProtocol
{
    public const string READY = "READY";
    public const string START = "START";

    public const string ATTACK = "ATTACK";
    public const string RESULT = "RESULT";
    public const string GAME_OVER = "GAME_OVER";

    public const string TURN_TIMEOUT = "TURN_TIMEOUT";

    public const string REPLAY_READY = "REPLAY_READY";
    public const string REPLAY_START = "REPLAY_START";

    public const string LEAVE = "LEAVE";

    public const string CHAT = "CHAT";
}