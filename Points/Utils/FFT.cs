using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils;

public class FFTUtils {
	/// <returns>Amplitude values</returns>
	public static IEnumerable<float> ComputeFFT(
		IReadOnlyList<float> signalPointsList
	) {

		// add up to power of 2 (zero-padding)
		int n = signalPointsList.Count;
		int powerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));

		var complexSignal = new Complex[powerOfTwo];
		for (int i = 0; i < n; i++) {
			complexSignal[i] = new Complex(signalPointsList[i], 0);
		}

		// applying window function to filtering
		var window = Window.Hann(powerOfTwo);
		for (int i = 0; i < powerOfTwo; i++) {
			complexSignal[i] *= window[i];
		}

		Fourier.Forward(complexSignal, FourierOptions.Matlab);

		int halfLength = powerOfTwo / 2;
		for (int i = 0; i < halfLength; i++) {
			yield return (float)complexSignal[i].Magnitude;
		}
	}

	public static IEnumerable<(float Frequency, float Amplitude)> ComputeFFTWithFrequencies(
		IReadOnlyList<float> signalPointsList,
		float samplingRate
	) {

		// add up to power of 2 (zero-padding)
		int n = signalPointsList.Count;
		int powerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));

		var complexSignal = new Complex[powerOfTwo];
		for (int i = 0; i < n; i++) {
			complexSignal[i] = new Complex(signalPointsList[i], 0);
		}

		// applying window function
		var window = Window.Hann(powerOfTwo);
		for (int i = 0; i < powerOfTwo; i++) {
			complexSignal[i] *= window[i];
		}

		Fourier.Forward(complexSignal, FourierOptions.Matlab);

		int halfLength = powerOfTwo / 2;

		// Calculate frequency resolution
		float frequencyStep = samplingRate / powerOfTwo;

		for (int i = 0; i < halfLength; i++) {
			float frequency = i * frequencyStep;
			float amplitude = (float)complexSignal[i].Magnitude;

			yield return (frequency, amplitude);
		}
	}
}