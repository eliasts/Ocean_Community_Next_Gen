#if NATIVE

using System;
using System.Runtime.InteropServices;
using UnityEngine;


public class uocean  {

#if (UNITY_IOS || UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR

        [DllImport("__Internal")]
		public static extern void UoceanInit( int w, int h, float wx, float wy, float wvspd, float wvscl, float chop, float sx, float sy, float sz, float wvdf);

		[DllImport("__Internal")]
		public static extern void UInit( int w, int h, float wx, float wy, float wvspd, float wvscl, float chop, float sx, float sy, float sz, float wvdf);

		[DllImport("__Internal")]
		public static extern void updVars( float wx, float wy, float wvspd, float wvscl, float chop, float wvdf, bool hh0);

		[DllImport("__Internal")]
		public static extern void InitWaveGenerator();

		[DllImport("__Internal")]
		protected static extern void setMyRandoms(bool enable, int size, IntPtr g1, IntPtr g2);

		[DllImport("__Internal")]
		protected static extern void getFixedTable(IntPtr g1, IntPtr g2);

		[DllImport("__Internal")]
		public static extern void UoceanClear(bool destroy);

		//set the number of threads for native multithreaded functions (android and Linux).
		//for all the other platforms(except webgl and webplayer) set this to >1 to have parallelization. If set to 1 no parallelization will be used.
		[DllImport("__Internal")]
		public static extern void setThreads(int threads);

		[DllImport("__Internal")]
		protected static extern void calcComplex(IntPtr data, IntPtr tx, float time, int ha, int hb);

		[DllImport("__Internal")]
		protected static extern void fft1(IntPtr data);

		[DllImport("__Internal")]
		protected static extern void fft2(IntPtr tx);

		[DllImport("__Internal")]
		protected static extern void calcPhase3b(IntPtr data, IntPtr tx, IntPtr vrt, IntPtr baseH, IntPtr normals, IntPtr tangents, bool rre, IntPtr canCheck, float scaleA);

		[DllImport("__Internal")]
		protected static extern void calcPhase4b(IntPtr vrt, IntPtr tangents, IntPtr floats, bool player, IntPtr pos);

		[DllImport("__Internal")]
		protected static extern void updateTilesA(IntPtr vrtLOD, IntPtr vrt, IntPtr tangentsLOD, IntPtr tangents, IntPtr normalsLOD, IntPtr normals, int L0D, float farLodOffset, IntPtr flodoffset, float flodFact);

		[DllImport("__Internal")]
		protected static extern void getHeightBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport("__Internal")]
		protected static extern void getHeightBatchF(IntPtr data, int size, IntPtr Vec3);

		[DllImport("__Internal")]
		protected static extern void getChoppyBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport("__Internal")]
		protected static extern void getChoppyBatchF(IntPtr data, int size, IntPtr Vec3);

		[DllImport("__Internal")]
		protected static extern void getHeightChoppyBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport("__Internal")]
		protected static extern void getHeightChoppyBatchF(IntPtr data, int size, IntPtr Vec3);
#endif

private const string libname = "ocean";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_WSA


        [DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        ,EntryPoint = "UoceanInit")]
		public static extern void UoceanInit( int w, int h, float wx, float wy, float wvspd, float wvscl, float chop, float sx, float sy, float sz, float wvdf);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif        
        , EntryPoint = "UInit")]
		public static extern void UInit( int w, int h, float wx, float wy, float wvspd, float wvscl, float chop, float sx, float sy, float sz, float wvdf);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "updVars")]
		public static extern void updVars( float wx, float wy, float wvspd, float wvscl, float chop, float wvdf, bool hh0);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "InitWaveGenerator")]
		public static extern void InitWaveGenerator();

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "setMyRandoms")]
		protected static extern void setMyRandoms(bool enable, int size, IntPtr g1, IntPtr g2);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getFixedTable")]
		protected static extern void getFixedTable(IntPtr g1, IntPtr g2);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "UoceanClear")]
		public static extern void UoceanClear(bool destroy);

		//set the number of threads for native multithreaded functions (android and Linux).
		//for all the other platforms(except webgl and webplayer) set this to >1 to have parallelization. If set to 1 no parallelization will be used.
		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "setThreads")]
		public static extern void setThreads(int threads);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "calcComplex")]
        protected static extern void calcComplex(IntPtr data, IntPtr tx, float time, int ha, int hb);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "fft1")]
		protected static extern void fft1(IntPtr data);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "fft2")]
		protected static extern void fft2(IntPtr tx);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "calcPhase3b")]
		protected static extern void calcPhase3b(IntPtr data, IntPtr tx, IntPtr vrt, IntPtr baseH, IntPtr normals, IntPtr tangents, bool rre, IntPtr canCheck, float scaleA);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "calcPhase4b")]
		protected static extern void calcPhase4b(IntPtr vrt, IntPtr tangents, IntPtr floats, bool player, IntPtr pos);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "updateTilesA")]
		protected static extern void updateTilesA(IntPtr vrtLOD, IntPtr vrt, IntPtr tangentsLOD, IntPtr tangents, IntPtr normalsLOD, IntPtr normals, int L0D, float farLodOffset, IntPtr flodoffset, float flodFact);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getHeightBatchV")]
		protected static extern void getHeightBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getHeightBatchF")]
		protected static extern void getHeightBatchF(IntPtr data, int size, IntPtr Vec3);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getChoppyBatchV")]
		protected static extern void getChoppyBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getChoppyBatchF")]
		protected static extern void getChoppyBatchF(IntPtr data, int size, IntPtr Vec3);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getHeightChoppyBatchV")]
		protected static extern void getHeightChoppyBatchV(IntPtr data, int size, IntPtr Vec3);

		[DllImport(libname
        #if (UNITY_STANDALONE_WIN && ENABLE_IL2CPP)
        , CallingConvention = CallingConvention.Cdecl
        #endif
        , EntryPoint = "getHeightChoppyBatchF")]
		protected static extern void getHeightChoppyBatchF(IntPtr data, int size, IntPtr Vec3);


