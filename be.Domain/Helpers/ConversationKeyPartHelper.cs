namespace be.Domain.Helpers;

public static class ConversationKeyPartHelper
{
    public static long Build(short userAId, short userBId)
    {
        var min = Math.Min(userAId, userBId);
        var max = Math.Max(userAId, userBId);
        return ((long)min << 32) | (ushort)max;
    }

    public static (int Min, int Max) Decode(long keyPart)
    {
        return ((int)(keyPart >> 32), (int)(keyPart & 0xFFFFFFFFL));
    }
}