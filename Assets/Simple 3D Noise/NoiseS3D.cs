using System;
using UnityEngine;

public static class NoiseS3D {
	
	private static int seed_;
	
	/// <summary> 
	///  The seed for the noise function. Randomized at startup by default.
	/// </summary>
	public static int seed {
		get {
			return seed_;
		}
		set {
			seed_ = value;
			UnityEngine.Random.seed = value;
			SetupNoise();
		}
	}
	
	private static int[][] grad3 = {new int[]{1,1,0}, new int[]{-1,1,0}, new int[]{1,-1,0}, new int[]{-1,-1,0},
		new int[]{1,0,1}, new int[]{-1,0,1}, new int[]{1,0,-1}, new int[]{-1,0,-1},
		new int[]{0,1,1}, new int[]{0,-1,1}, new int[]{0,1,-1}, new int[]{0,-1,-1}};
	
	private static int[][] grad4 = {new int[]{0,1,1,1}, new int[]{0,1,1,-1},  new int[]{0,1,-1,1},  new int[]{0,1,-1,-1},
		new int[]{0,-1,1,1},new int[] {0,-1,1,-1},new int[] {0,-1,-1,1},new int[] {0,-1,-1,-1},
		new int[]{1,0,1,1}, new int[]{1,0,1,-1},  new int[]{1,0,-1,1},  new int[]{1,0,-1,-1},
		new int[]{-1,0,1,1},new int[] {-1,0,1,-1},new int[] {-1,0,-1,1},new int[] {-1,0,-1,-1},
		new int[]{1,1,0,1}, new int[]{1,1,0,-1},  new int[]{1,-1,0,1},  new int[]{1,-1,0,-1},
		new int[]{-1,1,0,1},new int[] {-1,1,0,-1},new int[] {-1,-1,0,1},new int[] {-1,-1,0,-1},
		new int[]{1,1,1,0}, new int[]{1,1,-1,0},  new int[]{1,-1,1,0},  new int[]{1,-1,-1,0},
		new int[]{-1,1,1,0},new int[] {-1,1,-1,0},new int[] {-1,-1,1,0},new int[] {-1,-1,-1,0}};
	
	private static int[] p = null;
	
	private static int[] perm_ = null;
	private static int[] perm {
		get {
			if(perm_ == null)
				SetupNoise();
			return perm_;
		}
		set {
			perm_ = value;
		}
	}
	
