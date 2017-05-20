using System;
using System.IO;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Plugin.CropImage {
    public class RandomStream : IRandomAccessStream {
        Stream internstream;

        public RandomStream(Stream underlyingstream) {
            internstream = underlyingstream;
        }

        public IInputStream GetInputStreamAt(ulong position) {
            //THANKS Microsoft! This is GREATLY appreciated!
            internstream.Position = (long)position;
            return internstream.AsInputStream();
        }

        public IOutputStream GetOutputStreamAt(ulong position) {
            internstream.Position = (long)position;
            return internstream.AsOutputStream();
        }

        public ulong Size {
            get {
                return (ulong)internstream.Length;
            }
            set {
                internstream.SetLength((long)value);
            }
        }

        public bool CanRead {
            get { return internstream.CanRead; }
        }

        public bool CanWrite {
            get { return internstream.CanWrite; }
        }

        public IRandomAccessStream CloneStream() {
            throw new NotSupportedException();
        }

        public ulong Position {
            get { return (ulong)internstream.Position; }
        }

        public void Seek(ulong position) {
            internstream.Seek((long)position, SeekOrigin.Begin);
        }

        public void Dispose() {
            internstream.Dispose();
        }


        IAsyncOperationWithProgress<IBuffer, uint> IInputStream.ReadAsync(IBuffer buffer, uint count, InputStreamOptions options) {
            return GetInputStreamAt(Position).ReadAsync(buffer, count, options);
        }

        IAsyncOperationWithProgress<uint, uint> IOutputStream.WriteAsync(IBuffer buffer) {
            return GetOutputStreamAt(Position).WriteAsync(buffer);
        }

        IAsyncOperation<bool> IOutputStream.FlushAsync() {
            return GetOutputStreamAt(Position).FlushAsync();
        }
    }
}
