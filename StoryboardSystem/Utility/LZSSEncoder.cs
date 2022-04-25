using System;
using System.IO;

namespace StoryboardSystem; 

internal class LZSSEncoder : Stream, IDisposable {
    private const int OFFSET_BITS = 5;
    private const int LENGTH_BITS = 3;
    private const int SEARCH_LENGTH = 1 << OFFSET_BITS;
    private const int LOOKAHEAD_LENGTH = (1 << LENGTH_BITS) - 1;
    private const int WINDOW_LENGTH = SEARCH_LENGTH + LOOKAHEAD_LENGTH;
    private const int BUFFER_LENGTH = 4096;
    
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => outStream.Length;
    public override long Position {
        get => outStream.Position;
        set => throw new NotSupportedException();
    }

    private Stream outStream;
    private byte[] outBuffer = new byte[BUFFER_LENGTH];
    private int bufferPosition;
    private int[] window;
    private int windowPosition = 0;
    private int readToPosition = LOOKAHEAD_LENGTH;
    
    public LZSSEncoder(Stream outStream) {
        this.outStream = outStream;

        window = new int[WINDOW_LENGTH];

        for (int i = 0; i < WINDOW_LENGTH; i++)
            window[i] = -1;
    }

    public void Complete() {
        while (WindowGet(SEARCH_LENGTH) >= 0)
            Advance(-1);
        
        Flush();
    }

    public override void Flush() {
        outStream.Write(outBuffer, 0, bufferPosition);
        bufferPosition = 0;
    }

    public override void SetLength(long value) { }

    public override void Write(byte[] buffer, int offset, int count) {
        int end = offset + count;
        
        for (int i = offset; i < end; i++)
            Advance(buffer[i]);
    }

    public override void Close() => Dispose(true);

    public override int Read(byte[] buffer, int offset, int count) => 0;

    public override long Seek(long offset, SeekOrigin origin) => 0;

    protected override void Dispose(bool disposing) {
        if (!disposing)
            return;
        
        Complete();
        outStream.Close();
    }

    void IDisposable.Dispose() => Dispose(true);

    private void Advance(int value) {
        window[windowPosition % WINDOW_LENGTH] = value;
        windowPosition++;
        
        if (windowPosition < readToPosition)
            return;
        
        int runOffset = 0;
        int runLength = 0;

        for (int j = 0; j < SEARCH_LENGTH; j++) {
            int newLength = 0;
                
            for (int k = j, l = SEARCH_LENGTH; l < WINDOW_LENGTH; k++, l++) {
                if (WindowGet(k) != WindowGet(l))
                    break;

                newLength++;
            }

            if (newLength < runLength)
                continue;
                
            runOffset = j;
            runLength = newLength;
                    
            if (runLength == LOOKAHEAD_LENGTH)
                break;
        }

        if (runLength > 0) {
            OutputByte((byte) (runOffset | (runLength << (8 - LENGTH_BITS))));
            readToPosition += runLength;
                
            return;
        }

        OutputByte(0);
        OutputByte((byte) WindowGet(SEARCH_LENGTH));
        readToPosition++;
    }

    private void OutputByte(byte value) {
        if (bufferPosition == BUFFER_LENGTH)
            Flush();

        outBuffer[bufferPosition] = value;
        bufferPosition++;
    }

    private int WindowGet(int index) => window[(windowPosition + index) % WINDOW_LENGTH];
}