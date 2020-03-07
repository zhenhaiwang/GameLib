using System;

namespace GameLib
{
    /// <summary>
    /// Random number generator using Mersenne Twister algorithm
    /// </summary>
    /// <remarks>
    /// Inherits from System.Random
    /// </remarks>
    public class MersenneTwisterRandom : System.Random
    {
        private UInt32[] m_StateVector;
        private const int m_StateVectorSize = 624;
        private const int m_ValueM = 397;
        private const UInt32 m_UpperMask = 0x80000000; // most significant w-r bits
        private const UInt32 m_LowerMask = 0x7fffffff; // least significant r bits
        private UInt32[] m_Mag01 = { 0, 0x9908b0df }; // used in generating next random number
        private int m_NextValueIndex;

        /// <summary>
        /// Initializes a new instance of the MersenneTwisterRandom class, 
        /// using a time-dependent default seed value.
        /// </summary>
        public MersenneTwisterRandom()
        {
            initialise((UInt32)(System.DateTime.Now.Ticks & 0xFFFFFFFF));
        }

        /// <summary>
        /// Initializes a new instance of the MersenneTwisterRandom class, 
        /// using the specified seed value.
        /// </summary>
        /// <param name="seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        public MersenneTwisterRandom(int seed)
        {
            initialise((UInt32)seed);
        }

        /// <summary>
        /// Initialise the random number generator, using the specified seed value.
        /// </summary>
        /// <param name="seed">
        /// A number used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        private void initialise(UInt32 seed)
        {
            const UInt32 multiplier = 1812433253;
            m_StateVector = new UInt32[m_StateVectorSize];
            m_StateVector[0] = seed;
            for (int index = 1; index < m_StateVectorSize; index++)
            {
                m_StateVector[index] =
                    (multiplier * (m_StateVector[index - 1] ^ (m_StateVector[index - 1] >> 30)) + (UInt32)index);
            }
            m_NextValueIndex = m_StateVectorSize;
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A non-negative random integer between 0 and 2147483646.
        /// </returns>
        // Appears need to override Next even if overriding Sample as
        // doesn't appear to use overridden Sample method, at least not in
        // a way which I understand.
        public override int Next()
        {
            // convert down to signed int
            return (int)(GenerateRandomValue() & 0x7FFFFFFF);
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified 
        /// maximum.
        /// </summary>
        /// <param name="maxValue">
        /// The upper bound of the random number to be generated. 
        /// maxValue must be greater than or equal to zero.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and
        /// less than maxValue; that is, the range of return values 
        /// includes zero but not MaxValue.
        /// </returns>
        // Explicitly override so intellisense displays this type!
        public override int Next(int maxValue)
        {
            return base.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">
        /// The lower bound of the random number returned.
        /// </param>
        /// <param name="maxValue">
        /// The upper bound of the random number returned. maxValue must 
        /// be greater than or equal to minValue.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to minValue and 
        /// less than maxValue; that is, the range of return values 
        /// includes minValue but not MaxValue. If minValue equals 
        /// maxValue, minValue is returned.
        /// </returns>
        /// <remarks>
        /// Overrides base method to ensure negative values rounded in
        /// same way under Mono and Microsoft® .NET Framework.
        /// </remarks>
        public override int Next(int minValue, int maxValue)
        {
            return (int)Math.Floor(Sample() * (double)(maxValue - minValue) + (double)minValue);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or 
        /// equal to 0.0, and less than 1.0.
        /// </returns>
        /// <remarks>
        /// This method is the public version of the protected method, 
        /// <see cref="Sample"/>
        /// </remarks>
        public override double NextDouble()
        {
            return base.NextDouble();
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes to contain random numbers.
        /// </param>
        // Appears need to override NextBytes even if overriding Sample as
        // doesn't appear to use overridden Sample method, at least not in
        // a way which I understand.
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            int bufferLength = buffer.Length;
            int numberValues = (int)((bufferLength + 3) / 4);

            int bufferIndex = 0;
            for (int index = 0; index < numberValues; index++)
            {
                UInt32 randVal = GenerateRandomValue();
                byte val = (byte)((randVal & 0xFF000000) >> 24);
                buffer[bufferIndex++] = val;
                if (bufferIndex > bufferLength) break;
                val = (byte)((randVal & 0x00FF0000) >> 16);
                buffer[bufferIndex++] = val;
                if (bufferIndex > bufferLength) break;
                val = (byte)((randVal & 0x0000FF00) >> 8);
                buffer[bufferIndex++] = val;
                if (bufferIndex > bufferLength) break;
                val = (byte)(randVal & 0x000000FF);
                buffer[bufferIndex++] = val;
                if (bufferIndex > bufferLength) break;
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// Random number between 0.0 and 1.0.
        /// </returns>
        protected override double Sample()
        {
            double twoPower32 = 4294967296.0;
            return ((double)GenerateRandomValue()) / twoPower32;
        }

        /// <summary>
        /// Returns a random 32 bit intger between 0 and 4294967295 inclusive.
        /// </summary>
        /// <returns>
        /// Random 32 bit intger between 0 and 4294967295 inclusive.
        /// </returns>
        private UInt32 GenerateRandomValue()
        {
            const UInt32 temperingValue1 = 0x9d2c5680;
            const UInt32 temperingValue2 = 0xefc60000;
            UInt32 randValue;
            UInt32 intermediate;

            if (m_NextValueIndex >= m_StateVectorSize)
            {
                int index;
                for (index = 0; index < m_StateVectorSize - m_ValueM; index++)
                {
                    intermediate = (m_StateVector[index] & m_UpperMask) |
                        (m_StateVector[index + 1] & m_LowerMask);
                    m_StateVector[index] =
                        m_StateVector[index + m_ValueM] ^ (intermediate >> 1) ^
                        m_Mag01[(int)(intermediate & 1)];
                }
                for (; index < m_StateVectorSize - 1; index++)
                {
                    intermediate = (m_StateVector[index] & m_UpperMask) |
                        (m_StateVector[index + 1] & m_LowerMask);
                    m_StateVector[index] =
                        m_StateVector[index + m_ValueM - m_StateVectorSize] ^
                        (intermediate >> 1) ^
                        m_Mag01[(int)(intermediate & 1)];
                }
                intermediate = (m_StateVector[m_StateVectorSize - 1] & m_UpperMask) |
                    (m_StateVector[0] & m_LowerMask);
                m_StateVector[m_StateVectorSize - 1] = m_StateVector[m_ValueM - 1] ^
                    (intermediate >> 1) ^
                    m_Mag01[(int)(intermediate & 1)];

                m_NextValueIndex = 0;
            }
            randValue = m_StateVector[m_NextValueIndex++];

            // Tempering 
            randValue ^= (randValue >> 11);
            randValue ^= (randValue << 7) & temperingValue1;
            randValue ^= (randValue << 15) & temperingValue2;
            randValue ^= (randValue >> 18);

            return randValue;
        }
    }
}
