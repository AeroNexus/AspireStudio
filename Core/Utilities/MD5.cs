using System;
using System.Text;

namespace Aspire.Core.Utilities
{
	public class MD5
	{
		public static void Generate(string text, byte[] digest)
		{
			MD5 md5 = new MD5();
			md5.Update(text.ToCharArray(),text.Length-1);
			md5.Final(digest);
		}

		UInt32[] mBuf = new UInt32[4], mBits = new UInt32[2];
		UInt32[] mInput = new UInt32[16], mZero = new UInt32[16];

		MD5()
		{
			mBuf[0] = 0x67452301;
			mBuf[1] = 0xefcdab89;
			mBuf[2] = 0x98badcfe;
			mBuf[3] = 0x10325476;

			mBits[0] = 0;
			mBits[1] = 0;
		}

		void Final(byte[] digest)
		{
			uint count;

			/* Compute number of bytes mod 64 */
			count = (mBits[0] >> 3) & 0x3F;

			/* Set the first char of padding to 0x80.  This is safe since there is
			   always at least one byte free */
			int p = (int)count;
			Buffer.SetByte(mInput, p++, 0x80);

			/* Bytes of padding needed to make 64 bytes */
			count = 64 - 1 - count;

			/* Pad ref to 56 mod 64 */
			if (count < 8)
			{
				// Two lots of padding:  Pad the first block to 64 bytes

				Buffer.BlockCopy(mZero,0,mInput, p, (int)count);
				//byteReverse(mInput, 16);
				Transform();

				/* Now fill the next block with 56 bytes */
				Buffer.BlockCopy(mZero, 0, mInput, 0, 56);
			}
			else
			{
				/* Pad block to 56 bytes */
				Buffer.BlockCopy(mZero, 0, mInput, p, (int)(count-8));
			}
			//byteReverse(mInput, 14);

			/* Append length in bits and transform */
			Buffer.BlockCopy(mBits, 0, mInput, 14*4, 8);

			Transform();
			//byteReverse((unsigned char *) mBuf, 4);
			Buffer.BlockCopy(mBuf, 0, digest, 0, 16);
		}

		delegate UInt32 Function(UInt32 x, UInt32 y, UInt32 z);

		void MD5STEP(Function f, ref UInt32 w, UInt32 x, UInt32 y, UInt32 z, UInt32 data, int s)
		{
			w += f(x, y, z) + data;
			w = (w<<s) | (w>>(32-s));
			w += x;
		}

		UInt32 F1(UInt32 x, UInt32 y, UInt32 z){ return (z ^ (x & (y ^ z)));}
		UInt32 F2(UInt32 x, UInt32 y, UInt32 z){ return F1(z, x, y);}
		UInt32 F3(UInt32 x, UInt32 y, UInt32 z){ return (x ^ y ^ z);}
		UInt32 F4(UInt32 x, UInt32 y, UInt32 z){ return (y ^ (x | ~z));}