	private static void SetupNoise() {
		p = new int[256];
		for(int i = 0; i < 256; i++) p[i] = Mathf.FloorToInt(UnityEngine.Random.value * 256);
		
		perm_ = new int[512];
		for(int i = 0; i < 512; i++) perm_[i] = p[i & 255];
	}
	
	
	private static int[][] simplex = {
		new int[]{0,1,2,3}, new int[]{0,1,3,2}, new int[]{0,0,0,0}, new int[]{0,2,3,1}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{1,2,3,0},
		new int[]{0,2,1,3}, new int[]{0,0,0,0}, new int[]{0,3,1,2}, new int[]{0,3,2,1}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{1,3,2,0},
		new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0},
		new int[]{1,2,0,3}, new int[]{0,0,0,0}, new int[]{1,3,0,2}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{2,3,0,1}, new int[]{2,3,1,0},
		new int[]{1,0,2,3}, new int[]{1,0,3,2}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{2,0,3,1}, new int[]{0,0,0,0}, new int[]{2,1,3,0},
		new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0},
		new int[]{2,0,1,3}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{3,0,1,2}, new int[]{3,0,2,1}, new int[]{0,0,0,0}, new int[]{3,1,2,0},
		new int[]{2,1,0,3}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{0,0,0,0}, new int[]{3,1,0,2}, new int[]{0,0,0,0}, new int[]{3,2,0,1}, new int[]{3,2,1,0}};
	
	
	private static int fastfloor(double x) {
		return x > 0 ? (int)x : (int)x - 1;
	}
	private static double dot(int[] g, double x, double y) {
		return g[0] * x + g[1] * y;
	}
	private static double dot(int[] g, double x, double y, double z) {
		return g[0] * x + g[1] * y + g[2] * z;
	}
	private static double dot(int[] g, double x, double y, double z, double w) {
		return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////
	// NOISE GENERATION USING DOUBLES
	////////////////////////////////////////////////////////////////////////////////////////////////
	
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 1D noise field at the given coordinates.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	/// <param name="w">w coordinate parameter for the noise function.</param>
	public static double Noise(double x) {
		return Noise(x, 0);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a 2D value from a noise field at the given coordinates.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	/// <param name="w">w coordinate parameter for the noise function.</param>
	public static double Noise(double x, double y) {
		double n0, n1, n2;
		double F2 = 0.5 * (Math.Sqrt(3.0) - 1.0);
		double s = (x + y) * F2;
		int i = fastfloor(x + s);
		int j = fastfloor(y + s);
		double G2 = (3.0 - Math.Sqrt(3.0)) / 6.0;
		double t = (i + j) * G2;
		double X0 = i - t;
		double Y0 = j - t;
		double x0 = x - X0;
		double y0 = y - Y0;
		
		int i1, j1;
		if(x0 > y0) { i1 = 1; j1 = 0; } else { i1 = 0; j1 = 1; }
		
		double x1 = x0 - i1 + G2;
		double y1 = y0 - j1 + G2;
		double x2 = x0 - 1.0 + 2.0 * G2;
		double y2 = y0 - 1.0 + 2.0 * G2;
		
		int ii = i & 255;
		int jj = j & 255;
		int gi0 = perm[ii + perm[jj]] % 12;
		int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
		int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
		
		double t0 = 0.5 - x0 * x0 - y0 * y0;
		if(t0 < 0) n0 = 0.0;
		else {
			t0 *= t0;
			n0 = t0 * t0 * dot(grad3[gi0], x0, y0);
		}
		double t1 = 0.5 - x1 * x1 - y1 * y1;
		if(t1 < 0) n1 = 0.0;
		else {
			t1 *= t1;
			n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
		}
		double t2 = 0.5 - x2 * x2 - y2 * y2;
		if(t2 < 0) n2 = 0.0;
		else {
			t2 *= t2;
			n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
		}
		
		return 70.0 * (n0 + n1 + n2);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 3D noise field at the given coordinates.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	/// <param name="w">w coordinate parameter for the noise function.</param>
	public static double Noise(double x, double y, double z) {
		double n0, n1, n2, n3;
		
		double F3 = 1.0 / 3.0;
		double s = (x + y + z) * F3;
		int i = fastfloor(x + s);
		int j = fastfloor(y + s);
		int k = fastfloor(z + s);
		double G3 = 1.0 / 6.0;
		double t = (i + j + k) * G3;
		double X0 = i - t;
		double Y0 = j - t;
		double Z0 = k - t;
		double x0 = x - X0;
		double y0 = y - Y0;
		double z0 = z - Z0;
		
		int i1, j1, k1;
		int i2, j2, k2;
		if(x0 >= y0) {
			if(y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } else if(x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
		} else {
			if(y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } else if(x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
		}
		
		double x1 = x0 - i1 + G3;
		double y1 = y0 - j1 + G3;
		double z1 = z0 - k1 + G3;
		double x2 = x0 - i2 + 2.0 * G3;
		double y2 = y0 - j2 + 2.0 * G3;
		double z2 = z0 - k2 + 2.0 * G3;
		double x3 = x0 - 1.0 + 3.0 * G3;
		double y3 = y0 - 1.0 + 3.0 * G3;
		double z3 = z0 - 1.0 + 3.0 * G3;
		
		int ii = i & 255;
		int jj = j & 255;
		int kk = k & 255;
		int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
		int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
		int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
		int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;
		
		double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
		if(t0 < 0) n0 = 0.0;
		else {
			t0 *= t0;
			n0 = t0 * t0 * dot(grad3[gi0], x0, y0, z0);
		}
		double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
		if(t1 < 0) n1 = 0.0;
		else {
			t1 *= t1;
			n1 = t1 * t1 * dot(grad3[gi1], x1, y1, z1);
		}
		double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
		if(t2 < 0) n2 = 0.0;
		else {
			t2 *= t2;
			n2 = t2 * t2 * dot(grad3[gi2], x2, y2, z2);
		}
		double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
		if(t3 < 0) n3 = 0.0;
		else {
			t3 *= t3;
			n3 = t3 * t3 * dot(grad3[gi3], x3, y3, z3);
		}
		
		return 32.0 * (n0 + n1 + n2 + n3);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 4D noise field at the given coordinates.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	/// <param name="w">w coordinate parameter for the noise function.</param>
	public static double Noise(double x, double y, double z, double w) {
		
		double F4 = (Math.Sqrt(5.0) - 1.0) / 4.0;
		double G4 = (5.0 - Math.Sqrt(5.0)) / 20.0;
		double n0, n1, n2, n3, n4;
		
		double s = (x + y + z + w) * F4;
		int i = fastfloor(x + s);
		int j = fastfloor(y + s);
		int k = fastfloor(z + s);
		int l = fastfloor(w + s);
		double t = (i + j + k + l) * G4;
		double X0 = i - t;
		double Y0 = j - t;
		double Z0 = k - t;
		double W0 = l - t;
		double x0 = x - X0;
		double y0 = y - Y0;
		double z0 = z - Z0;
		double w0 = w - W0;
		
		int c1 = (x0 > y0) ? 32 : 0;
		int c2 = (x0 > z0) ? 16 : 0;
		int c3 = (y0 > z0) ? 8 : 0;
		int c4 = (x0 > w0) ? 4 : 0;
		int c5 = (y0 > w0) ? 2 : 0;
		int c6 = (z0 > w0) ? 1 : 0;
		int c = c1 + c2 + c3 + c4 + c5 + c6;
		int i1, j1, k1, l1;
		int i2, j2, k2, l2;
		int i3, j3, k3, l3;
		
		i1 = simplex[c][0] >= 3 ? 1 : 0;
		j1 = simplex[c][1] >= 3 ? 1 : 0;
		k1 = simplex[c][2] >= 3 ? 1 : 0;
		l1 = simplex[c][3] >= 3 ? 1 : 0;
		
		i2 = simplex[c][0] >= 2 ? 1 : 0;
		j2 = simplex[c][1] >= 2 ? 1 : 0;
		k2 = simplex[c][2] >= 2 ? 1 : 0;
		l2 = simplex[c][3] >= 2 ? 1 : 0;
		
		i3 = simplex[c][0] >= 1 ? 1 : 0;
		j3 = simplex[c][1] >= 1 ? 1 : 0;
		k3 = simplex[c][2] >= 1 ? 1 : 0;
		l3 = simplex[c][3] >= 1 ? 1 : 0;
		
		double x1 = x0 - i1 + G4;
		double y1 = y0 - j1 + G4;
		double z1 = z0 - k1 + G4;
		double w1 = w0 - l1 + G4;
		double x2 = x0 - i2 + 2.0 * G4;
		double y2 = y0 - j2 + 2.0 * G4;
		double z2 = z0 - k2 + 2.0 * G4;
		double w2 = w0 - l2 + 2.0 * G4;
		double x3 = x0 - i3 + 3.0 * G4;
		double y3 = y0 - j3 + 3.0 * G4;
		double z3 = z0 - k3 + 3.0 * G4;
		double w3 = w0 - l3 + 3.0 * G4;
		double x4 = x0 - 1.0 + 4.0 * G4;
		double y4 = y0 - 1.0 + 4.0 * G4;
		double z4 = z0 - 1.0 + 4.0 * G4;
		double w4 = w0 - 1.0 + 4.0 * G4;
		int ii = i & 255;
		int jj = j & 255;
		int kk = k & 255;
		int ll = l & 255;
		int gi0 = perm[ii + perm[jj + perm[kk + perm[ll]]]] % 32;
		int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1 + perm[ll + l1]]]] % 32;
		int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2 + perm[ll + l2]]]] % 32;
		int gi3 = perm[ii + i3 + perm[jj + j3 + perm[kk + k3 + perm[ll + l3]]]] % 32;
		int gi4 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1 + perm[ll + 1]]]] % 32;
		
		double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
		if(t0 < 0) n0 = 0.0;
		else {
			t0 *= t0;
			n0 = t0 * t0 * dot(grad4[gi0], x0, y0, z0, w0);
		}
		double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
		if(t1 < 0) n1 = 0.0;
		else {
			t1 *= t1;
			n1 = t1 * t1 * dot(grad4[gi1], x1, y1, z1, w1);
		}
		double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
		if(t2 < 0) n2 = 0.0;
		else {
			t2 *= t2;
			n2 = t2 * t2 * dot(grad4[gi2], x2, y2, z2, w2);
		}
		double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
		if(t3 < 0) n3 = 0.0;
		else {
			t3 *= t3;
			n3 = t3 * t3 * dot(grad4[gi3], x3, y3, z3, w3);
		}
		double t4 = 0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
		if(t4 < 0) n4 = 0.0;
		else {
			t4 *= t4;
			n4 = t4 * t4 * dot(grad4[gi4], x4, y4, z4, w4);
		}
		return 27.0 * (n0 + n1 + n2 + n3 + n4);
	}
	
	private static int octaves_ = 4;
	
	
	/// <summary> 
	/// Number of octaves to use when generating combined noise octaves, higher number of octaves result in less blurry, more cloudy noise;
	/// Value is clamped between 1 and 10. Higher values take longer to compute.
	/// </summary>
	public static int octaves {
		get {
			return octaves_;
		}
		set {
			octaves_ = Mathf.Clamp(value, 1, 10);
		}
	}
	
	/// <summary> 
	/// Value that defines how much higher octaves effect the end result in combined noise generation;
	/// </summary>
	public static float falloff = 0.5f;
	
	////////////////////////////////////////////////////////////////////////////////////////////////
	// OCTAVE METHODS DOUBLE
	////////////////////////////////////////////////////////////////////////////////////////////////
	
	private static double CombineNoise(double[] noiseValues) {
		double finalNoiseValue = 0.0;
		double amplitude = 1.0;
		double totalAmplitude = 0.0;
		
		for(int o = 0; o < octaves; o++) {
			amplitude *= falloff;
			totalAmplitude += amplitude;
			finalNoiseValue += noiseValues[o] * amplitude;
		}
		
		return finalNoiseValue / totalAmplitude;
	}
	
	private static double[] GetNoiseValues(double x, double y, double z, double w, int dimension) {
		double[] noiseValues = new double[octaves];
		double freq = 1.0;
		
		switch(dimension) {
		case 1:
			for(int o = 0; o < octaves; o++) {
				noiseValues[o] = Noise(x * freq);
				freq *= 2.0;
			}
			break;
			
		case 2:
			for(int o = 0; o < octaves; o++) {
				noiseValues[o] = Noise(x * freq, y * freq);
				freq *= 2.0;
			}
			break;
			
		case 3:
			for(int o = 0; o < octaves; o++) {
				noiseValues[o] = Noise(x * freq, y * freq, z * freq);
				freq *= 2.0;
			}
			break;
			
		case 4:
			for(int o = 0; o < octaves; o++) {
				noiseValues[o] = Noise(x * freq, y * freq, z * freq, w * freq);
				freq *= 2.0;
			}
			break;
		}
		
		return noiseValues;
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 1D combined noise field at the given coordinates.
	/// This method uses the octaves variable to generate noise combined over the set number of octaves.
	/// The falloff variable defines how much higher octaves effect the end result.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	public static double NoiseCombinedOctaves(double x) {
		double[] noiseValues = GetNoiseValues(x, 0, 0, 0, 1);
		return CombineNoise(noiseValues);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 2D combined noise field at the given coordinates.
	/// This method uses the octaves variable to generate noise combined over the set number of octaves.
	/// The falloff variable defines how much higher octaves effect the end result.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	public static double NoiseCombinedOctaves(double x, double y) {
		double[] noiseValues = GetNoiseValues(x, y, 0, 0, 2);
		return CombineNoise(noiseValues);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 3D combined noise field at the given coordinates.
	/// This method uses the octaves variable to generate noise combined over the set number of octaves.
	/// The falloff variable defines how much higher octaves effect the end result.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	public static double NoiseCombinedOctaves(double x, double y, double z) {
		double[] noiseValues = GetNoiseValues(x, y, z, 0, 3);
		return CombineNoise(noiseValues);
	}
	
	/// <summary> 
	/// Returns value between -1 and 1 that represents a value from a 4D combined noise field at the given coordinates.
	/// This method uses the octaves variable to generate noise combined over the set number of octaves.
	/// The falloff variable defines how much higher octaves effect the end result.
	/// </summary>
	/// <returns>double</returns>
	/// <param name="x">x coordinate parameter for the noise function.</param>
	/// <param name="y">y coordinate parameter for the noise function.</param>
	/// <param name="z">z coordinate parameter for the noise function.</param>
	/// <param name="w">w coordinate parameter for the noise function.</param>
	public static double NoiseCombinedOctaves(double x, double y, double z, double w) {
		double[] noiseValues = GetNoiseValues(x, y, z, w, 4);
		return CombineNoise(noiseValues);
	}
	
	
	//
	// GPU STUFF
	//
	
	static bool needsFakeBuffer = true;
	
	private static void SetShaderVars(ComputeShader shader, Vector2 noiseOffset, bool normalize, float noiseScale, int kernel) {
		shader.SetInt("octaves", octaves);
		shader.SetFloat("falloff", falloff);
		
		shader.SetInt("normalize", System.Convert.ToInt32(normalize));
		shader.SetFloat("noiseScale", noiseScale);
		shader.SetVector("offset", noiseOffset);
		
		if (needsFakeBuffer) {
			ComputeBuffer cb = new ComputeBuffer(1, 4);
			shader.SetBuffer(kernel, "float1Array", cb);
			shader.SetBuffer(kernel, "float2Array", cb);
			shader.SetBuffer(kernel, "float3Array", cb);
			shader.SetBuffer(kernel, "float4Array", cb);
			cb.Release();
			needsFakeBuffer = false;
		}
	}
	
	private static Texture2D ToTexture2DNoise(this RenderTexture inTex, bool apply = true, bool release = false, TextureFormat format = TextureFormat.RGBA32) {
		Texture2D tex = new Texture2D(inTex.width, inTex.height, format, false);
		RenderTexture.active = inTex;
		tex.ReadPixels(new Rect(0, 0, inTex.width, inTex.height), 0, 0);
		if(release) {
			RenderTexture.active = null;
			inTex.Release();
		}
		if(apply)
			tex.Apply();
		return tex;
	}
	
	private static string shaderPath = "Shaders/NoiseS3DGPU";
	private static string noShaderMsg = "Could not find the noise compute shader. Did you move/rename any of the files?";
	
	/// <summary> 
	/// Uses the GPU to generate a RenderTexture where the pixels in the texture represent noise.
	/// Set the octaves variable before calling this to a desired value.
	/// </summary>
	/// <returns>RenderTexture</returns>
	/// <param name="width"> Width of the texture to generate. </param>
	/// <param name="height"> Height of the texture to generate. </param>
	/// <param name="noiseOffsetX"> X Coordinate of the offset for the noise on the texture. </param>
	/// <param name="noiseOffsetY"> Y Coordinate of the offset for the noise on the texture. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static RenderTexture GetNoiseRenderTexture(int width, int height, float noiseOffsetX = 0, float noiseOffsetY = 0, float noiseScale = 0.01f, bool normalize = true) {
		RenderTexture retTex = new RenderTexture(width, height, 0);
		retTex.enableRandomWrite = true;
		retTex.Create();
		
		ComputeShader shader = Resources.Load(shaderPath) as ComputeShader;
		if(shader == null) {
			Debug.LogError(noShaderMsg);
			return null;
		}
		
		int[] resInts = { width, height};
		
		int kernel = shader.FindKernel("ComputeNoise");
		shader.SetTexture(kernel, "Result", retTex);
		SetShaderVars(shader, new Vector2(noiseOffsetX, noiseOffsetY), normalize, noiseScale, kernel);
		shader.SetInts("reses", resInts);
		
		ComputeBuffer permBuffer = new ComputeBuffer(perm.Length, 4);
		permBuffer.SetData(perm);
		shader.SetBuffer(kernel, "perm", permBuffer);
		
		shader.Dispatch(kernel, Mathf.CeilToInt(width / 16f), Mathf.CeilToInt(height / 16f), 1);
		
		permBuffer.Release();
		
		return retTex;
	}
	
	/// <summary> 
	/// Uses the GPU to generate a Texture2D where the pixels in the texture represent noise.
	/// Set the octaves variable before calling this to a desired value.
	/// </summary>
	/// <returns>Texture2D</returns>
	/// <param name="width"> Width of the texture to generate. </param>
	/// <param name="height"> Height of the texture to generate. </param>
	/// <param name="noiseOffsetX"> X Coordinate of the offset for the noise on the texture. </param>
	/// <param name="noiseOffsetY"> Y Coordinate of the offset for the noise on the texture. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static Texture2D GetNoiseTexture(int width, int height, float noiseOffsetX = 0, float noiseOffsetY = 0, float noiseScale = 0.01f, bool normalize = true) {
		
		RenderTexture renderTex = GetNoiseRenderTexture(width, height, noiseOffsetX, noiseOffsetY, noiseScale, normalize);
		
		Texture2D retTex = renderTex.ToTexture2DNoise(true, true);
		
		return retTex;
	}
	
	/// <summary> 
	/// Uses the GPU to process an array of 1D coordinates for noise and return an array with the noise at the specified coordinates.
	/// </summary>
	/// <returns>Float array</returns>
	/// <param name="positions"> Array of coordinates to process. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static float[] NoiseArrayGPU(float[] positions, float noiseScale = 0.01f, bool normalize = true) {
		ComputeShader shader = Resources.Load(shaderPath) as ComputeShader;
		if(shader == null) {
			Debug.LogError(noShaderMsg);
			return null;
		}
		
		int kernel = shader.FindKernel("ComputeNoiseArray");
		SetShaderVars(shader, Vector2.zero, normalize, noiseScale, kernel);
		shader.SetInts("dimension", 1);
		
		ComputeBuffer permBuffer = new ComputeBuffer(perm.Length, 4);
		permBuffer.SetData(perm);
		shader.SetBuffer(kernel, "perm", permBuffer);
		
		ComputeBuffer posBuffer = new ComputeBuffer(positions.Length, 4);
		posBuffer.SetData(positions);
		shader.SetBuffer(kernel, "float1Array", posBuffer);
		
		ComputeBuffer outputBuffer = new ComputeBuffer(positions.Length, 4);
		shader.SetBuffer(kernel, "outputArray", outputBuffer);
		
		shader.Dispatch(kernel, Mathf.CeilToInt(positions.Length / 16f), 1, 1);
		
		float[] outputData = new float[positions.Length];
		outputBuffer.GetData(outputData);
		
		permBuffer.Release();
		posBuffer.Release();
		outputBuffer.Release();
		
		return outputData;
	}
	
	/// <summary> 
	/// Uses the GPU to process an array of 2D coordinates for noise and return an array with the noise at the specified coordinates.
	/// </summary>
	/// <returns>Float array</returns>
	/// <param name="positions"> Array of coordinates to process. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static float[] NoiseArrayGPU(Vector2[] positions, float noiseScale = 0.01f, bool normalize = true) {
		ComputeShader shader = Resources.Load(shaderPath) as ComputeShader;
		if(shader == null) {
			Debug.LogError(noShaderMsg);
			return null;
		}
		
		int kernel = shader.FindKernel("ComputeNoiseArray");
		SetShaderVars(shader, Vector2.zero, normalize, noiseScale, kernel);
		shader.SetInt("dimension", 2);
		
		ComputeBuffer permBuffer = new ComputeBuffer(perm.Length, 4);
		permBuffer.SetData(perm);
		shader.SetBuffer(kernel, "perm", permBuffer);
		
		ComputeBuffer posBuffer = new ComputeBuffer(positions.Length, 8);
		posBuffer.SetData(positions);
		shader.SetBuffer(kernel, "float2Array", posBuffer);
		
		ComputeBuffer outputBuffer = new ComputeBuffer(positions.Length, 4);
		shader.SetBuffer(kernel, "outputArray", outputBuffer);
		
		shader.Dispatch(kernel, Mathf.CeilToInt(positions.Length / 16f), 1, 1);
		
		float[] outputData = new float[positions.Length];
		outputBuffer.GetData(outputData);
		
		permBuffer.Release();
		posBuffer.Release();
		outputBuffer.Release();
		
		return outputData;
	}
	
	/// <summary> 
	/// Uses the GPU to process an array of 3D coordinates for noise and return an array with the noise at the specified coordinates.
	/// </summary>
	/// <returns>Float array</returns>
	/// <param name="positions"> Array of coordinates to process. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static float[] NoiseArrayGPU(Vector3[] positions, float noiseScale = 0.01f, bool normalize = true) {
		ComputeShader shader = Resources.Load(shaderPath) as ComputeShader;
		if(shader == null) {
			Debug.LogError(noShaderMsg);
			return null;
		}
		
		int kernel = shader.FindKernel("ComputeNoiseArray");
		SetShaderVars(shader, Vector2.zero, normalize, noiseScale, kernel);
		shader.SetInt("dimension", 3);
		
		ComputeBuffer permBuffer = new ComputeBuffer(perm.Length, 4);
		permBuffer.SetData(perm);
		shader.SetBuffer(kernel, "perm", permBuffer);
		
		ComputeBuffer posBuffer = new ComputeBuffer(positions.Length, 12);
		posBuffer.SetData(positions);
		shader.SetBuffer(kernel, "float3Array", posBuffer);
		
		ComputeBuffer outputBuffer = new ComputeBuffer(positions.Length, 4);
		shader.SetBuffer(kernel, "outputArray", outputBuffer);
		
		shader.Dispatch(kernel, Mathf.CeilToInt(positions.Length / 16f), 1, 1);
		
		float[] outputData = new float[positions.Length];
		outputBuffer.GetData(outputData);
		
		permBuffer.Release();
		posBuffer.Release();
		outputBuffer.Release();
		
		return outputData;
	}
	
	/// <summary> 
	/// Uses the GPU to process an array of 4D coordinates for noise and return an array with the noise at the specified coordinates.
	/// </summary>
	/// <returns>Float array</returns>
	/// <param name="positions"> Array of coordinates to process. </param>
	/// <param name="noiseScale"> Value to scale the noise coordinates by. </param>
	/// <param name="normalize"> Whether or not to remap the noise from (-1, 1) to (0, 1). </param>
	public static float[] NoiseArrayGPU(Vector4[] positions, float noiseScale = 0.01f, bool normalize = true) {
		ComputeShader shader = Resources.Load(shaderPath) as ComputeShader;
		if(shader == null) {
			Debug.LogError(noShaderMsg);
			return null;
		}
		
		int kernel = shader.FindKernel("ComputeNoiseArray");
		SetShaderVars(shader, Vector2.zero, normalize, noiseScale, kernel);
		shader.SetInt("dimension", 4);
		
		ComputeBuffer permBuffer = new ComputeBuffer(perm.Length, 4);
		permBuffer.SetData(perm);
		shader.SetBuffer(kernel, "perm", permBuffer);
		
		ComputeBuffer posBuffer = new ComputeBuffer(positions.Length, 16);
		posBuffer.SetData(positions);
		shader.SetBuffer(kernel, "float4Array", posBuffer);
		
		ComputeBuffer outputBuffer = new ComputeBuffer(positions.Length, 4);
		shader.SetBuffer(kernel, "outputArray", outputBuffer);
		
		shader.Dispatch(kernel, Mathf.CeilToInt(positions.Length / 16f), 1, 1);
		
		float[] outputData = new float[positions.Length];
		outputBuffer.GetData(outputData);
		
		permBuffer.Release();
		posBuffer.Release();
		outputBuffer.Release();
		
		return outputData;
	}
	
	
}
