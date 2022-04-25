using System;
using System.IO;

namespace StoryboardSystem; 

internal class LZSSDecoder : Stream, IDisposable {
    private const int OFFSET_BITS = 5;
    private const int LENGTH_BITS = 3;
    private const int SEARCH_LENGTH = 1 << OFFSET_BITS;
    private const int LOOKAHEAD_LENGTH = (1 << LENGTH_BITS) - 1;
    private const int BUFFER_LENGTH = 4096;
    
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => inStream.Length;
    public override long Position {
        get => inStream.Position;
        set => throw new NotSupportedException();
    }

    private Stream inStream;
    private byte[] inBuffer = new byte[BUFFER_LENGTH];
    private int bufferPosition = BUFFER_LENGTH;
    private byte[] search = new byte[SEARCH_LENGTH];
    private int searchPosition = 0;
    private int endOfInput = BUFFER_LENGTH + 1;
    private int runLength;
    private int runOffset;

    public LZSSDecoder(Stream inStream) => this.inStream = inStream;

    public override void Flush() { }

    public override void SetLength(long value) { }

    public override void Write(byte[] buffer, int offset, int count) { }

    public override void Close() => Dispose(true);

    public override int Read(byte[] buffer, int offset, int count) {
        int end = offset + count;
        int i = offset;

        while (i < end && bufferPosition < endOfInput) {
            byte value;
            
            while (runLength > 0) {
                value = search[(searchPosition + runOffset) % SEARCH_LENGTH];
                search[searchPosition % SEARCH_LENGTH] = value;
                searchPosition++;
                buffer[i] = value;
                i++;
                runLength--;

                if (i == end)
                    return i - offset;
            }

            value = InputByte();

            if (value == 0) {
                value = InputByte();
                search[searchPosition % SEARCH_LENGTH] = value;
                searchPosition++;
                buffer[i] = value;
                i++;
                
                continue;
            }

            runLength = (value >> (8 - LENGTH_BITS)) & LOOKAHEAD_LENGTH;
            runOffset = value & (SEARCH_LENGTH - 1);
        }

        return i - offset;
    }

    public override long Seek(long offset, SeekOrigin origin) => 0;
    
    protected override void Dispose(bool disposing) {
        if (!disposing)
            return;
        
        inStream.Close();
    }

    void IDisposable.Dispose() => Dispose(true);

    private byte InputByte() {
        if (bufferPosition == BUFFER_LENGTH) {
            bufferPosition = 0;
            endOfInput = BUFFER_LENGTH + 1;
            
            while (bufferPosition < BUFFER_LENGTH) {
                int countRead = inStream.Read(inBuffer, bufferPosition, BUFFER_LENGTH - bufferPosition);
                
                if (countRead == 0) {
                    endOfInput = bufferPosition;
                    
                    break;
                }
                
                bufferPosition += countRead;
            }

            bufferPosition = 0;
        }

        int currentPosition = bufferPosition;

        bufferPosition++;

        return inBuffer[currentPosition];
    }
}