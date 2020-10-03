NoiseS3D can be used to create noise in 1 - 4 dimensions

In order to make some noise you need to call one of NoiseS3D's static methods.
For basic noise usage you can call any of these methods to generate noise from coordinates in a given dimension:

//1D
public static double Noise(double x) {}
//2D
public static double Noise(double x, double y) {}
//3D
public static double Noise(double x, double y, double z) {}
//4D
public static double Noise(double x, double y, double z, double w) {}

Example:
double noiseValue = NoiseS3D.Noise(50, 50); // gets noise at coordinates (50, 50)

These methods return doubles for double precision, if you need floats you can cast them like so:
float noiseValue = (float)NoiseS3D.Noise(50, 50); // gets noise at coordinates (50, 50)

NoiseS3D is capable of multiple octave combinations of noise to create fractal brownian noise
using the following methods and variables:

//1D
public static double NoiseCombinedOctaves(double x) {}
//2D
public static double NoiseCombinedOctaves(double x, double y) {}
//3D
public static double NoiseCombinedOctaves(double x, double y, double z) {}
//4D
public static double NoiseCombinedOctaves(double x, double y, double z, double w) {}

Example:
double noiseValue = NoiseS3D.NoiseCombinedOctaves(50, 50); // gets fractal noise at coordinates (50, 50)

The variables NoiseS3D.octaves and NoiseS3D.falloff are directly used in these methods.
Changing the number of octaves results in more iterations that takes longer to compute but
result in finer more detailed/sharper fractal noise.
Falloff controls how much the subsequent octaves effect the final result.

The variable NoiseS3D.seed is used to set the seed of the noise generation
Setting this to the same value before calling any of the noise methods will result in the same
noise being generated every time.

NoiseS3D also has some GPU accelerated operation modes for working with large datasets.
(Keeping in mind these run in a compute shader that can only run on modern-ish hardware)
The methods for such modes are as follows:

//returns a rendertexture filled with noise, effected by the settings passed to it
public static RenderTexture GetNoiseRenderTexture(int width, int height, float noiseOffsetX = 0, float noiseOffsetY = 0, float noiseScale = 0.01f, bool normalize = true) {}

//basically the same as the previous method except converts it to Texture2D before returning
//this makes it take slightly longer
public static Texture2D GetNoiseTexture(int width, int height, float noiseOffsetX = 0, float noiseOffsetY = 0, float noiseScale = 0.01f, bool normalize = true) {}

These methods you can pass an array of positions in 1 - 4 dimensions and have the GPU churn through them:

//1D
public static float[] NoiseArrayGPU(float[]   positions, float noiseScale = 0.01f, bool normalize = true) {}
//2D
public static float[] NoiseArrayGPU(Vector2[] positions, float noiseScale = 0.01f, bool normalize = true) {}
//3D
public static float[] NoiseArrayGPU(Vector3[] positions, float noiseScale = 0.01f, bool normalize = true) {}
//4D
public static float[] NoiseArrayGPU(Vector4[] positions, float noiseScale = 0.01f, bool normalize = true) {}

Example:
Vector2[] someInputData = new Vector2[1000];
//initialize input data here
float[] noiseData = NoiseArrayGPU(someInputData);

To use combined noise to create fractal noise on GPU, you must set the NoiseS3D.octaves variable BEFORE calling the 
GPU based methods. Same goes for NoiseS3D.falloff and NoiseS3D.seed.
If NoiseS3D.octaves is set to 1, it will be treated as if not combined, greater than one and it will create fractal noise.
Computing more octaves is much less costly on GPU.


