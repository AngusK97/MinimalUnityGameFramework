using System;
using System.Security.Cryptography;

namespace FrameworkConfig.Editor
{
    internal static class GoogleServiceKey
    {
        // Unity's Mono does not implement ImportPkcs8PrivateKey. Decode only the
        // two-prime RSA structure used by Google; key operations remain in .NET RSA.
        public static RSA CreateSigner(byte[] encoded)
        {
            if (encoded == null || encoded.Length == 0 || encoded.Length > 64 * 1024)
            {
                throw InvalidKey();
            }

            RSA signer = null;
            var parameters = new RSAParameters();

            try
            {
                var document = new DerReader(encoded, 0, encoded.Length);
                var privateKeyInfo = document.ReadElement(0x30);
                document.RequireEnd();
                privateKeyInfo.ReadZeroVersion();
                var algorithm = privateKeyInfo.ReadElement(0x30);
                algorithm.ReadElement(0x06).RequireBytes(new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 });
                algorithm.ReadElement(0x05).RequireEnd();
                algorithm.RequireEnd();
                var octets = privateKeyInfo.ReadElement(0x04);
                privateKeyInfo.RequireEnd();
                var key = octets.ReadElement(0x30);
                octets.RequireEnd();
                key.ReadZeroVersion();
                parameters.Modulus = key.ReadPositiveInteger();
                parameters.Exponent = key.ReadPositiveInteger();
                parameters.D = key.ReadPositiveInteger();
                parameters.P = key.ReadPositiveInteger();
                parameters.Q = key.ReadPositiveInteger();
                parameters.DP = key.ReadPositiveInteger();
                parameters.DQ = key.ReadPositiveInteger();
                parameters.InverseQ = key.ReadPositiveInteger();
                key.RequireEnd();

                int size = parameters.Modulus.Length;

                if (size < 256 || (size == 256 && parameters.Modulus[0] < 0x80)
                    || parameters.Exponent.Length > size
                    || (parameters.Exponent[parameters.Exponent.Length - 1] & 1) == 0
                    || (parameters.Exponent.Length == 1 && parameters.Exponent[0] == 1))
                {
                    throw InvalidKey();
                }

                parameters.D = Pad(parameters.D, size);
                int halfSize = (size + 1) / 2;
                parameters.P = Pad(parameters.P, halfSize);
                parameters.Q = Pad(parameters.Q, halfSize);
                parameters.DP = Pad(parameters.DP, halfSize);
                parameters.DQ = Pad(parameters.DQ, halfSize);
                parameters.InverseQ = Pad(parameters.InverseQ, halfSize);
                signer = RSA.Create();
                signer.ImportParameters(parameters);

                if (signer.KeySize < 2048)
                {
                    throw InvalidKey();
                }

                return signer;
            }
            catch
            {
                signer?.Dispose();
                throw;
            }
            finally
            {
                Clear(parameters.Modulus);
                Clear(parameters.Exponent);
                Clear(parameters.D);
                Clear(parameters.P);
                Clear(parameters.Q);
                Clear(parameters.DP);
                Clear(parameters.DQ);
                Clear(parameters.InverseQ);
            }
        }

        private static byte[] Pad(byte[] value, int size)
        {
            if (value.Length > size)
            {
                throw InvalidKey();
            }

            if (value.Length == size)
            {
                return value;
            }

            var padded = new byte[size];
            Buffer.BlockCopy(value, 0, padded, size - value.Length, value.Length);
            Clear(value);
            return padded;
        }

        private static void Clear(byte[] bytes)
        {
            if (bytes != null)
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private static CryptographicException InvalidKey()
        {
            return new CryptographicException("Expected an unencrypted, two-prime PKCS#8 RSA key of at least 2048 bits.");
        }

        private sealed class DerReader
        {
            private readonly byte[] _data;
            private readonly int _end;
            private int _position;

            public DerReader(byte[] data, int position, int length)
            {
                _data = data;
                _position = position;
                _end = position + length;
            }

            public DerReader ReadElement(byte expectedTag)
            {
                if (ReadByte() != expectedTag)
                {
                    throw InvalidKey();
                }

                int firstLength = ReadByte();
                long length = firstLength;

                if ((firstLength & 0x80) != 0)
                {
                    int count = firstLength & 0x7f;

                    if (count == 0 || count > 4)
                    {
                        throw InvalidKey();
                    }

                    length = ReadByte();

                    if (length == 0)
                    {
                        throw InvalidKey();
                    }

                    for (var index = 1; index < count; index++)
                    {
                        length = (length << 8) | ReadByte();
                    }

                    if (length < 128)
                    {
                        throw InvalidKey();
                    }
                }

                if (length > _end - _position)
                {
                    throw InvalidKey();
                }

                var element = new DerReader(_data, _position, (int)length);
                _position += (int)length;
                return element;
            }

            public void ReadZeroVersion()
            {
                ReadElement(0x02).RequireBytes(new byte[] { 0 });
            }

            public byte[] ReadPositiveInteger()
            {
                var integer = ReadElement(0x02);
                int first = integer.ReadByte();

                if ((first & 0x80) != 0)
                {
                    throw InvalidKey();
                }

                int start = integer._position - 1;

                if (first == 0)
                {
                    // DER adds one sign byte only when the unsigned value's high bit is set.
                    if ((integer.ReadByte() & 0x80) == 0)
                    {
                        throw InvalidKey();
                    }

                    start++;
                }

                var value = new byte[integer._end - start];
                Buffer.BlockCopy(_data, start, value, 0, value.Length);
                return value;
            }

            public void RequireBytes(byte[] expected)
            {
                foreach (byte value in expected)
                {
                    if (ReadByte() != value)
                    {
                        throw InvalidKey();
                    }
                }

                RequireEnd();
            }

            public void RequireEnd()
            {
                if (_position != _end)
                {
                    throw InvalidKey();
                }
            }

            private byte ReadByte()
            {
                if (_position >= _end)
                {
                    throw InvalidKey();
                }

                return _data[_position++];
            }
        }
    }
}