#endif
		//feed it with a Vector3 array where x and z are the input positions. Y will get the Height.
		public static void _getHeightBatch(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getHeightBatchV(dtbuf.AddrOfPinnedObject(), outpos.Length,  obuf.AddrOfPinnedObject());
			dtbuf.Free();  obuf.Free();
		}

		//feed it with a Vector3 array where x and z are the input positions. Y will get the Height. (faster, less accurate.)
		public static void _getHeightBatchF(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getHeightBatchF(dtbuf.AddrOfPinnedObject(), outpos.Length,  obuf.AddrOfPinnedObject());
			dtbuf.Free();  obuf.Free();
		}

		//feed it with a Vector3 array where x and z are the input positions. Y will get the chopiness offset.
		public static void _getChoppyBatch(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getChoppyBatchV(dtbuf.AddrOfPinnedObject(), outpos.Length, obuf.AddrOfPinnedObject());
			dtbuf.Free(); obuf.Free();
		}

		//feed it with a Vector3 array where x and z are the input positions. Y will get the chopiness offset. (faster, less accurate.)
		public static void _getChoppyBatchF(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getChoppyBatchF(dtbuf.AddrOfPinnedObject(), outpos.Length, obuf.AddrOfPinnedObject());
			dtbuf.Free(); obuf.Free();
		}
		
		//get the true height, considering choppy waves, in one go. Faster then having to use the above 2 functions.
		//feed it with a Vector3 array where x and z are the input positions. Y will get the Height considering choppy waves.
		public static void _getHeightChoppyBatch(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getHeightChoppyBatchV(dtbuf.AddrOfPinnedObject(), outpos.Length,  obuf.AddrOfPinnedObject());
			dtbuf.Free(); obuf.Free();
		}

		//get the true height, considering choppy waves, in one go. Faster then having to use the above 2 functions.
		//feed it with a Vector3 array where x and z are the input positions. Y will get the Height considering choppy waves. (faster, less accurate.)
		public static void _getHeightChoppyBatchF(ComplexF[] dt, ref Vector3[] outpos) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle obuf = GCHandle.Alloc(outpos, GCHandleType.Pinned);
			getHeightChoppyBatchF(dtbuf.AddrOfPinnedObject(), outpos.Length,  obuf.AddrOfPinnedObject());
			dtbuf.Free(); obuf.Free();
		}

		public static void _calcComplex(ComplexF[] dt, ComplexF[] tx,  float time, int ha, int hb) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle txbuf = GCHandle.Alloc(tx, GCHandleType.Pinned);
			calcComplex(dtbuf.AddrOfPinnedObject(), txbuf.AddrOfPinnedObject(), time, ha, hb);			
			dtbuf.Free(); txbuf.Free();
		}

		public static void _fft1(ComplexF[] dt) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			fft1(dtbuf.AddrOfPinnedObject());			
			dtbuf.Free();
		}

		public static void _fft2(ComplexF[] tx) {
			GCHandle txbuf = GCHandle.Alloc(tx, GCHandleType.Pinned);
			fft2(txbuf.AddrOfPinnedObject());			
			txbuf.Free();
		}

		public static void _calcPhase3(ComplexF[] dt, ComplexF[] tx,  Vector3[] vrt, Vector3[] baseH, Vector3[] normals, Vector4[] tangents, bool rre, byte[] canCheck, float scaleA) {
			GCHandle dtbuf = GCHandle.Alloc(dt, GCHandleType.Pinned);
			GCHandle txbuf = GCHandle.Alloc(tx, GCHandleType.Pinned);
			GCHandle vbuf = GCHandle.Alloc(vrt, GCHandleType.Pinned);
			GCHandle bbuf = GCHandle.Alloc(baseH, GCHandleType.Pinned);
			GCHandle nbuf = GCHandle.Alloc(normals, GCHandleType.Pinned);
			GCHandle tbuf = GCHandle.Alloc(tangents, GCHandleType.Pinned);
			GCHandle cbuf = GCHandle.Alloc(canCheck, GCHandleType.Pinned);
			calcPhase3b(dtbuf.AddrOfPinnedObject(), txbuf.AddrOfPinnedObject(), vbuf.AddrOfPinnedObject(), bbuf.AddrOfPinnedObject(), nbuf.AddrOfPinnedObject(), tbuf.AddrOfPinnedObject(), rre, cbuf.AddrOfPinnedObject(), scaleA );		
			dtbuf.Free(); txbuf.Free(); vbuf.Free(); bbuf.Free(); nbuf.Free(); tbuf.Free(); cbuf.Free();
		}

		public static void _calcPhase4b(Vector3[] vrt, Vector4[] tangents, float[] floats, bool player, Vector3[] pos) {
			GCHandle vbuf = GCHandle.Alloc(vrt, GCHandleType.Pinned);
			GCHandle tbuf = GCHandle.Alloc(tangents, GCHandleType.Pinned);
			GCHandle fbuf = GCHandle.Alloc(floats, GCHandleType.Pinned);
			GCHandle pbuf = GCHandle.Alloc(pos, GCHandleType.Pinned);
			calcPhase4b( vbuf.AddrOfPinnedObject(), tbuf.AddrOfPinnedObject(), fbuf.AddrOfPinnedObject(), player, pbuf.AddrOfPinnedObject() );		
			vbuf.Free(); tbuf.Free(); fbuf.Free(); pbuf.Free();
		}

		public static void _updateTilesA(Vector3[] vrtLOD,  Vector3[] vrt, Vector4[] tangentsLOD,  Vector4[] tangents,Vector3[] normalsLOD, Vector3[]  normals, int L0D, float farLodOffset, float[] flodoffset, float flodFact) {
			GCHandle v1 = GCHandle.Alloc(vrtLOD, GCHandleType.Pinned);
			GCHandle v2 = GCHandle.Alloc(vrt, GCHandleType.Pinned);
			GCHandle t1 = GCHandle.Alloc(tangentsLOD, GCHandleType.Pinned);
			GCHandle t2= GCHandle.Alloc(tangents, GCHandleType.Pinned);
			GCHandle n1 = GCHandle.Alloc(normalsLOD, GCHandleType.Pinned);
			GCHandle n2 = GCHandle.Alloc(normals, GCHandleType.Pinned);
			GCHandle f1 = GCHandle.Alloc(flodoffset, GCHandleType.Pinned);
			updateTilesA(v1.AddrOfPinnedObject(), v2.AddrOfPinnedObject(), t1.AddrOfPinnedObject(), t2.AddrOfPinnedObject(), n1.AddrOfPinnedObject(), n2.AddrOfPinnedObject(), L0D, farLodOffset, f1.AddrOfPinnedObject(), flodFact );		
			v1.Free(); v2.Free(); t1.Free(); t2.Free(); n1.Free(); n2.Free(); f1.Free();
		}

		public static void _setFixedRandomTable(bool enable, int size, float[] g1, float[] g2) {
			GCHandle vg1 = GCHandle.Alloc(g1, GCHandleType.Pinned);
			GCHandle vg2 = GCHandle.Alloc(g2, GCHandleType.Pinned);

			setMyRandoms( enable, size, vg1.AddrOfPinnedObject(), vg2.AddrOfPinnedObject() );		
			vg1.Free(); vg2.Free();
		}

		public static void _getFixedRandomTable(float[] g1, float[] g2) {
			GCHandle vg1 = GCHandle.Alloc(g1, GCHandleType.Pinned);
			GCHandle vg2 = GCHandle.Alloc(g2, GCHandleType.Pinned);

			getFixedTable( vg1.AddrOfPinnedObject(), vg2.AddrOfPinnedObject() );		
			vg1.Free(); vg2.Free();
		}
}
#endif