		void Transform()
		{
			UInt32 a, b, c, d;

			a = mBuf[0];
			b = mBuf[1];
			c = mBuf[2];
			d = mBuf[3];

			MD5STEP(F1, ref a, b, c, d, mInput[0] + 0xd76aa478, 7);
			MD5STEP(F1, ref d, a, b, c, mInput[1] + 0xe8c7b756, 12);
			MD5STEP(F1, ref c, d, a, b, mInput[2] + 0x242070db, 17);
			MD5STEP(F1, ref b, c, d, a, mInput[3] + 0xc1bdceee, 22);
			MD5STEP(F1, ref a, b, c, d, mInput[4] + 0xf57c0faf, 7);
			MD5STEP(F1, ref d, a, b, c, mInput[5] + 0x4787c62a, 12);
			MD5STEP(F1, ref c, d, a, b, mInput[6] + 0xa8304613, 17);
			MD5STEP(F1, ref b, c, d, a, mInput[7] + 0xfd469501, 22);
			MD5STEP(F1, ref a, b, c, d, mInput[8] + 0x698098d8, 7);
			MD5STEP(F1, ref d, a, b, c, mInput[9] + 0x8b44f7af, 12);
			MD5STEP(F1, ref c, d, a, b, mInput[10] + 0xffff5bb1, 17);
			MD5STEP(F1, ref b, c, d, a, mInput[11] + 0x895cd7be, 22);
			MD5STEP(F1, ref a, b, c, d, mInput[12] + 0x6b901122, 7);
			MD5STEP(F1, ref d, a, b, c, mInput[13] + 0xfd987193, 12);
			MD5STEP(F1, ref c, d, a, b, mInput[14] + 0xa679438e, 17);
			MD5STEP(F1, ref b, c, d, a, mInput[15] + 0x49b40821, 22);

			MD5STEP(F2, ref a, b, c, d, mInput[1] + 0xf61e2562, 5);
			MD5STEP(F2, ref d, a, b, c, mInput[6] + 0xc040b340, 9);
			MD5STEP(F2, ref c, d, a, b, mInput[11] + 0x265e5a51, 14);
			MD5STEP(F2, ref b, c, d, a, mInput[0] + 0xe9b6c7aa, 20);
			MD5STEP(F2, ref a, b, c, d, mInput[5] + 0xd62f105d, 5);
			MD5STEP(F2, ref d, a, b, c, mInput[10] + 0x02441453, 9);
			MD5STEP(F2, ref c, d, a, b, mInput[15] + 0xd8a1e681, 14);
			MD5STEP(F2, ref b, c, d, a, mInput[4] + 0xe7d3fbc8, 20);
			MD5STEP(F2, ref a, b, c, d, mInput[9] + 0x21e1cde6, 5);
			MD5STEP(F2, ref d, a, b, c, mInput[14] + 0xc33707d6, 9);
			MD5STEP(F2, ref c, d, a, b, mInput[3] + 0xf4d50d87, 14);
			MD5STEP(F2, ref b, c, d, a, mInput[8] + 0x455a14ed, 20);
			MD5STEP(F2, ref a, b, c, d, mInput[13] + 0xa9e3e905, 5);
			MD5STEP(F2, ref d, a, b, c, mInput[2] + 0xfcefa3f8, 9);
			MD5STEP(F2, ref c, d, a, b, mInput[7] + 0x676f02d9, 14);
			MD5STEP(F2, ref b, c, d, a, mInput[12] + 0x8d2a4c8a, 20);

			MD5STEP(F3, ref a, b, c, d, mInput[5] + 0xfffa3942, 4);
			MD5STEP(F3, ref d, a, b, c, mInput[8] + 0x8771f681, 11);
			MD5STEP(F3, ref c, d, a, b, mInput[11] + 0x6d9d6122, 16);
			MD5STEP(F3, ref b, c, d, a, mInput[14] + 0xfde5380c, 23);
			MD5STEP(F3, ref a, b, c, d, mInput[1] + 0xa4beea44, 4);
			MD5STEP(F3, ref d, a, b, c, mInput[4] + 0x4bdecfa9, 11);
			MD5STEP(F3, ref c, d, a, b, mInput[7] + 0xf6bb4b60, 16);
			MD5STEP(F3, ref b, c, d, a, mInput[10] + 0xbebfbc70, 23);
			MD5STEP(F3, ref a, b, c, d, mInput[13] + 0x289b7ec6, 4);
			MD5STEP(F3, ref d, a, b, c, mInput[0] + 0xeaa127fa, 11);
			MD5STEP(F3, ref c, d, a, b, mInput[3] + 0xd4ef3085, 16);
			MD5STEP(F3, ref b, c, d, a, mInput[6] + 0x04881d05, 23);
			MD5STEP(F3, ref a, b, c, d, mInput[9] + 0xd9d4d039, 4);
			MD5STEP(F3, ref d, a, b, c, mInput[12] + 0xe6db99e5, 11);
			MD5STEP(F3, ref c, d, a, b, mInput[15] + 0x1fa27cf8, 16);
			MD5STEP(F3, ref b, c, d, a, mInput[2] + 0xc4ac5665, 23);

			MD5STEP(F4, ref a, b, c, d, mInput[0] + 0xf4292244, 6);
			MD5STEP(F4, ref d, a, b, c, mInput[7] + 0x432aff97, 10);
			MD5STEP(F4, ref c, d, a, b, mInput[14] + 0xab9423a7, 15);
			MD5STEP(F4, ref b, c, d, a, mInput[5] + 0xfc93a039, 21);
			MD5STEP(F4, ref a, b, c, d, mInput[12] + 0x655b59c3, 6);
			MD5STEP(F4, ref d, a, b, c, mInput[3] + 0x8f0ccc92, 10);
			MD5STEP(F4, ref c, d, a, b, mInput[10] + 0xffeff47d, 15);
			MD5STEP(F4, ref b, c, d, a, mInput[1] + 0x85845dd1, 21);
			MD5STEP(F4, ref a, b, c, d, mInput[8] + 0x6fa87e4f, 6);
			MD5STEP(F4, ref d, a, b, c, mInput[15] + 0xfe2ce6e0, 10);
			MD5STEP(F4, ref c, d, a, b, mInput[6] + 0xa3014314, 15);
			MD5STEP(F4, ref b, c, d, a, mInput[13] + 0x4e0811a1, 21);
			MD5STEP(F4, ref a, b, c, d, mInput[4] + 0xf7537e82, 6);
			MD5STEP(F4, ref d, a, b, c, mInput[11] + 0xbd3af235, 10);
			MD5STEP(F4, ref c, d, a, b, mInput[2] + 0x2ad7d2bb, 15);
			MD5STEP(F4, ref b, c, d, a, mInput[9] + 0xeb86d391, 21);
			mBuf[0] += a;
			mBuf[1] += b;
			mBuf[2] += c;
			mBuf[3] += d;
		}

		void Update(char[] sourceChars, int length)
		{
			/* Update bitcount */
			byte[] source = Encoding.ASCII.GetBytes(sourceChars);
			int p, t = (int)mBits[0];
			if ((mBits[0] = (uint)(t + (length << 3))) < t)
				mBits[1]++;		/* Carry from low to high */
			mBits[1] += (uint)(length >> 29);

			t = (t >> 3) & 0x3f;	/* Bytes already in shsInfo->data */

			/* Handle any leading odd-sized chunks */

			if (t > 0)
			{
				p = t;

				t = 64 - t;
				if (length < t)
				{
					Buffer.BlockCopy(source,0,mInput,p,length);
					return;
				}
				Buffer.BlockCopy(source,0,mInput,p,t);
				//byteReverse(mInput, 16);
				Transform();
				p += t;
				length -= t;
			}
			/* Process data in 64-byte chunks */

			p = 0;
			while (length >= 64)
			{
				Buffer.BlockCopy(source, p, mInput, 0, 64);
				//byteReverse(mInput, 16);
				Transform();
				p += 64;
				length -= 64;
			}

			/* Handle any remaining bytes of data. */
      if(length>0)
			  Buffer.BlockCopy(source, p, mInput, 0, length);
		}


	}
}
