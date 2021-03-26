namespace ElectronHostedBlazor.Rendering
{
   using System;
   using System.Diagnostics;
   using System.IO;
   using NodeHostEnvironment;

   internal sealed class ReusableArrayBufferStream : Stream
   {
      private int _length;
      private int _position;
      private ArrayBuffer _buffer;

      public ReusableArrayBufferStream(ArrayBuffer initialBuffer)
      {
         Debug.Assert(initialBuffer != null);
         _buffer = initialBuffer;
      }

      public ArrayBuffer Buffer => _buffer;

      public override void Flush()
      {
         // NOP
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         throw new NotSupportedException();
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         if (offset > int.MaxValue || offset < int.MinValue)
            throw new ArgumentException("Only integer offsets are supported");
         switch (origin)
         {
            case SeekOrigin.Begin:
               _position = (int)offset;
               break;
            case SeekOrigin.Current:
               _position += (int)offset;
               break;
            case SeekOrigin.End:
               _position = _length - (int)offset;
               break;
            default:
               throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
         }

         return _position;
      }

      public override void SetLength(long value)
      {
         if (value > int.MaxValue)
            throw new ArgumentException("Only integer sizes are supported");
         if (value > _buffer.ByteLength)
         {
            ResizeBuffer((int)value);
         }

         _length = (int)value;
         if (_position > _length)
            _position = _length;
      }

      public override unsafe void Write(byte[] buffer, int offset, int count)
      {
         var endPosition = _position + count;
         if (endPosition > _buffer.ByteLength)
         {
            ResizeBuffer(endPosition);
         }

         fixed (byte* srcPtr = buffer)
            System.Buffer.MemoryCopy(srcPtr + offset,
                                     (byte*)_buffer.Address.ToPointer() + _position,
                                     _buffer.ByteLength - _position,
                                     count);

         _position = endPosition;
         if (endPosition > _length)
            _length = endPosition;
      }

      private void ResizeBuffer(in int requiredSize)
      {
         var size = _buffer.ByteLength;
         while (size < requiredSize)
            size <<= 1;

         /*var previous = _buffer;         
         _buffer = CreateArrayBuffer(size);
         System.Buffer.BlockCopy(previous, 0, _buffer, 0, previous.Length);*/
         _buffer = _buffer.JsObject.transfer(size);
      }

      public override bool CanRead => false;

      public override bool CanSeek => true;

      public override bool CanWrite => true;

      public override long Length => _length;

      public override long Position
      {
         get => _position;
         set
         {
            if (value > int.MaxValue || value < int.MinValue)
               throw new ArgumentException("Only integer offsets are supported");
            if (_position > _length)
               SetLength(_position);
         }
      }
   }
}
