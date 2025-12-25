using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalCreation.Presets;

public class Sin : IGenerationPreset {
	public string Name => "Синусоида";

	IGenerationPreset.Parameter<float> frequency = new(1, "Частота") {
		Validator = (v) => v >= float.Epsilon
	};
	IGenerationPreset.Parameter<float> phase = new(1, "Фаза") {
		Validator = (v) => v >= float.Epsilon
	};
	IGenerationPreset.Parameter<float> amplitude = new(1, "Амплитуда") {
		Validator = (v) => v >= float.Epsilon
	};

	public IEnumerable<IGenerationPreset.IParameter> Parameters => new List<IGenerationPreset.IParameter>() {
		frequency, phase, amplitude
	};

	public IEnumerable<float> GetPoints(float timeStep, int amount) {
		for (int ix = 0; ix < amount; ix++) {
			float x = ix * timeStep;

			yield return MathF.Sin((x - phase.Value) * 2 * MathF.PI * frequency.Value) * amplitude.Value;
		}
	}
}
