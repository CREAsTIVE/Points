using Points.Windows.SignalCreation.Presets;

public class SquareWave : IGenerationPreset {
	public string Name => "Меандр";

	IGenerationPreset.Parameter<float> frequency = new(1, "Частота") {
		Validator = (v) => v >= float.Epsilon
	};

	IGenerationPreset.Parameter<float> phase = new(0, "Фаза");

	IGenerationPreset.Parameter<float> amplitude = new(1, "Амплитуда") {
		Validator = (v) => v >= float.Epsilon
	};

	IGenerationPreset.Parameter<float> dutyCycle = new(0.5f, "Скважность (0-1)") {
		Validator = (v) => v > 0 && v < 1
	};

	public IEnumerable<IGenerationPreset.IParameter> Parameters => new List<IGenerationPreset.IParameter>() {
		frequency, phase, amplitude, dutyCycle
	};

	public async IAsyncEnumerable<float> GetPoints(float timeStep, int amount) {
		for (int ix = 0; ix < amount; ix++) {
			float x = ix * timeStep;
			float normalizedTime = (x * frequency.Value - phase.Value) % 1.0f;
			if (normalizedTime < 0) normalizedTime += 1.0f;

			yield return normalizedTime < dutyCycle.Value ? amplitude.Value : -amplitude.Value;
		}
	}
}