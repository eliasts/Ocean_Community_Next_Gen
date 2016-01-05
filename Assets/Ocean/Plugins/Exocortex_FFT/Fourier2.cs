/*
 * BSD Licence:
 * Copyright (c) 2001, 2002 Ben Houston [ ben@exocortex.org ]
 * Exocortex Technologies [ www.exocortex.org ]
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the 
 * documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the <ORGANIZATION> nor the names of its contributors
 * may be used to endorse or promote products derived from this software
 * without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
 * DAMAGE.
 */
 #if !NATIVE

using System;

	// Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
	// Version: May 4, 2002

	/// <summary>
	/// <p>Static functions for doing various Fourier Operations.</p>
	/// </summary>
	public class Fourier2 {
		
		//======================================================================================

		private Fourier2() {
		}

		//-------------------------------------------------------------------------------------
		
		private const int	cMaxLength	= 4096;
		private const int	cMinLength	= 1;

		private const int	cMaxBits	= 12;
		private const int	cMinBits	= 0;

		private const float _PI = (float)Math.PI;
	

		static private bool	IsPowerOf2( int x ) {
			return	(x & (x - 1)) == 0;
			//return	( x == Pow2( Log2( x ) ) );
		}
		static private int	Pow2( int exponent ) {
			if( exponent >= 0 && exponent < 31 ) {
				return	1 << exponent;
			}
			return	0;
		}
		static private int	Log2( int x ) {
			if( x <= 65536 ) {
				if( x <= 256 ) {
					if( x <= 16 ) {
						if( x <= 4 ) {	
							if( x <= 2 ) {
								if( x <= 1 ) {
									return	0;
								}
								return	1;	
							}
							return	2;				
						}
						if( x <= 8 )
							return	3;			
						return	4;				
					}
					if( x <= 64 ) {
						if( x <= 32 )
							return	5;	
						return	6;				
					}
					if( x <= 128 )
						return	7;		
					return	8;				
				}
				if( x <= 4096 ) {	
					if( x <= 1024 ) {	
						if( x <= 512 )
							return	9;			
						return	10;				
					}
					if( x <= 2048 )
						return	11;			
					return	12;				
				}
				if( x <= 16384 ) {
					if( x <= 8192 )
						return	13;			
					return	14;				
				}
				if( x <= 32768 )
					return	15;	
				return	16;	
			}
			if( x <= 16777216 ) {
				if( x <= 1048576 ) {
					if( x <= 262144 ) {	
						if( x <= 131072 )
							return	17;			
						return	18;				
					}
					if( x <= 524288 )
						return	19;			
					return	20;				
				}
				if( x <= 4194304 ) {
					if( x <= 2097152 )
						return	21;	
					return	22;				
				}
				if( x <= 8388608 )
					return	23;		
				return	24;				
			}
			if( x <= 268435456 ) {	
				if( x <= 67108864 ) {	
					if( x <= 33554432 )
						return	25;			
					return	26;				
				}
				if( x <= 134217728 )
					return	27;			
				return	28;				
			}
			if( x <= 1073741824 ) {
				if( x <= 536870912 )
					return	29;			
				return	30;				
			}
			//	since int is unsigned it can never be higher than 2,147,483,647
			//	if( x <= 2147483648 )
			//		return	31;	
			//	return	32;	
			return	31;
		}

		//-------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------
		
		static private int[][]	_reversedBits	= new int[ cMaxBits ][];
		static private int[]		GetReversedBits( int numberOfBits ) {

			if( _reversedBits[ numberOfBits - 1 ] == null ) {
				int		maxBits = Pow2( numberOfBits );
				int[]	reversedBits = new int[ maxBits ];
				for( int i = 0; i < maxBits; i ++ ) {
					int oldBits = i; 
					int newBits = 0;
					for( int j = 0; j < numberOfBits; j ++ ) {
						newBits = ( newBits << 1 ) | ( oldBits & 1 );
						oldBits = ( oldBits >> 1 );
					}
					reversedBits[ i ] = newBits;
				}
				_reversedBits[ numberOfBits - 1 ] = reversedBits;
			}
			return	_reversedBits[ numberOfBits - 1 ];
		}

		//-------------------------------------------------------------------------------------



		static private void ReorderArray( ComplexF[] data ) {

			int length = data.Length;

			int[] reversedBits = GetReversedBits( Log2( length ) );
			for( int i = 0; i < length; i ++ ) {
				int swap = reversedBits[ i ];
				if( swap > i ) {
					ComplexF temp = data[ i ];
					data[ i ] = data[ swap ];
					data[ swap ] = temp;
				}
			}
		}

		//======================================================================================

		private static int[][]	_reverseBits = null;

		private static int	_ReverseBits( int bits, int n ) {
			int bitsReversed = 0;
			for( int i = 0; i < n; i ++ ) {
				bitsReversed = ( bitsReversed << 1 ) | ( bits & 1 );
				bits = ( bits >> 1 );
			}
			return bitsReversed;
		}
		/*
		static int MyIntPow(int num, int exp){
			int result = 1;
			while (exp > 0) {
				if (exp % 2 == 1) result *= num;
				exp >>= 1;
				num *= num;
			}
			return result;
		}*/


		private static void	InitializeReverseBits( int levels ) {
			_reverseBits = new int[levels + 1][];
			for( int j = 0; j < ( levels + 1 ); j ++ ) {
				int count = (int) Math.Pow( 2, j );
				_reverseBits[j] = new int[ count ];
				for( int i = 0; i < count; i ++ ) {
					_reverseBits[j][i] = _ReverseBits( i, j );
				}
			}
		}

		private static int _lookupTabletLength = -1;
		private static float[,][]	_uRLookup	= null;
		private static float[,][]	_uILookup	= null;
		private static	float[,][]	_uRLookupF	= null;
		private static	float[,][]	_uILookupF	= null;

		static int MyCeilIntF(float g) {
			if(g>=0)return (int)g+1; else return (int)g;
		}

		private static void	SyncLookupTableLength( int length ) {

			if( length > _lookupTabletLength ) {
				//int level = (int) Math.Ceiling( Math.Log( length, 2 ) );
				int level = MyCeilIntF( (float)Math.Log( length, 2 ) );
				InitializeReverseBits( level );
				InitializeComplexRotations( level );
				_lookupTabletLength = length;
			}
		}

		private static int	GetLookupTableLength() {
			return	_lookupTabletLength;
		}

		public static void	ClearLookupTables() {
			_uRLookup	= null;
			_uILookup	= null;
			_uRLookupF	= null;
			_uILookupF	= null;
			_lookupTabletLength	= -1;
		}
		
		private static void InitializeComplexRotations( int levels ) {
			int ln = levels;
			
			_uRLookup = new float[ levels + 1, 2 ][];
			_uILookup = new float[ levels + 1, 2 ][];

			_uRLookupF = new float[ levels + 1, 2 ][];
			_uILookupF = new float[ levels + 1, 2 ][];

			int N = 1;
			for( int level = 1; level <= ln; level ++ ) {
				int M = N;
				N <<= 1;

				//float scale = (float)( 1 / Math.Sqrt( 1 << ln ) );

				// positive sign ( i.e. [M,0] )
				{
					float	uR = 1;
					float	uI = 0;
					float	angle = _PI / M * 1;
					float	wR = (float) Math.Cos( angle );
					float	wI = (float) Math.Sin( angle );

					_uRLookup[level,0] = new float[ M ];
					_uILookup[level,0] = new float[ M ];
					_uRLookupF[level,0] = new float[ M ];
					_uILookupF[level,0] = new float[ M ];

					for( int j = 0; j < M; j ++ ) {
						_uRLookupF[level,0][j] = _uRLookup[level, 0][j] = uR;
						_uILookupF[level,0][j] = _uILookup[level, 0][j] = uI;
						float	uwI = uR*wI + uI*wR;
						uR = uR*wR - uI*wI;
						uI = uwI;
					}
				}
				{


				// negative sign ( i.e. [M,1] )
					float	uR = 1;
                    float	uI = 0;
					float	angle = _PI / M * -1;
					float	wR = (float) Math.Cos( angle );
					float	wI = (float) Math.Sin( angle );

					_uRLookup[level,1] = new float[ M ];
					_uILookup[level,1] = new float[ M ];
					_uRLookupF[level,1] = new float[ M ];
					_uILookupF[level,1] = new float[ M ];

					for( int j = 0; j < M; j ++ ) {
						_uRLookupF[level,1][j] = _uRLookup[level, 1][j] = uR;
						_uILookupF[level,1][j] = _uILookup[level, 1][j] = uI;
						float	uwI = uR*wI + uI*wR;
						uR = uR*wR - uI*wI;
						uI = uwI;
					}
				}

			}
		}
		

		//======================================================================================
		
		static private ComplexF[]	_bufferCF		= new ComplexF[ 0 ];

		static private void		LockBufferCF( int length, ref ComplexF[] buffer ) {
			if( length != _bufferCF.Length ) {
				_bufferCF	= new ComplexF[ length ];
			}
			buffer =	_bufferCF;
		}

		static private void		UnlockBufferCF( ref ComplexF[] buffer ) {
			buffer = null;
		}



		private static void	LinearFFT_Quick( ComplexF[] data, int start, int inc, int length, FourierDirection direction ) {
			
			// copy to buffer
			ComplexF[]	buffer = null;
			LockBufferCF( length, ref buffer );
			int j = start;
			for( int i = 0; i < length; i ++ ) {
				buffer[ i ] = data[ j ];
				j += inc;
			}

			FFT( buffer, length, direction );

			// copy from buffer
			j = start;
			for( int i = 0; i < length; i ++ ) {
				data[ j ] = buffer[ i ];
				j += inc;
			}
			UnlockBufferCF( ref buffer );
		}


		//======================================================================================
		 

		/// <summary>
		/// Compute a 1D fast Fourier transform of a dataset of complex numbers.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		/// <param name="direction"></param>
		public static void	FFT( ComplexF[] data, int length, FourierDirection direction ) {

			SyncLookupTableLength( length );

			int ln = Log2( length );
			
			// reorder array
			ReorderArray( data );
			
			// successive doubling
			int N = 1;
			int signIndex = ( direction == FourierDirection.Forward ) ? 0 : 1;
			
			for( int level = 1; level <= ln; level ++ ) {
				int M = N;
				N <<= 1;

				float[] uRLookup = _uRLookupF[ level, signIndex ];
				float[] uILookup = _uILookupF[ level, signIndex ];

				for( int j = 0; j < M; j ++ ) {
					float uR = uRLookup[j];
					float uI = uILookup[j];

					for( int even = j; even < length; even += N ) {
						int odd	 = even + M;
						
						float	r = data[ odd ].Re;
						float	i = data[ odd ].Im;

						float	odduR = r * uR - i * uI;
						float	odduI = r * uI + i * uR;

						r = data[ even ].Re;
						i = data[ even ].Im;
						
						data[ even ].Re	= r + odduR;
						data[ even ].Im	= i + odduI;
						
						data[ odd ].Re	= r - odduR;
						data[ odd ].Im	= i - odduI;
					}
				}
			}

		}



		/// <summary>
		/// Compute a 2D fast fourier transform on a data set of complex numbers
		/// </summary>
		/// <param name="data"></param>
		/// <param name="xLength"></param>
		/// <param name="yLength"></param>
		/// <param name="direction"></param>
		public static void	FFT2( ComplexF[] data, int xLength, int yLength, FourierDirection direction ) {

			int xInc = 1;
			int yInc = xLength;

			if( xLength > 1 ) {
				SyncLookupTableLength( xLength );
				for( int y = 0; y < yLength; y ++ ) {
					int xStart = y * yInc;
					LinearFFT_Quick( data, xStart, xInc, xLength, direction );
				}
			}
		
			if( yLength > 1 ) {
				SyncLookupTableLength( yLength );
				for( int x = 0; x < xLength; x ++ ) {
					int yStart = x * xInc;
					LinearFFT_Quick( data, yStart, yInc, yLength, direction );
				}
			}
		}

		
	}
 #endif
