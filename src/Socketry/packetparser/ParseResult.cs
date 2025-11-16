namespace packetparser
{
    public record ParseResult(Packet packet, int bytesConsumed) { }
}