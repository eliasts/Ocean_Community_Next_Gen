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

using System;


// Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
// Version: May 4, 2002

/// <summary>
/// <p>A single-precision complex number representation.</p>
/// </summary>

//using System.Runtime.InteropServices;
//[StructLayout(LayoutKind.Sequential)]
//unsafe public struct ComplexF {

public struct ComplexF {

	//-----------------------------------------------------------------------------------
	//-----------------------------------------------------------------------------------

	/// <summary>
	/// The real component of the complex number
	/// </summary>
	public float Re;

	/// <summary>
	/// The imaginary component of the complex number
	/// </summary>
	public float Im;

	//-----------------------------------------------------------------------------------
	//-----------------------------------------------------------------------------------

	/// <summary>
	/// Create a complex number from a real and an imaginary component
	/// </summary>
	/// <param name="real"></param>
	/// <param name="imaginary"></param>
	public ComplexF( float real, float imaginary ) {
		Re = real;
		Im = imaginary;
	}


	/// <summary>
	/// Create a complex number from a real and an imaginary component
	/// </summary>
	/// <param name="real"></param>
	/// <param name="imaginary"></param>
	/// <returns></returns>
	static public ComplexF	FromRealImaginary( float real, float imaginary ) {
		ComplexF c;
		c.Re	= real;
		c.Im	= imaginary;
		return c;
	}

	//-----------------------------------------------------------------------------------

	/// <summary>
	/// Get the conjugate of the complex number
	/// </summary>
	/// <returns></returns>
	public ComplexF GetConjugate() {
		return FromRealImaginary(Re, -Im);
	}


	//-----------------------------------------------------------------------------------

	/// <summary>
	/// Are these two complex numbers equivalent?
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static bool	operator==( ComplexF a, ComplexF b ) {
		return	( a.Re == b.Re ) && ( a.Im == b.Im );
	}

	/// <summary>
	/// Are these two complex numbers different?
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static bool	operator!=( ComplexF a, ComplexF b ) {
		return	( a.Re != b.Re ) || ( a.Im != b.Im );
	}

	/// <summary>
	/// Get the hash code of the complex number
	/// </summary>
	/// <returns></returns>
	public override int		GetHashCode() {
		return	(Re.GetHashCode() ^ Im.GetHashCode() );
	}

	/// <summary>
	/// Is this complex number equivalent to another object?
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public override bool	Equals( object o ) {
		if( o is ComplexF ) {
			ComplexF c = (ComplexF) o;
			return   ( this == c );
		}
		return	false;
	}


	//-----------------------------------------------------------------------------------


	/// <summary>
	/// Add to complex numbers
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static ComplexF operator+( ComplexF a, ComplexF b ) {
		a.Re	= a.Re + b.Re;
		a.Im	= a.Im + b.Im;
		return a;
	}



	/// <summary>
	/// Subtract two complex numbers
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static ComplexF operator-( ComplexF a, ComplexF b ) {
		a.Re	= a.Re - b.Re;
		a.Im	= a.Im - b.Im;
		return a;
	}

	/// <summary>
	/// Multiply a complex number by a real
	/// </summary>
	/// <param name="a"></param>
	/// <param name="f"></param>
	/// <returns></returns>
	public static ComplexF operator*( ComplexF a, float f ) {
		a.Re	= a.Re * f;
		a.Im	= a.Im * f;
		return a;
	}


	/// <summary>
	/// Multiply two complex numbers together
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static ComplexF operator*( ComplexF a, ComplexF b ) {
		// (x + yi)(u + vi) = (xu – yv) + (xv + yu)i. 
		float	x = a.Re, y = a.Im;
		float	u = b.Re, v = b.Im;
		a.Re	= x * u - y * v;
		a.Im	= x * v + y * u;
		return a;
	}

	/// <summary>
	/// Divide a complex number by a real number
	/// </summary>
	/// <param name="a"></param>
	/// <param name="f"></param>
	/// <returns></returns>
	public static ComplexF operator/( ComplexF a, float f ) {
		if( f == 0 ) {
			throw new DivideByZeroException();
		}
		a.Re	= a.Re / f;
		a.Im	= a.Im / f;
		return a;
	}

	//----------------------------------------------------------------------------------
}