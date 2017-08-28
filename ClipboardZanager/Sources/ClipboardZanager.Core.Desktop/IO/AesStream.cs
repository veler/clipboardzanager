using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.Core;
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;

namespace ClipboardZanager.Core.Desktop.IO
{
    /// <summary>
    /// Provides a <see cref="Stream"/> crypted by AES algorithm, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    internal sealed class AesStream : Stream
    {
        #region Fields

        private readonly Stream _baseStream;
        private readonly AesManaged _aes;
        private readonly ICryptoTransform _encryptor;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that defines whether the base stream must be disposed when the <see cref="AesStream"/> is disposing.
        /// </summary>
        public bool AutoDisposeBaseStream { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => _baseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _baseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _baseStream.CanWrite;

        /// <inheritdoc/>
        public override long Length => _baseStream.Length;

        /// <inheritdoc/>
        public override long Position { get { return _baseStream.Position; } set { _baseStream.Position = value; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="AesStream"/> class.
        /// </summary>
        /// <param name="baseStream">The stream to read or write with encryption.</param>
        /// <param name="password">The password used to encrypt or decrypt the data.</param> 
        /// <param name="salt">Must be unique for each stream otherwise there is NO security.</param>
        public AesStream(Stream baseStream, SecureString password, byte[] salt)
        {
            Requires.NotNull(baseStream, nameof(baseStream));
            Requires.NotNull(password, nameof(password));
            Requires.NotNull(salt, nameof(salt));

            _baseStream = baseStream;
            using (var key = new Rfc2898DeriveBytes(SecurityHelper.ToUnsecureString(password), salt))
            {
                _aes = new AesManaged()
                {
                    KeySize = 128,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                };
                _aes.Key = key.GetBytes(_aes.KeySize / 8);
                _aes.IV = new byte[16]; // zero buffer is adequate since we have to use new salt for each stream
                _encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);
            }

            AutoDisposeBaseStream = true;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Flush()
        {
            _baseStream.Flush();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var streamPos = Position;
            var ret = _baseStream.Read(buffer, offset, count);
            Cipher(buffer, offset, count, streamPos);
            return ret;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Cipher(buffer, offset, count, Position);
            _baseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encryptor?.Dispose();
                _aes?.Dispose();
                if (AutoDisposeBaseStream)
                {
                    _baseStream?.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Cipher or decipher a byte array.
        /// </summary>
        /// <param name="buffer">The byte array to cipher or decipher</param>
        /// <param name="offset">The offset</param>
        /// <param name="count">The buffer size</param>
        /// <param name="streamPos">The position of the buffer in the base stream</param>
        private void Cipher(byte[] buffer, int offset, int count, long streamPos)
        {
            // find block number
            var blockSizeInByte = _aes.BlockSize / 8;
            var blockNumber = streamPos / blockSizeInByte + 1;
            var keyPos = streamPos % blockSizeInByte;

            // buffer
            var outBuffer = new byte[blockSizeInByte];
            var nonce = new byte[blockSizeInByte];
            var init = false;

            for (var i = offset; i < count; i++)
            {
                // encrypt the nonce to form next xor buffer (unique key)
                if (!init || keyPos % blockSizeInByte == 0)
                {
                    BitConverter.GetBytes(blockNumber).CopyTo(nonce, 0);
                    _encryptor.TransformBlock(nonce, 0, nonce.Length, outBuffer, 0);
                    if (init)
                    {
                        keyPos = 0;
                    }
                    init = true;
                    blockNumber++;
                }
                buffer[i] ^= outBuffer[keyPos]; // simple XOR with generated unique key
                keyPos++;
            }
        }

        #endregion
    }
}
